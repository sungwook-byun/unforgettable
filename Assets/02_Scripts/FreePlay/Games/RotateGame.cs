using UnityEngine;
using UnityEngine.UI;

public class RotateGame : MiniGameBase
{
    [Header("Models")]
    [SerializeField] Transform targetModel;   // 정답
    [SerializeField] Transform playerModel;   // 플레이어
    [SerializeField] GameObject[] models;
    [SerializeField] ParticleSystem hintParticle;

    [Header("UI")]
    [SerializeField] Button resetButton;
    [SerializeField] Slider xSlider;
    [SerializeField] Slider ySlider;
    [SerializeField] Slider zSlider;

    [Header("Settings")]
    public float winAngleThreshold = 10f;  // 허용 오차(도)
    float[] anglePerLevel = { 45, 120, 180 };

    bool playing, rotating, playingHint;

    void Start()
    {
        // 모델 초기화
        targetModel.rotation = Quaternion.identity;
        playerModel.rotation = Quaternion.identity;
        GameObject ran = models[Random.Range(0, models.Length)];
        Instantiate(ran, targetModel);
        Instantiate(ran, playerModel);

        SetupSliders();

        // 리셋버튼 초기화
        resetButton.onClick.AddListener(OnClickResetButton);
        resetButton.interactable = false;
    }

    void OnClickResetButton()
    {
        playerModel.rotation = Quaternion.identity;
        xSlider.value = 0.5f;
        ySlider.value = 0.5f;
        zSlider.value = 0.5f;
    }

    public override void Setup(Score s, TimerController t, InputReader i)
    {
        base.Setup(s, t, i);

        // 정답 랜덤 회전
        RandomizeTargetRotation();
    }

    void SetupSliders()
    {
        // 슬라이더 초기화
        xSlider.onValueChanged.AddListener(OnSliderChanged);
        ySlider.onValueChanged.AddListener(OnSliderChanged);
        zSlider.onValueChanged.AddListener(OnSliderChanged);
        xSlider.interactable = false;
        ySlider.interactable = false;
        zSlider.interactable = false;

        // 초기값: 중앙
        xSlider.value = 0.5f;
        ySlider.value = 0.5f;
        zSlider.value = 0.5f;
    }

    void OnSliderChanged(float _)
    {
        if (playerModel == null) return;

        // 슬라이더(0~1) → -180~180 변환
        float xRot = (xSlider.value - 0.5f) * 360f;
        float yRot = (ySlider.value - 0.5f) * 360f;
        float zRot = (zSlider.value - 0.5f) * 360f;

        playerModel.rotation = Quaternion.Euler(xRot, yRot, zRot);
        if(!rotating) rotating = true;

        float angleDiff = Quaternion.Angle(playerModel.rotation, targetModel.rotation);
        if (angleDiff <= winAngleThreshold)
        {
            if (!playingHint)
            {
                playingHint = true;
                hintParticle.Play();
            }
        }
        else
        {
            if (playingHint)
            {
                playingHint = false;
                hintParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                //hintParticle.Stop();
            }
        }
    }

    private void Update()
    {
        if (playing && rotating && timer.Running && !timer.Paused)
        {
            if (input.PointerUp) CheckMatch();
        }
    }

    void RandomizeTargetRotation()
    {
        float angle = anglePerLevel[MiniGameContext.gameLevel];
        Vector3 randomEuler = Vector3.zero;
        int it = 0;
        while(Quaternion.Angle(Quaternion.identity, Quaternion.Euler(randomEuler)) <= winAngleThreshold)
        {
            randomEuler = new Vector3(
                Random.Range(-angle, angle),
                Random.Range(-angle, angle),
                Random.Range(-angle, angle));
            it++;
            if (it > 10) break;
        }

        targetModel.rotation = Quaternion.Euler(randomEuler);
    }

    void CheckMatch()
    {
        if (!playing || !timer.Running || timer.Paused) return;

        float angleDiff = Quaternion.Angle(playerModel.rotation, targetModel.rotation);
        if (angleDiff <= winAngleThreshold)
        {
            float accuracy = Mathf.Max(0f, winAngleThreshold - angleDiff);
;
            score.Add((int)((100 * accuracy) + (timer.Remaining * 50)));
            clear = true;
            Finish();
        }
    }

    void HandleFinish(int finalScore, bool clear)
    {
        xSlider.interactable = false;
        ySlider.interactable = false;
        zSlider.interactable = false;
        resetButton.interactable = false;
    }

    public override void Begin()
    {
        playing = true;
        xSlider.interactable = true;
        ySlider.interactable = true;
        zSlider.interactable = true;
        resetButton.interactable = true;
        OnFinished += HandleFinish;
    }

    private void OnDestroy()
    {
        OnFinished -= HandleFinish;
    }
}
