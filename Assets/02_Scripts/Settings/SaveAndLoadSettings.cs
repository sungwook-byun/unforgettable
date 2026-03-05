using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SaveAndLoadSettings : MonoBehaviour {
    [Header("Save File Button")]
    [SerializeField] private Button autoSaveButton;
    [SerializeField] private Button saveFirstButton;
    [SerializeField] private Button saveSecondButton;
    [SerializeField] private Button saveThirdButton;

    [Header("Background UI")]
    [SerializeField] private GameObject saveLoadBackground;

    [Header("Fade Out Animation Name")]
    [SerializeField] private string fadeOutClipName = "Save&Load FadeOut"; // 애니메이션 이름 지정

    public static bool IsOpenedPanel { get; private set; } = false;

    private Animator animator;
    private bool isClosing = false; // 중복 입력 방지용

    void Awake() {
        autoSaveButton.onClick.AddListener(() => SetSaveLoad("AutoSave"));
        saveFirstButton.onClick.AddListener(() => SetSaveLoad("SaveSlot1"));
        saveSecondButton.onClick.AddListener(() => SetSaveLoad("SaveSlot2"));
        saveThirdButton.onClick.AddListener(() => SetSaveLoad("SaveSlot3"));

        animator = GetComponent<Animator>();
    }

    void OnEnable() {
        IsOpenedPanel = true;
        isClosing = false;
    }

    void OnDisable() {
        IsOpenedPanel = false;
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Escape) && !isClosing) {
            StartCoroutine(PlayFadeOutAndClose());
        }
    }

    private IEnumerator PlayFadeOutAndClose() {
        isClosing = true;
        animator.Play(fadeOutClipName);

        // 현재 실행 중인 State 정보 가져오기
        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
        float clipLength = state.length;

        // 애니메이션 길이만큼 대기
        yield return new WaitForSeconds(clipLength);

        // 비활성화 처리
        gameObject.SetActive(false);
        isClosing = false;
        saveLoadBackground.SetActive(false);
    }

    private void SetSaveLoad(string slotName) {
        Debug.Log($"세이브 파일 선택됨: {slotName}");
    }
}
