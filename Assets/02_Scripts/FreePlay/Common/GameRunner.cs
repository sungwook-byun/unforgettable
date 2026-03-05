using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameRunner : MonoBehaviour
{
    [SerializeField] MiniGameDatabase database;
    GameObject miniGamePrefab;

    [SerializeField] InputReader input;
    [SerializeField] Score score;
    [SerializeField] TimerController timer;
    [SerializeField] TextMeshProUGUI countdownText;
    [SerializeField] GameObject loading;

    MiniGameBase game;

    bool useTimer, counting;
    float timeLimit;
    float countdown = 3f;

    [Header("Start Panel")]
    [SerializeField] StartPanel startPanel;

    [Header("Pause Panel")]
    [SerializeField] PausePanel pausePanel;

    [Header("Result Panel")]
    [SerializeField] GameObject resultPanel;
    [SerializeField] Button lobbyButton_result, retryButton_result;
    [SerializeField] TextMeshProUGUI scoreText, titleText_result;
    [SerializeField] RectTransform resultBox, clearEffect, clearConfetti, clearCrown;
    [SerializeField] StarTween[] stars;

    [SerializeField] AudioClip gameBgm, countdownSfx, startSfx, finishSfx, overSfx;

    IEnumerator Start()
    {
        AudioManager.Instance.StopBGM();

        loading.SetActive(true);

        // DB에서 이번 미니게임 정보 가져오기
        MiniGameData data = database.miniGames[MiniGameContext.gameIndex];
        miniGamePrefab = data.gamePrefab;
        timeLimit = data.timeLimit;
        countdown = data.countDown;

        // 미니게임 인스턴스
        game = Instantiate(miniGamePrefab).GetComponent<MiniGameBase>();
        //game.gameObject.SetActive(false);
        yield return null;
        
        titleText_result.text = data.title;
        
        startPanel.SetPanel(data,
            onSelectLevel: (level) => OnClickLevelButton(level),
            onLobby: () => OnClickLobbyButton());

        pausePanel.SetPanel(
            onPause: () => PauseTimer(),
            onRetry: () => OnClickRetryButton(),
            onLobby: () => OnClickLobbyButton());

        lobbyButton_result.onClick.AddListener(OnClickLobbyButton);
        retryButton_result.onClick.AddListener(OnClickRetryButton);

        // 타이머 쓸지 여부 결정
        useTimer = timeLimit > 0f;

        // 타이머 이벤트 연결
        if (useTimer)
        {
            // 시간이 다 됐을 때 게임 종료 처리
            timer.OnTimeUp += HandleTimeUp;
        }
        else
        {
            // 무제한 모드면 HUD 숨김
            timer.HideHUD();
        }

        loading.SetActive(false);
    }

    void OnClickLevelButton(int level)
    {
        MiniGameContext.gameLevel = level;
        startPanel.gameObject.SetActive(false);
        StartCoroutine(Countdown());
    }

    void OnClickLobbyButton()
    {
        Time.timeScale = 1;
        //SceneManager.LoadScene("FreePlayLobbyScene");
        GameSceneManager.Instance.LoadSceneWithLoading(SceneNames.FreePlayLobby);
    }

    void OnClickRetryButton()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("FreePlayGameScene");
        //GameSceneManager.Instance.LoadSceneWithLoading(SceneNames.FreePlayGame);
    }

    IEnumerator Countdown()
    {
        counting = true;
        game.Setup(score, timer, input);
        game.OnFinished += OnFinished;

        // 카운트다운 연출
        timer.SetPaused(true, false);
        for (int i = (int)countdown; i > 0; i--)
        {
            countdownText.text = i.ToString();
            AudioManager.Instance.PlaySFX(countdownSfx);
            yield return new WaitForSeconds(1f);
        }
        countdownText.gameObject.SetActive(false);
        AudioManager.Instance.PlaySFX(startSfx);
        counting = false;
        // 카운트다운 끝 → 게임 시작!
        timer.SetPaused(false);
        if (useTimer)
        {
            timer.StartTimer(timeLimit);
        }

        AudioManager.Instance.PlayBGM(gameBgm, true);
        yield return null;
        game.Begin();
    }

    void Update()
    {
        // ESC로 일시정지/재개
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (startPanel.gameObject.activeInHierarchy) return;
            PauseTimer();
        }
    }

    void PauseTimer()
    {
        if (counting) return;
        bool pause = !timer.Paused;
        timer.SetPaused(pause);
        game.Pause(pause);
        pausePanel.gameObject.SetActive(pause);
    }

    // 타이머가 0이 된 순간 불리는 콜백
    void HandleTimeUp()
    {
        // 1) 게임 일시정지
        timer.StopTimer();

        // 2) 혹시 미니게임이 아직 Finish 안 불렀다면 강제 종료
        game.End();
    }

    void OnFinished(int finalScore, bool clear)
    {
        scoreText.text = "내 점수 : " + finalScore.ToString();

        // ui 초기화
        for (int i = 0; i < stars.Length; i++)
        {
            stars[i].SetEmpty();
        }
        clearEffect.gameObject.SetActive(false);
        clearConfetti.gameObject.SetActive(false);
        clearCrown.gameObject.SetActive(false);
        resultPanel.SetActive(true);

        RectTransform resultRT = resultBox.GetComponent<RectTransform>();
        resultRT.localScale = Vector3.zero;
        resultRT.DOScale(1, 0.5f).SetEase(Ease.OutBack)
                .SetLink(resultRT.gameObject, LinkBehaviour.KillOnDisable | LinkBehaviour.KillOnDestroy)
                .onComplete = () => 
                {
                    AudioManager.Instance.StopBGM();
                    if (clear)
                    {
                        clearConfetti.gameObject.SetActive(true);
                        clearConfetti.localScale = Vector3.zero;
                        clearConfetti.DOScale(1, 0.5f).SetEase(Ease.OutElastic).SetLink(clearConfetti.gameObject, LinkBehaviour.KillOnDisable | LinkBehaviour.KillOnDestroy);

                        clearEffect.gameObject.SetActive(true);
                        clearEffect.DORotate(new Vector3(0, 0, 360), 10f, RotateMode.FastBeyond360).SetEase(Ease.Linear).SetLoops(-1).SetLink(clearEffect.gameObject, LinkBehaviour.KillOnDisable | LinkBehaviour.KillOnDestroy);
                        Image image = clearEffect.GetComponent<Image>();
                        image.DOFade(0.2f, 1f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo).SetLink(image.gameObject, LinkBehaviour.KillOnDisable | LinkBehaviour.KillOnDestroy);

                        clearCrown.gameObject.SetActive(true);
                        clearCrown.localScale = Vector3.zero;
                        clearCrown.DOScale(1, 0.5f).SetEase(Ease.OutBack).SetDelay(0.5f).SetLink(clearCrown.gameObject, LinkBehaviour.KillOnDisable | LinkBehaviour.KillOnDestroy)
                                  .OnComplete(() => 
                                  {
                                      for (int i = 0; i <= MiniGameContext.gameLevel; i++)
                                      {
                                          stars[i].Play();
                                      }
                                  });

                        
                        AudioManager.Instance.PlaySFX(finishSfx);
                    }
                    else AudioManager.Instance.PlaySFX(overSfx);
                };

        
    }
}
