using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.UIElements.UxmlAttributeDescription;

public class PatternGame : MiniGameBase
{
    [Header("Pattern & Line")]
    [Range(2, 9)]
    [SerializeField] int patternLength;
    [SerializeField] Transform[] patternPoints;
    [SerializeField] LineRenderer answerLine, playerLine;
    [SerializeField] float revealDuration = 3f;
    //Vector2Int[] lengthPerLevel = { new Vector2Int(2, 3), new Vector2Int(3, 4), new Vector2Int(4, 5) };
    int[] lengthPerLevel = { 3, 4, 5 };

    [Header("Sfx")]
    [SerializeField] AudioClip successSfx;
    [SerializeField] AudioClip failSfx;


    bool[] used;      // 길이 9
    int[,] skip;      // 10x10 (1-based 접근 편하게)
    int[] workPath;    // [9] 임시 생성용 버퍼
    int[] answerPath;  // [9] 실제 정답 패턴 저장
    int answerLen;

    int poolSize = 100;
    int[,] patternPool;
    int poolCount;

    int[] playerPath;
    int playerLen;

    [Header("UI")]
    //[SerializeField] GameObject[] solvedMarks;
    [SerializeField] RectTransform keyRT;
    [SerializeField] RectTransform[] lockRT;
    [SerializeField] ParticleSystem successParticle;
    [SerializeField] TextMeshProUGUI guideText;
    [SerializeField] Button skipButton;
    int totalPattern = 5;
    int solvedPattern = 0;

    enum GameState { Idle, Showing, Playing }
    GameState state;

    public override void Begin()
    {
        StartCoroutine(RoundRoutine());
    }

    public override void Setup(Score s, TimerController t, InputReader i)
    {
        base.Setup(s, t, i);
        PreGeneratePool();
    }

    private void Awake()
    {
        used = new bool[10];
        skip = new int[10, 10];
        workPath = new int[9];
        answerPath = new int[9];
        playerPath = new int[9];
        patternPool = new int[poolSize, 9];

        InitSkipTable();
        skipButton.onClick.AddListener(OnClickSkipButton);
        skipButton.interactable = false;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //PreGeneratePool();
        state = GameState.Idle;
    }

    IEnumerator RoundRoutine(float wait=0)
    {
        skipButton.interactable = false;

        // 잠시 대기
        yield return new WaitForSeconds(wait);

        // 라운드 시작할 때 모두 초기화
        ClearAllHighlights();
        playerLine.enabled = false;
        playerLine.positionCount = 0;

        // 새 패턴 선택
        PickPatternFromPool();

        // 라인 보여주기
        DrawAnswerPattern();
        answerLine.enabled = true;
        state = GameState.Showing;
        guideText.text = "패턴을 암기하세요!";

        // 카운트다운(패턴 암기 시간)
        yield return new WaitForSeconds(revealDuration);

        // 라인 숨기고 플레이 모드
        answerLine.enabled = false;
        state = GameState.Playing;
        guideText.text = "패턴을 똑같이 그려주세요!";
        skipButton.interactable = true;
    }

    private void Update()
    {
        if (state != GameState.Playing) return;
        if (!timer.Running || timer.Paused) return;

        if (input.PointerDown)
        {
            // 드로잉 시작
            ResetPlayerAttemptVisuals();
            TryAddPlayerNode(GetClosestPointIndex(input.PointerWorldPos));
        }
        else if (input.PointerHold)
        {
            TryAddPlayerNode(GetClosestPointIndex(input.PointerWorldPos));
        }
        else if (input.PointerUp)
        {
            // 플레이어 입력 종료 -> 정답 검사
            bool ok = CheckPlayerPattern();
            ClearAllHighlights();
            if (ok) 
            {
                PlayUnlockTween(lockRT[solvedPattern]);
                solvedPattern++;
                successParticle.Play();

                score.Add(100, true);
                AudioManager.Instance.PlaySFX(successSfx);

                // 플레이어 라인 초기화
                playerLine.enabled = false;
                playerLine.positionCount = 0;

                StopAllCoroutines();

                if(solvedPattern >= totalPattern)
                {
                    clear = true;
                    state = GameState.Idle;
                    Finish();
                }
                else
                {
                    StartCoroutine(RoundRoutine(1));
                }
            }
            else
            {
                if (playerLen > 0)
                {
                    PlayFailTween();
                    score.Miss();
                    AudioManager.Instance.PlaySFX(failSfx);

                    playerLine.enabled = false;
                    playerLine.positionCount = 0;
                }
            }
        }
    }

    void PlayUnlockTween(RectTransform lockRT)
    {
        // 1. 초기화 & 활성화
        keyRT.gameObject.SetActive(true);
        keyRT.anchoredPosition = Vector2.zero;
        keyRT.localScale = Vector3.zero;

        // 목적지 좌표 
        // lockRT (화면 UI 위치) -> keyRT 부모 공간 위치
        RectTransform canvasRT = keyRT.root as RectTransform;

        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(null, lockRT.position);
        Vector2 localPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            keyRT.parent as RectTransform, screenPos, null, out localPos
        );

        Vector2 targetPos = localPos;

        Sequence seq = DOTween.Sequence();

        // 2. 키가 날아가며 커지기 (0.5s)
        //seq.Append(keyRT.DOScale(1f, 0.25f).SetEase(Ease.OutBack).SetLoops(2, LoopType.Yoyo));
        seq.Append(keyRT.DOScale(2f, 0.25f));
        seq.Append(keyRT.DOScale(0f, 0.5f));
        seq.Join(keyRT.DOAnchorPos(targetPos, 0.5f).SetEase(Ease.OutQuad));

        // 3. 도착 시 자물쇠 애니메이션 준비
        seq.AppendCallback(() =>
        {
            // lock의 첫 자식
            RectTransform lockChild = lockRT.GetChild(0) as RectTransform;
            if (lockChild != null)
            {
                lockChild.localRotation = Quaternion.Euler(0, 180, 0);
                lockChild.anchoredPosition = new Vector2(-45, 60);
            }
        });

        // 4. 자물쇠 흔들기 (회전 Z축)
        seq.Append(lockRT.DOLocalRotate(new Vector3(0, 0, 10), 0.15f).SetEase(Ease.InOutSine));
        seq.Append(lockRT.DOLocalRotate(new Vector3(0, 0, -10), 0.15f).SetEase(Ease.InOutSine));
        seq.Append(lockRT.DOLocalRotate(Vector3.zero, 0.1f).SetEase(Ease.OutSine));

        seq.AppendCallback(() => keyRT.gameObject.SetActive(false));
    }

    void PlayFailTween()
    {
        // 흔들림 강도/타임 튜닝 가능
        float duration = 0.25f;
        float strength = 0.25f;

        for (int i = 0; i < playerLen; i++)
        {
            int node = playerPath[i] - 1; // 1..9 → 0..8
            Transform t = patternPoints[node];

            // 노드가 가진 트윈 중복 방지
            t.DOKill();

            // scale reset (혹시 이미 펀치나 효과 먹었으면)
            t.localScale = Vector3.one*0.7f;

            // 흔들기
            t.DOShakeScale(
                duration,
                strength,
                vibrato: 10,
                randomness: 90f,
                fadeOut: true
            ).OnComplete(() => { t.localScale = Vector3.one*0.7f; });
        }
    }

    void OnClickSkipButton()
    {
        // 플레이어 라인 초기화
        playerLine.enabled = false;
        playerLine.positionCount = 0;

        StopAllCoroutines();
        StartCoroutine(RoundRoutine());
    }

    #region Pattern
    private void PreGeneratePool()
    {
        poolCount = 0;
        int safety = 0;
        //patternLength = Random.Range(lengthPerLevel[MiniGameContext.gameLevel].x, lengthPerLevel[MiniGameContext.gameLevel].y + 1);
        patternLength = lengthPerLevel[MiniGameContext.gameLevel];

        while (poolCount < poolSize && safety < poolSize * 20)
        {
            safety++;

            if (GenerateRandomPattern(patternLength))
            {
                for (int i = 0; i < patternLength; i++)
                {
                    patternPool[poolCount, i] = workPath[i];
                }
                poolCount++;
            }
        }
    }

    // 패턴 하나 뽑아서 answerPath에 복사
    private void PickPatternFromPool()
    {
        int idx = Random.Range(0, poolCount);
        answerLen = patternLength;
        for (int i = 0; i < answerLen; i++)
        {
            answerPath[i] = patternPool[idx, i];
        }
    }

    // 안드로이드 패턴 룰의 skip 테이블 초기화
    private bool GenerateRandomPattern(int n)
    {
        if (n < 2) n = 2;
        if (n > 9) n = 9;

        // used 초기화
        for (int i = 1; i <= 9; i++) used[i] = false;

        // 랜덤 시작점
        int start = Random.Range(1, 10);
        workPath[0] = start;
        used[start] = true;

        int depth = 1;
        int guard = 0;

        while (depth < n && guard < 128)
        {
            guard++;

            int nextNode = GetRandomNextNode(workPath[depth - 1]);
            if (nextNode == -1)
            {
                // 막혔으면 다시 리셋해서 새로 시작
                for (int i = 1; i <= 9; i++) used[i] = false;
                start = Random.Range(1, 10);
                workPath[0] = start;
                used[start] = true;
                depth = 1;
                continue;
            }

            workPath[depth] = nextNode;
            used[nextNode] = true;
            depth++;
        }

        // 성공했는지 확인
        return depth == n;
    }

    private int GetRandomNextNode(int last)
    {
        // 후보 모으기
        int cCount = 0;

        int[] candidates = new int[9];

        for (int nxt = 1; nxt <= 9; nxt++)
        {
            if (used[nxt]) continue;
            int req = skip[last, nxt];
            if (req == 0 || used[req])
            {
                candidates[cCount++] = nxt;
            }
        }

        if (cCount == 0) return -1;
        int pick = Random.Range(0, cCount);
        return candidates[pick];
    }

    private void InitSkipTable()
    {
        // 수평
        skip[1, 3] = skip[3, 1] = 2;
        skip[4, 6] = skip[6, 4] = 5;
        skip[7, 9] = skip[9, 7] = 8;
        // 수직
        skip[1, 7] = skip[7, 1] = 4;
        skip[2, 8] = skip[8, 2] = 5;
        skip[3, 9] = skip[9, 3] = 6;
        // 대각
        skip[1, 9] = skip[9, 1] = 5;
        skip[3, 7] = skip[7, 3] = 5;
    }

    private void DrawAnswerPattern()
    {
        answerLine.positionCount = answerLen;
        for (int i = 0; i < answerLen; i++)
        {
            int pIndex = answerPath[i] - 1; 
            answerLine.SetPosition(i, patternPoints[pIndex].position);
        }
    }
    #endregion

    #region PlayerInput
    private const float SNAP_RADIUS_SQR = 0.08f; 

    private int GetClosestPointIndex(Vector3 worldPos)
    {
        float bestDist = SNAP_RADIUS_SQR;
        int bestIdx = -1;

        for (int i = 0; i < 9; i++)
        {
            float d = (patternPoints[i].position - worldPos).sqrMagnitude;
            if (d < bestDist)
            {
                bestDist = d;
                bestIdx = i;
            }
        }

        // 반경 안에 든 점이 없으면 -1
        if (bestIdx == -1)
            return -1;

        return bestIdx + 1;
    }

    // 플레이 중 계속 호출: 새로운 점이면 playerPath에 추가
    private void TryAddPlayerNode(int node)
    {
        if (node < 1 || node > 9) return;

        if (playerLen == 0)
        {
            playerPath[0] = node;
            playerLen = 1;
            HighlightPoint(node);
            UpdatePlayerLine();
            return;
        }

        // 같은 노드를 계속 주입하려는 경우
        int lastNode = playerPath[playerLen - 1];
        if (node == lastNode) return;

        // 이미 추가된 점은 쓸 수 없음
        for (int i = 0; i < playerLen; i++)
        {
            if (playerPath[i] == node) return;
        }

        // 점프 규칙 체크 (같은 안드로이드 규칙 적용)
        int last = playerPath[playerLen - 1];
        int req = skip[last, node];
        if (req != 0 && !PlayerAlreadyHas(req)) return;

        // 추가
        playerPath[playerLen] = node;
        playerLen++;

        HighlightPoint(node);
        UpdatePlayerLine();
    }

    private bool PlayerAlreadyHas(int node)
    {
        for (int i = 0; i < playerLen; i++)
        {
            if (playerPath[i] == node) return true;
        }
        return false;
    }

    private void UpdatePlayerLine()
    {
        playerLine.positionCount = playerLen;

        for (int i = 0; i < playerLen; i++)
        {
            int idx = playerPath[i] - 1;
            playerLine.SetPosition(i, patternPoints[idx].position);
        }

        if (!playerLine.enabled)
            playerLine.enabled = true;
    }

    private void HighlightPoint(int node)
    {
        int idx = node - 1;
        if (idx < 0 || idx >= patternPoints.Length) return;
        if (patternPoints[idx].childCount > 0) patternPoints[idx].GetChild(0).gameObject.SetActive(true);
        patternPoints[idx].DOPunchScale(Vector3.one * 0.5f, 0.3f).SetLink(gameObject, LinkBehaviour.KillOnDisable | LinkBehaviour.KillOnDestroy);
    }

    private void ClearAllHighlights()
    {
        for (int i = 0; i < patternPoints.Length; i++)
        {
            if (patternPoints[i].childCount > 0)
            {
                patternPoints[i].GetChild(0).gameObject.SetActive(false);
            }
        }
    }

    private void ResetPlayerAttemptVisuals()
    {
        playerLen = 0;
        playerLine.enabled = false;
        playerLine.positionCount = 0;

        // 이전 시도에서 켜둔 하이라이트 지우기
        ClearAllHighlights();
    }
    #endregion

    private bool CheckPlayerPattern()
    {
        if (playerLen != answerLen) return false;

        // 정방향 비교
        bool forwardMatch = true;
        for (int i = 0; i < answerLen; i++)
        {
            if (playerPath[i] != answerPath[i])
            {
                forwardMatch = false;
                break;
            }
        }

        if (forwardMatch) return true;

        // 역방향 비교
        for (int i = 0; i < answerLen; i++)
        {
            if (playerPath[i] != answerPath[answerLen - 1 - i])
            {
                return false;
            }
        }
        return true;
    }
}
