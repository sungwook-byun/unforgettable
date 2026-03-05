using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ControlSettings : MonoBehaviour {
    [Header("Move Control Button")]
    [SerializeField] private Button forwardButton;
    [SerializeField] private Button backwardButton;
    [SerializeField] private Button leftButton;
    [SerializeField] private Button rightButton;

    [Header("Interact Control Button")]
    [SerializeField] private Button interactButton;
    [SerializeField] private Button cancelButton;

    [Header("All Key Sprites (name = key string)")]
    [SerializeField] private Sprite[] allKeySprites;

    private Button currentButton;
    private TextMeshProUGUI currentButtonText;
    private Image currentButtonImage;
    private bool waitingForInput = false;

    public static bool IsWaitingForInput { get; private set; } = false;

    private void Awake() {
        // 버튼 클릭 시 StartKeyChange 호출
        forwardButton.onClick.AddListener(() => StartKeyChange(forwardButton));
        backwardButton.onClick.AddListener(() => StartKeyChange(backwardButton));
        leftButton.onClick.AddListener(() => StartKeyChange(leftButton));
        rightButton.onClick.AddListener(() => StartKeyChange(rightButton));
        interactButton.onClick.AddListener(() => StartKeyChange(interactButton));
        cancelButton.onClick.AddListener(() => StartKeyChange(cancelButton));
    }

    private void StartKeyChange(Button button) {
        if (waitingForInput) return;

        waitingForInput = true;
        IsWaitingForInput = true; // 추가

        currentButton = button;
        currentButtonImage = button.GetComponent<Image>();
        currentButtonText = button.GetComponentInChildren<TextMeshProUGUI>();

        SetImageAlpha(currentButtonImage, 0f);

        currentButtonText.text = "아무 키를 입력해주세요...";
        StartCoroutine(FadeText(currentButtonText, 1f));

        StartCoroutine(WaitForKeyInput());
    }

    private IEnumerator FadeText(TextMeshProUGUI text, float duration) {
        while (waitingForInput) {
            // Fade In
            for (float t = 0f; t < 1f; t += Time.deltaTime / (duration / 2f)) {
                SetTextAlpha(text, t);
                yield return null;
            }
            // Fade Out
            for (float t = 1f; t > 0f; t -= Time.deltaTime / (duration / 2f)) {
                SetTextAlpha(text, t);
                yield return null;
            }
        }
        SetTextAlpha(text, 1f); // 종료 시 완전히 표시
    }

    private void SetTextAlpha(TextMeshProUGUI text, float alpha) {
        if (text == null) return;
        Color c = text.color;
        c.a = Mathf.Clamp01(alpha);
        text.color = c;
    }

    private IEnumerator WaitForKeyInput() {
        // Enter 키 버퍼 초기화
        yield return null; // 한 프레임 대기

        while (waitingForInput) {
            if (Input.anyKeyDown) {
                foreach (KeyCode key in System.Enum.GetValues(typeof(KeyCode))) {
                    if (Input.GetKeyDown(key)) {
                        if (key == KeyCode.Return) continue; // Enter 무시
                        ApplyKeyChange(key.ToString().ToLower());
                        yield break;
                    }
                }
            }
            yield return null;
        }
    }

    private void ApplyKeyChange(string keyName) {
        keyName = keyName.ToLower();

        // 1. 중복 체크
        if (IsKeyAlreadyAssigned(keyName)) {
            // 중복 키 입력 처리
            currentButtonText.text = "이미 사용 중인 키입니다.";
            currentButtonText.color = Color.red;
            SetImageAlpha(currentButtonImage, 0f);
            StartCoroutine(RestoreButtonAfterDelay(2f));
            return;
        }

        // 2. Sprite 찾기
        Sprite found = null;
        foreach (var s in allKeySprites) {
            if (s.name.ToLower() == keyName) {
                found = s;
                break;
            }
        }

        if (found != null) {
            currentButtonImage.sprite = found;
            currentButtonText.text = "";
            SetImageAlpha(currentButtonImage, 1f);
            EndKeyChange();
        } else {
            // 입력할 수 없는 키 처리
            currentButtonText.text = "입력할 수 없는 키입니다.";
            currentButtonText.color = Color.red;
            SetImageAlpha(currentButtonImage, 0f);
            StartCoroutine(RestoreButtonAfterDelay(2f));
        }
    }

    private IEnumerator RestoreButtonAfterDelay(float delay) {
        yield return new WaitForSeconds(delay);

        // 이미지 원래대로 보이게
        SetImageAlpha(currentButtonImage, 1f);

        // 텍스트 원래 색상 및 내용 복원
        currentButtonText.color = Color.white;
        currentButtonText.text = ""; // 필요시 버튼 기본 텍스트로 바꿀 수 있음

        EndKeyChange();
    }

    private void EndKeyChange() {
        waitingForInput = false;
        IsWaitingForInput = false; // 추가
    }

    private void SetImageAlpha(Image image, float alpha) {
        if (image == null) return;
        Color color = image.color;
        color.a = Mathf.Clamp01(alpha);
        image.color = color;
    }

    private bool IsKeyAlreadyAssigned(string keyName) {
        keyName = keyName.ToLower();
        // 모든 버튼 체크
        Button[] allButtons = { forwardButton, backwardButton, leftButton, rightButton, interactButton, cancelButton };
        foreach (var btn in allButtons) {
            if (btn == currentButton) continue; // 현재 버튼 제외

            Image img = btn.GetComponent<Image>();
            if (img != null && img.sprite != null && img.sprite.name.ToLower() == keyName) {
                return true; // 동일한 키가 이미 다른 버튼에 할당됨
            }
        }
        return false;
    }
}
