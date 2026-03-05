using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnvironmentMenu : MonoBehaviour {
    [System.Serializable]
    public class SettingItem {
        public TextMeshProUGUI label;
        public Button button;
    }

    [Header("환경 설정 항목")]
    [SerializeField] private SettingItem[] items;

    [Header("언어 설정 UI")]
    [SerializeField] private Button leftArrow;
    [SerializeField] private Button rightArrow;

    [Header("UI 강조 색상")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color highlightColor = Color.yellow;

    [Header("입력 간격 설정")]
    [SerializeField] private float initialDelay = 0.3f;
    [SerializeField] private float repeatRate = 0.08f;

    private int currentIndex = 0;
    private float nextUpTime = 0f;
    private float nextDownTime = 0f;

    void Start() {
        UpdateHighlight();
    }

    void OnEnable() {
        currentIndex = 0;
        UpdateHighlight();
    }

    void Update() {
        if (SaveAndLoadSettings.IsOpenedPanel) return; // 저장/불러오기 UI 열려있으면 메뉴 이동 무시
        HandleNavigation();
        HandleSelection();
        HandleAdjust(); // ← 좌우키 처리
    }

    // ▲▼ 메뉴 이동
    void HandleNavigation() {
        // ↑ 키
        if (Input.GetKeyDown(KeyCode.UpArrow)) {
            MoveSelection(-1);
            nextUpTime = Time.time + initialDelay;
        } else if (Input.GetKey(KeyCode.UpArrow) && Time.time >= nextUpTime) {
            MoveSelection(-1);
            nextUpTime = Time.time + repeatRate;
        }

        // ↓ 키
        if (Input.GetKeyDown(KeyCode.DownArrow)) {
            MoveSelection(1);
            nextDownTime = Time.time + initialDelay;
        } else if (Input.GetKey(KeyCode.DownArrow) && Time.time >= nextDownTime) {
            MoveSelection(1);
            nextDownTime = Time.time + repeatRate;
        }
    }

    // Enter로 현재 버튼 실행
    void HandleSelection() {
        if (currentIndex == 2) return; // 언어 설정은 제외
        if (Input.GetKeyDown(KeyCode.Return)) {
            var currentItem = items[currentIndex];
            if (currentItem != null && currentItem.button != null) {
                currentItem.button.onClick.Invoke();
                AudioManager.Instance.PlayUI(0);
            }
        }
    }

    // ← → 입력으로 현재 버튼의 기능만 실행
    void HandleAdjust() {
        if (currentIndex != 2) return;
        if (Input.GetKeyDown(KeyCode.LeftArrow)) {
            if (leftArrow != null) leftArrow.onClick.Invoke();
            AudioManager.Instance.PlayUI(1);
        } else if (Input.GetKeyDown(KeyCode.RightArrow)) {
            if (rightArrow != null) rightArrow.onClick.Invoke();
            AudioManager.Instance.PlayUI(1);
        }
    }

    // 선택 이동
    void MoveSelection(int direction) {
        currentIndex = (currentIndex + direction + items.Length) % items.Length;
        UpdateHighlight();
        AudioManager.Instance.PlayUI(1);
    }

    // 강조 표시 업데이트
    void UpdateHighlight() {
        for (int i = 0; i < items.Length; i++) {
            items[i].label.color = (i == currentIndex) ? highlightColor : normalColor;
        }
    }
}
