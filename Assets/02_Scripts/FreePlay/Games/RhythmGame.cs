using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RhythmGame : MiniGameBase
{
    [Header("Rhythm")]
    [SerializeField] float barDuration = 2f;
    [SerializeField] int measuresToPlay = 8;
    [SerializeField] float judgePerfectMs = 100f;
    [SerializeField] float judgeGoodMs = 200f;
    [SerializeField] float guideOffsetMs = 250f;   // 가이드 늦게 들리는 지연 보정
    [SerializeField] Image[] noteImages;
    [SerializeField] Image noteBar;
    [SerializeField] TextMeshProUGUI guideText;

    float GuideOffsetSec => guideOffsetMs / 1000f;
    float WPerfect => judgePerfectMs / 1000f;
    float WGood => judgeGoodMs / 1000f;

    float NoteStep => barDuration / (float)measuresToPlay;
    int minHits, maxHits;
    Vector2Int[] hitsPerLevel = { new Vector2Int(1, 3), new Vector2Int(3, 4), new Vector2Int(4, 6) };

    [Header("Audio Scheduling")]
    [SerializeField] AudioSource[] guidePool;
    [SerializeField] AudioSource hitSource, bgmSource;
    [SerializeField] AudioClip guideClip, hitClip;

    [Header("FX")]
    [SerializeField] ParticleSystem perfectFxPrefab;
    [SerializeField] ParticleSystem goodFxPrefab;
    [SerializeField] ParticleSystem missFxPrefab;
    [SerializeField] Animator rocketAnim;
    [SerializeField] Image hitTextImage;
    [SerializeField] Sprite[] hitTextSprites;

    enum Phase { None, Listen, Play, Done }
    Phase phase = Phase.None;

    enum NoteState { Pending, Hit, Missed }

    float[][] patterns;
    float[] currentPattern;         // 0 ~ barDuration (원본)
    List<float> targetTimes = new(); // GuideOffset 적용 후 정렬된 타겟 시간
    List<NoteState> noteStates = new();

    int measureIndex;
    int guidePoolCursor = 0;

    // 기록(원하면 유지)
    List<float> playerTimes = new();

    // 시간
    double gameStartDsp;
    double phaseStartDsp;
    bool paused;
    double pauseStartDsp;
    float phaseElapsedAtPause;


    // 배경 스크롤
    [SerializeField] Renderer scrollBg;
    bool scrolling;
    float scrollSpeed = 0.1f;

    // 라이프
    [SerializeField] RectTransform[] lifeRT;
    int maxLife = 3;
    int life;

    void Start()
    {
        AudioManager.Instance.SetBGMVolume(0);
        clear = true;
        OnFinished += HandleFinish;
    }

    private void OnDestroy()
    {
        AudioManager.Instance.SetBGMVolume(1);
        OnFinished -= HandleFinish;
    }

    void HandleFinish(int score, bool clear)
    {
        phase = Phase.Done;
        scrolling = false;
        bgmSource.Stop();
        StopAllCoroutines();
        for (int i = 0; i < guidePool.Length; i++) guidePool[i].Stop();
    }

    void GameOver()
    {
        clear = false;
        Finish();
    }

    public override void Begin()
    {
        minHits = hitsPerLevel[MiniGameContext.gameLevel].x;
        maxHits = hitsPerLevel[MiniGameContext.gameLevel].y;
        life = maxLife;

        PickNewPattern();

        gameStartDsp = AudioSettings.dspTime + 0.1;
        bgmSource.PlayScheduled(gameStartDsp);

        BeginPhase(Phase.Listen);

        scrolling = true;
    }

    public override void Pause(bool isPaused)
    {
        if (paused == isPaused) return;
        paused = isPaused;

        if (isPaused)
        {
            // 오디오 정지/예약 취소
            if (bgmSource && bgmSource.isPlaying) bgmSource.Pause();
            CancelAllScheduledGuides();

            // 트윈/이펙트 일시정지
            DOTween.PauseAll();

            // DSP 기준 기록(보정용)
            pauseStartDsp = AudioSettings.dspTime;
            phaseElapsedAtPause = (float)(pauseStartDsp - phaseStartDsp);
        }
        else
        {
            // DSP 보정: 멈춰있던 만큼 기준선을 앞으로 민다
            double pausedDuration = AudioSettings.dspTime - pauseStartDsp;
            phaseStartDsp += pausedDuration;
            gameStartDsp += pausedDuration; // 다음 페이즈 계산에서도 일관 

            // 오디오 재개
            if (bgmSource) bgmSource.UnPause();

            // 가이드 재예약
            if (phase == Phase.Listen)
            {
                RescheduleGuideFrom(phaseElapsedAtPause);
            }
        }
    }

    // 가이드 재예약
    void RescheduleGuideFrom(float elapsed)
    {
        if (phase != Phase.Listen) return;             
        if (currentPattern == null || currentPattern.Length == 0) return;

        double baseTime = phaseStartDsp + 0.05;

        // elapsed 이후의 박자만 다시 예약
        for (int k = 0; k < currentPattern.Length; k++)
        {
            float ts = currentPattern[k];
            if (ts < 0f || ts > barDuration) continue;
            if (ts + 1e-4f < elapsed) continue;        // 이미 지난 박자는 스킵

            double at = baseTime + ts;

            // 오디오 가이드
            if (guideClip != null && guidePool != null && guidePool.Length > 0)
            {
                var src = guidePool[guidePoolCursor];
                guidePoolCursor = (guidePoolCursor + 1) % guidePool.Length;
                src.clip = guideClip;
                src.Stop();                             // 혹시 모를 잔여 재생/예약 제거
                src.PlayScheduled(at);
            }

            // 비주얼 펄스
            int idx = TimeToIndex(ts);
            if (ValidImgIndex(idx) && noteImages[idx] != null)
                StartCoroutine(PulseAtDspTime(noteImages[idx], at));
        }
    }

    void CancelAllScheduledGuides()
    {
        StopAllCoroutines();
        if (guidePool == null) return;
        for (int i = 0; i < guidePool.Length; i++)
            if (guidePool[i]) { guidePool[i].Stop(); } 
    }

    void Update()
    {
        if (paused || phase is Phase.Done or Phase.None || currentPattern == null) return;

        if (scrolling)
        {
            Material mat = scrollBg.material;
            Vector2 off = mat.mainTextureOffset;
            off.y -= scrollSpeed * Time.deltaTime;
            mat.mainTextureOffset = off;
        }

        double now = AudioSettings.dspTime;
        float t = (float)(now - phaseStartDsp);

        if (phase == Phase.Listen)
        {
            noteBar.fillAmount = Mathf.Clamp01(t / barDuration);
            if (t >= barDuration) BeginPhase(Phase.Play);
            return;
        }

        if (phase == Phase.Play)
        {
            if (input.JumpDown)
            {
                OnHit(t);
                if (hitClip) hitSource.PlayOneShot(hitClip);
            }

            AutoMissUntil(t);

            noteBar.fillAmount = Mathf.Clamp01(t / barDuration);

            if (t >= barDuration)
            {
                // 남은 타겟 마무리
                AutoMissUntil(float.MaxValue);

                measureIndex++;
                if (measureIndex >= measuresToPlay) { Finish(); }
                else { PickNewPattern(); BeginPhase(Phase.Listen); }
            }
        }
    }

    void BeginPhase(Phase next)
    {
        phase = next;

        double blockDuration = 2.0 * barDuration; // Listen + Play
        double phaseOffset =
            (phase == Phase.Listen) ? 0.0 :
            (phase == Phase.Play) ? barDuration :
            0.0;

        phaseStartDsp = gameStartDsp + (measureIndex * blockDuration) + phaseOffset;

        if (phase == Phase.Listen)
        {
            playerTimes.Clear();
            ScheduleGuide(withAudio: true, withVisual: true);
            guideText.text = "잘 듣고 박자를 기억해요.";
        }
        else if (phase == Phase.Play)
        {
            playerTimes.Clear();
            for (int i = 0; i < noteImages.Length; i++)
                if (noteImages[i]) noteImages[i].enabled = false;
            ScheduleGuide(withAudio: false, withVisual: false); // 플레이 중엔 펄스 생략
            PrepareTargetsForPlay();
            guideText.text = "이제 따라해 볼까요? 스페이스 바를 눌러요!";
        }
    }

    void ScheduleGuide(bool withAudio, bool withVisual)
    {
        double baseTime = phaseStartDsp + 0.05;

        foreach (var ts in currentPattern)
        {
            if (ts < 0f || ts > barDuration) continue;

            double at = baseTime + ts;

            if (withAudio && guideClip != null)
            {
                var src = guidePool[guidePoolCursor];
                guidePoolCursor = (guidePoolCursor + 1) % guidePool.Length;
                src.clip = guideClip;
                src.Stop();
                src.PlayScheduled(at);
            }

            if (withVisual)
            {
                int idx = TimeToIndex(ts);
                if (ValidImgIndex(idx) && noteImages[idx] != null)
                    StartCoroutine(PulseAtDspTime(noteImages[idx], at));
            }
        }
    }

    IEnumerator PulseAtDspTime(Image img, double targetDsp)
    {
        while (AudioSettings.dspTime < targetDsp) yield return null;
        Pulse(img);
    }

    void Pulse(Image img, float upScale = 1.25f, float upTime = 0.08f, float downTime = 0.12f)
    {
        if (!img) return;
        img.transform.DOKill();
        img.transform.localScale = Vector3.one;

        DOTween.Sequence()
            .SetLink(img.gameObject)
            .Append(img.transform.DOScale(upScale, upTime).SetEase(Ease.OutQuad))
            .Append(img.transform.DOScale(1f, downTime).SetEase(Ease.InQuad));
    }

    void PickNewPattern()
    {
        // 모든 패턴 생성
        patterns = RhythmPatternLib.GenerateAllPatterns(NoteStep, barDuration, minHits, maxHits);
        // 랜덤 선택
        currentPattern = patterns[UnityEngine.Random.Range(0, patterns.Length)];

        // 비주얼 슬롯 표시(고정 마커)
        for (int i = 0; i < currentPattern.Length; i++)
        {
            int idx = TimeToIndex(currentPattern[i]);
            if (ValidImgIndex(idx) && noteImages[idx]) noteImages[idx].enabled = true;
        }
    }

    void PrepareTargetsForPlay()
    {
        targetTimes.Clear();
        noteStates.Clear();

        // 가이드 지연 보정 적용 후 타겟 생성/정렬
        for (int i = 0; i < currentPattern.Length; i++)
        {
            float t = currentPattern[i] + GuideOffsetSec;
            if (t >= 0f && t <= barDuration)
            {
                targetTimes.Add(t);
            }
        }
        targetTimes.Sort();

        for (int i = 0; i < targetTimes.Count; i++)
            noteStates.Add(NoteState.Pending);
    }

    void OnHit(float tNow)
    {
        // 가장 가까운 Pending 타겟 탐색
        int bestIdx = -1;
        float bestAbs = float.MaxValue;

        for (int i = 0; i < targetTimes.Count; i++)
        {
            if (noteStates[i] != NoteState.Pending) continue;
            float ad = Mathf.Abs(tNow - targetTimes[i]);
            if (ad < bestAbs)
            {
                bestAbs = ad;
                bestIdx = i;
            }
        }

        if (bestIdx == -1) return;

        if (bestAbs <= WGood)
        {
            noteStates[bestIdx] = NoteState.Hit;
            playerTimes.Add(tNow); // 기록 유지

            if (bestAbs <= WPerfect)
            {
                score.Add(100);
                PlayFxHit(JudgeType.Perfect);
                EmphasizeNote(bestIdx, Color.white, 1.3f);
            }
            else
            {
                score.Add(50);
                PlayFxHit(JudgeType.Good);
                EmphasizeNote(bestIdx, new Color(1f, 1f, 1f, 0.85f), 1.15f);
            }
        }
        else
        {
            LoseLife();
            score.Miss();
            PlayFxHit(JudgeType.Miss);
        }
    }

    void AutoMissUntil(float tNow)
    {
        // Good 윈도우가 지난 Pending 타겟을 Miss로 소모(1회만)
        for (int i = 0; i < targetTimes.Count; i++)
        {
            if (noteStates[i] != NoteState.Pending) continue;

            float tooLate = targetTimes[i] + WGood;
            if (tNow > tooLate)
            {
                noteStates[i] = NoteState.Missed; // ⬅ 소모 처리(중복 방지)
                score.Miss();
                LoseLife();
                PlayFxHit(JudgeType.Miss);
            }
            else
            {
                // 정렬되어 있으므로 이후는 아직 창 안
                break;
            }
        }
    }
    void LoseLife()
    {
        if (phase == Phase.Done) return; // 종료 중복 방지
        life--;
        lifeRT[life].DOScale(0, 0.5f).SetEase(Ease.OutBack);
        if (life <= 0)
        {
            GameOver();
        }
    }

    enum JudgeType { Perfect, Good, Miss }

    void PlayFxHit(JudgeType jt)
    {
        ParticleSystem particle = null;
        Sprite sprite = null;
        switch (jt)
        {
            case JudgeType.Perfect: 
                particle = perfectFxPrefab; 
                rocketAnim.SetTrigger("doPerfect");
                sprite = hitTextSprites[0];
                break;
            case JudgeType.Good: 
                particle = goodFxPrefab;
                sprite = hitTextSprites[1];
                break;
            case JudgeType.Miss: 
                particle = missFxPrefab; 
                rocketAnim.SetTrigger("doMiss");
                sprite = hitTextSprites[2];
                break;
        }
        if (!particle || !sprite) return;

        particle.Play();

        hitTextImage.DOKill();
        hitTextImage.sprite = sprite;
        hitTextImage.SetNativeSize();
        Color c = hitTextImage.color;
        c.a = 0;
        hitTextImage.color = c;
        DOTween.Sequence().SetLink(hitTextImage.gameObject).Append(hitTextImage.DOFade(1f, 0.25f)).AppendInterval(0.5f).Append(hitTextImage.DOFade(0f, 0.25f));
    }

    void EmphasizeNote(int targetIdxSorted, Color tint, float scaleUp)
    {
        int imgIdx = TimeToImageIndexFromTargetIndex(targetIdxSorted);
        if (!ValidImgIndex(imgIdx)) return;

        var img = noteImages[imgIdx];
        if (!img) return;

        var tr = img.transform;
        tr.DOKill();
        tr.localScale = Vector3.one;
        img.DOKill();

        DOTween.Sequence().SetLink(img.gameObject)
            .Append(tr.DOScale(scaleUp, 0.07f).SetEase(Ease.OutQuad))
            .Append(tr.DOScale(1f, 0.10f).SetEase(Ease.InQuad));

        img.DOColor((tint == default ? Color.white : tint), 0.05f)
           .OnComplete(() => img.DOColor(Color.white, 0.12f));
    }

    int TimeToIndex(float t) => Mathf.Clamp((int)Mathf.Round(t / NoteStep), 0, noteImages.Length - 1);
    bool ValidImgIndex(int idx) => 0 <= idx && idx < noteImages.Length;

    int TimeToImageIndexFromTargetIndex(int targetIdxSorted)
    {
        // targetTimes = currentPattern + GuideOffset 후 정렬
        float originalT = targetTimes[targetIdxSorted] - GuideOffsetSec; // 원본 패턴 시간으로 복원
        return TimeToIndex(originalT);
    }
}

public static class RhythmPatternLib
{
    public static float[][] GenerateAllPatterns(float step = 0.25f, float duration = 2.0f, int minHits = 1, int maxHits = 8)
    {
        // 첫 박 제외 필터 
        bool excludeFirstBeat = true;

        int slots = Mathf.FloorToInt((duration - 1e-6f) / step);
        var grid = new float[slots + 1];
        for (int i = 0; i <= slots; i++) grid[i] = i * step;

        // 첫 박(Beat 1) 범위 마스크 계산: 4/4 가정 -> 한 박 길이 = duration/4
        int slotsPerBeat = Mathf.Max(1, Mathf.RoundToInt((duration / 4f) / step));
        int firstBeatMask = (1 << Mathf.Min(slotsPerBeat, slots + 1)) - 1; // 하위 slotsPerBeat 비트

        // 길이(히트수) 버킷(정렬 비용 절약)
        int maxBucket = Mathf.Min(maxHits, slots + 1);
        var buckets = new List<float[]>[maxBucket + 1];
        for (int i = 0; i <= maxBucket; i++) buckets[i] = new List<float[]>();

        int total = 1 << (slots + 1);         // 모든 부분집합
        //var all = new List<float[]>(total);
        for (int mask = 1; mask < total; mask++)
        {
            // 첫 박 제외
            if (excludeFirstBeat && (mask & firstBeatMask) != 0) continue;

            // 히트 수
            int hits = CountBits(mask);
            if (hits < minHits || hits > maxHits) continue;

            // 여기까지 통과한 것만 배열 생성(할당 감소)
            var times = new float[hits];
            int k = 0;
            for (int i = 0, bit = 1; i <= slots; i++, bit <<= 1)
                if ((mask & bit) != 0)
                    times[k++] = grid[i];

            buckets[hits].Add(times);
        }

        // 결과 묶기
        int count = 0;
        for (int h = minHits; h <= maxBucket; h++) count += buckets[h].Count;

        var all = new float[count][];
        int idx = 0;
        for (int h = minHits; h <= maxBucket; h++)
            foreach (var arr in buckets[h]) all[idx++] = arr;

        return all;
    }

    static int CountBits(int x)
    {
        // Hamming weight
        x = x - ((x >> 1) & 0x55555555);
        x = (x & 0x33333333) + ((x >> 2) & 0x33333333);
        return (((x + (x >> 4)) & 0x0F0F0F0F) * 0x01010101) >> 24;
    }
}
