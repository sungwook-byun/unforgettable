using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuActions : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private GameObject mainMenuUI;

    [Header("Main Menu Buttons")]
    [SerializeField] private Button mainPlayButton;
    [SerializeField] private Button freePlayButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button quitButton;

    [Header("UI Fade Objects")]
    [SerializeField] private CanvasGroup backgroundGroup;
    [SerializeField] private CanvasGroup titleGroup;
    [SerializeField] private CanvasGroup subtitleGroup;
    [SerializeField] private CanvasGroup buttonsGroup;

    [Header("Audio Settings")]
    [SerializeField] private AudioClip startMenuBgm;
    [SerializeField] private float fadeDuration = 2f; // Fade-in 시간
    [SerializeField] private float musicFadeDuration = 2f;

    // 추가 - 시작 확인 패널 관련
    [Header("Start Confirm Panel")]
    [SerializeField] private GameObject startConfirmPanel; // 추가 - 패널 오브젝트
    [SerializeField] private Button newGameButton;         // 추가 - 처음부터 버튼
    [SerializeField] private Button continueButton;        // 추가 - 이어하기 버튼
    [SerializeField] private Button cancelButton;          // 추가 - 취소 버튼 (선택)

    private AudioSource bgmSource;

    void Awake()
    {
        mainMenuUI.SetActive(true);
        optionsPanel.SetActive(false);

        // 수정 - 기존 MainPlay 대신 패널 호출 함수 연결
        mainPlayButton.onClick.AddListener(OnMainPlayClicked); // 추가 - 버튼 클릭 시 패널 호출
        freePlayButton.onClick.AddListener(FreePlay);
        optionsButton.onClick.AddListener(OpenOptions);
        quitButton.onClick.AddListener(QuitGame);

        // 추가 - 패널 버튼 연결
        if (newGameButton != null)
            newGameButton.onClick.AddListener(StartNewGame);
        if (continueButton != null)
            continueButton.onClick.AddListener(ContinueGame);
        if (cancelButton != null)
            cancelButton.onClick.AddListener(CloseConfirmPanel);

        mainMenuUI.SetActive(true);
        SetCanvasGroupAlpha(backgroundGroup, 0f);
        SetCanvasGroupAlpha(titleGroup, 0f);
        SetCanvasGroupAlpha(subtitleGroup, 0f);
        SetCanvasGroupAlpha(buttonsGroup, 0f);

        // 추가 - 패널 기본 비활성화
        if (startConfirmPanel != null)
            startConfirmPanel.SetActive(false);
    }

    void Start()
    {
        if (AudioManager.Instance != null)
        {
            Transform bgmTransform = AudioManager.Instance.transform.Find("Bgm Sources");
            bgmSource = bgmTransform?.GetComponent<AudioSource>();

            if (bgmSource == null)
                Debug.LogWarning("Bgm Sources 오브젝트 또는 AudioSource를 찾을 수 없습니다.");
        }
        else
        {
            Debug.LogWarning("AudioManager 인스턴스가 아직 존재하지 않습니다.");
        }

        StartCoroutine(FadeInMenuSequence());
    }

    private IEnumerator FadeInMenuSequence()
    {
        if (TryGetComponent<ButtonGroupActions>(out var buttonManager))
            buttonManager.IsFadeInPlaying = true;

        float t = 0f;
        bgmSource.clip = startMenuBgm;
        bgmSource.volume = 0f;
        bgmSource.Play();

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Clamp01(t / fadeDuration);
            SetCanvasGroupAlpha(backgroundGroup, alpha);
            bgmSource.volume = Mathf.Clamp01(t / musicFadeDuration);
            yield return null;
        }

        SetCanvasGroupAlpha(backgroundGroup, 1f);
        bgmSource.volume = 1f;

        yield return StartCoroutine(FadeCanvasGroup(titleGroup, 1f, fadeDuration / 2f));
        yield return StartCoroutine(FadeCanvasGroup(subtitleGroup, 1f, fadeDuration / 2f));
        yield return StartCoroutine(FadeCanvasGroup(buttonsGroup, 1f, fadeDuration / 2f));

        if (buttonManager != null)
            buttonManager.IsFadeInPlaying = false;
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup group, float targetAlpha, float duration)
    {
        float startAlpha = group.alpha;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            group.alpha = Mathf.Lerp(startAlpha, targetAlpha, t / duration);
            yield return null;
        }
        group.alpha = targetAlpha;
    }

    private void SetCanvasGroupAlpha(CanvasGroup group, float alpha)
    {
        if (group == null) return;
        group.alpha = Mathf.Clamp01(alpha);
    }

    // 추가 - 메인플레이 버튼 클릭 시 확인 패널 표시
    private void OnMainPlayClicked()
    {
        if (startConfirmPanel != null)
        {
            startConfirmPanel.SetActive(true);
            mainMenuUI.SetActive(false); // 패널 띄울때 메인 메뉴 숨기기
        }
        else
        {
            MainPlay(); // 패널이 없을 경우 원래 동작
        }
    }

    // 추가 - 처음부터 버튼
    private void StartNewGame()
    {
        if (SaveManager.Instance != null && SaveManager.Instance.HasSaveData())
        {
            SaveManager.Instance.DeleteSave();
            Debug.Log("기존 세이브 파일 삭제됨");
        }

        AudioManager.Instance.StopBGM();
        GameSceneManager.Instance.LoadSceneWithLoading(SceneNames.GrandmaRoom);
    }

    // 추가 - 이어하기 버튼
    private void ContinueGame()
    {
        if (SaveManager.Instance != null && SaveManager.Instance.HasSaveData())
        {
            SaveData data = SaveManager.Instance.Load();
            if (data != null && !string.IsNullOrEmpty(data.sceneName))
            {
                AudioManager.Instance.StopBGM();
                GameSceneManager.Instance.LoadSceneWithLoading(data.sceneName);
                return;
            }
        }

        // 세이브가 없으면 새 게임 시작
        StartNewGame();
    }

    // 추가 - 취소 버튼
    private void CloseConfirmPanel()
    {
        startConfirmPanel.SetActive(false);
        mainMenuUI.SetActive(true);
    }

    // 버튼 동작
    public void MainPlay()
    {
        AudioManager.Instance.StopBGM();

        // 세이브 매니저가 존재하고 세이브 파일이 있을 때
        if (SaveManager.Instance != null && SaveManager.Instance.HasSaveData())
        {
            SaveData data = SaveManager.Instance.Load();
            if (data != null && !string.IsNullOrEmpty(data.sceneName))
            {
                // 추가 - 기억속세계 마지막 저장 시에는 그대로 해당 씬으로 시작
                if (data.sceneName == SceneNames.TestMemoryWorld)
                {
                    GameSceneManager.Instance.LoadSceneWithLoading(SceneNames.TestMemoryWorld);
                    return;
                }

                Debug.Log($"세이브 데이터 감지됨: {data.sceneName} 씬으로 이동합니다");
                GameSceneManager.Instance.LoadSceneWithLoading(data.sceneName);
                return;
            }
        }

        // 세이브 데이터가 없을 경우 새 게임 시작
        Debug.Log("세이브 데이터가 없어 새 게임을 시작합니다");
        GameSceneManager.Instance.LoadSceneWithLoading(SceneNames.GrandmaRoom);
    }

    public void FreePlay()
    {
        GameSceneManager.Instance.LoadSceneWithLoading("FreePlayLobbyScene");
    }

    public void OpenOptions()
    {
        optionsPanel.SetActive(true);
        mainMenuUI.SetActive(false);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
