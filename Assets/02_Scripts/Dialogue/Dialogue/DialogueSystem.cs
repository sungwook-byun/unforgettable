using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DialogueSystem : Singleton<DialogueSystem> {
    [Header("UI 연결")]
    [SerializeField] private GameObject dialogueUI;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private Image portraitImage;

    [Header("Interact 안내 UI")]
    [SerializeField] private GameObject interactUI;
    [SerializeField] private TextMeshProUGUI interactText;
    [SerializeField] private float interactFadeDuration = 0.3f;

    [Header("Choice UI")]
    [SerializeField] private ChoiceUI choiceUI;

    [Header("Filter UI")]
    [SerializeField] private FilterUI filterUI; // Inspector에서 연결

    [Header("Ohters")]
    [SerializeField] private CanvasGroup fadeInBackground;

    private CanvasGroup interactCanvasGroup;

    private DialogueScriptable currentDialogue;
    private int currentIndex;
    private bool isDialogueActive;
    private Coroutine typingCoroutine;

    private float inputLockTime = 0.2f;
    private float inputLockedUntil = 0f;

    private Action onDialogueEnd;
    private bool skipNextInput = false;

    public bool IsDialogueActive => isDialogueActive;
    public float InputLockedUntil => inputLockedUntil;

    

    protected override void Awake() {
        base.Awake();
        // Interact UI 초기화
        if (interactUI != null) {
            interactCanvasGroup = interactUI.GetComponent<CanvasGroup>();
            if (interactCanvasGroup == null)
                interactCanvasGroup = interactUI.AddComponent<CanvasGroup>();

            interactUI.SetActive(false);
            interactCanvasGroup.alpha = 0f;
        }

        dialogueUI.SetActive(false);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void Start() {
        fadeInBackground.alpha = 0f;
    }

    private void Update() {
        if (!isDialogueActive) return;
        if (Time.time < inputLockedUntil) return;

        // 선택지 UI가 열려 있으면 다음 대사 진행 무시
        if (choiceUI.IsActive()) return;

        if (Input.GetKeyDown(KeyCode.E) || Input.GetMouseButtonDown(0)) {
            if (skipNextInput) {
                skipNextInput = false; // 한번만 무시
                return;
            }

            DialogueData data = currentDialogue.dialogues[currentIndex];

            if (typingCoroutine != null) {
                // 타이핑 중이면 즉시 전체 출력
                StopCoroutine(typingCoroutine);
                dialogueText.text = data.dialogue;
                typingCoroutine = null;

                // 타이핑 끝났으므로 선택지 표시
                if (data.choices != null && data.choices.Length > 0) {
                    choiceUI.ShowChoices(data.choices, OnChoiceSelected);
                }
            } else {
                if (data.choices != null && data.choices.Length > 0) {
                    // 선택지가 아직 없으면 띄우기
                    choiceUI.ShowChoices(data.choices, OnChoiceSelected);
                } else {
                    NextDialogue();
                }
            }
        }
    }

    public void StartFadeIn(float time) {
        fadeInBackground.gameObject.SetActive(true);
        StartCoroutine(FadeIn(time));
    }

    private IEnumerator FadeIn(float time) {
        float elapsed = 0f;
        fadeInBackground.alpha = 0f;

        while (elapsed < time) {
            elapsed += Time.deltaTime;
            fadeInBackground.alpha = Mathf.Clamp01(elapsed / time);
            yield return null;
        }
    }

    private void OnChoiceSelected(int choiceIndex) {
        DialogueData data = currentDialogue.dialogues[currentIndex];
        int nextIndex = data.choices[choiceIndex].nextIndex;
        currentIndex = nextIndex;

        // 선택지에서 E키 입력으로 넘어간 경우, 다음 Update에서 입력 무시
        skipNextInput = true;

        ShowDialogue();
    }

    #region 대화 시작/진행
    public void StartDialogue(DialogueScriptable dialogueData, Action onEndCallback = null) {
        if (dialogueData == null || isDialogueActive) return;

        currentDialogue = dialogueData;
        currentIndex = 0;
        isDialogueActive = true;

        dialogueUI.SetActive(true);
        SetCanvasGroupAlpha(dialogueUI, 1f);

        // 플레이어 이동 금지
        PlayerController2 player = FindFirstObjectByType<PlayerController2>();
        if (player != null) player.SetCanMove(false);

        // Interact UI 숨김
        if (interactUI != null)
            StartCoroutine(FadeOutAndDisable(interactCanvasGroup, interactFadeDuration));

        inputLockedUntil = Time.time + inputLockTime;

        onDialogueEnd = onEndCallback;

        ShowDialogue();
    }

    private void ShowDialogue() {
        if (currentIndex >= currentDialogue.dialogues.Length) {
            EndDialogue();
            return;
        }

        DialogueData data = currentDialogue.dialogues[currentIndex];

        // 필터 적용
        if (filterUI != null) {
            filterUI.SetFilter(data.filterType);
            filterUI.FadeIn(0.3f); // 필요시 페이드
        }

        SpeakerData speaker = currentDialogue.speakers[data.speakerIndex];
        nameText.text = speaker.speakerName;
        descriptionText.text = speaker.description;
        portraitImage.sprite = data.customPortrait != null ? data.customPortrait : speaker.portrait;

        SetAlphaToOpaque(nameText);
        SetAlphaToOpaque(descriptionText);
        SetAlphaToOpaque(dialogueText);

        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeText(data));
    }

    private IEnumerator TypeText(DialogueData data) {
        dialogueText.text = "";
        foreach (char c in data.dialogue) {
            dialogueText.text += c;
            yield return new WaitForSeconds(data.typingSpeed);
        }

        typingCoroutine = null;

        // 대사가 끝난 후 선택지 표시
        if (data.choices != null && data.choices.Length > 0) {
            choiceUI.ShowChoices(data.choices, OnChoiceSelected);
        }
    }

    private void NextDialogue() {
        DialogueData data = currentDialogue.dialogues[currentIndex];

        // 다음 대사가 끝나는 조건 확인
        DialogueData nextData = currentDialogue.dialogues[currentIndex];
        if ((nextData.choices == null || nextData.choices.Length == 0)
            && nextData.isChoiceOnly
            && nextData.nextDialogueIndex == -1) {
            EndDialogue();
            return;
        }

        // 다음 인덱스가 명시되어 있다면
        if (data.nextDialogueIndex >= 0 && data.nextDialogueIndex < currentDialogue.dialogues.Length) {
            currentIndex = data.nextDialogueIndex;
        } else {
            // 순차 진행
            do {
                currentIndex++;
                if (currentIndex >= currentDialogue.dialogues.Length) {
                    EndDialogue();
                    return;
                }
            } while (currentDialogue.dialogues[currentIndex].isChoiceOnly);
        }

        ShowDialogue();
    }

    private void EndDialogue() {
        StartCoroutine(FadeOutAndDisableCanvas(dialogueUI, 0.2f));
        isDialogueActive = false;
        currentDialogue = null;

        PlayerController2 player = FindFirstObjectByType<PlayerController2>();
        if (player != null) player.SetCanMove(true);

        // E키 중복 입력 방지를 위해 입력 잠금 추가
        inputLockedUntil = Time.time + 0.2f;

        // 콜백 실행
        onDialogueEnd?.Invoke();
        onDialogueEnd = null;
    }
    #endregion

    #region Interact UI
    public void ShowInteractUI(string text = "E키를 눌러 대화하기") {
        if (interactUI == null || interactText == null) return;

        interactText.text = text;
        interactUI.SetActive(true);
        StartCoroutine(FadeCanvasGroup(interactCanvasGroup, 0f, 1f, interactFadeDuration));
    }

    public void HideInteractUI() {
        if (interactUI == null) return;
        StartCoroutine(FadeOutAndDisable(interactCanvasGroup, interactFadeDuration));
    }
    #endregion

    #region Utility
    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float start, float end, float duration) {
        float elapsed = 0f;
        while (elapsed < duration) {
            elapsed += Time.deltaTime;
            cg.alpha = Mathf.Lerp(start, end, elapsed / duration);
            yield return null;
        }
        cg.alpha = end;
    }

    private IEnumerator FadeOutAndDisable(CanvasGroup cg, float duration) {
        yield return FadeCanvasGroup(cg, cg.alpha, 0f, duration);
        cg.gameObject.SetActive(false);
    }

    private IEnumerator FadeOutAndDisableCanvas(GameObject obj, float duration) {
        CanvasGroup cg = obj.GetComponent<CanvasGroup>();
        if (cg == null) {
            cg = obj.AddComponent<CanvasGroup>();
            cg.alpha = 1f;
        }
        yield return FadeCanvasGroup(cg, cg.alpha, 0f, duration);
        obj.SetActive(false);
    }

    private void SetAlphaToOpaque(TextMeshProUGUI tmp) {
        Color c = tmp.color;
        c.a = 1f;
        tmp.color = c;
    }

    private void SetCanvasGroupAlpha(GameObject obj, float alpha) {
        CanvasGroup cg = obj.GetComponent<CanvasGroup>();
        if (cg == null) {
            cg = obj.AddComponent<CanvasGroup>();
        }
        cg.alpha = alpha;
    }
    #endregion

    private void OnDestroy() {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        // 씬 진입 시마다 실행됨
        fadeInBackground.alpha = 0f;
        fadeInBackground.gameObject.SetActive(false);
    }
}
