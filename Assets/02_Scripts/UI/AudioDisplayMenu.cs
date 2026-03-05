using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AudioDisplayMenu : MonoBehaviour {
    [System.Serializable]
    public class SettingItem {
        public TextMeshProUGUI label;
        public Slider slider;
    }

    [Header("설정 항목")]
    [SerializeField] private SettingItem[] items;

    [Header("UI 강조 색상")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color highlightColor = Color.yellow;

    [Header("조절 단위")]
    [SerializeField] private float step = 0.05f; // 0~1 기준

    [Header("입력 간격 설정")]
    [SerializeField] private float initialDelay = 0.3f; // 꾹 누름 시 첫 반복까지의 지연
    [SerializeField] private float repeatRate = 0.08f;  // 연속 입력 간격

    private int currentIndex = 0;

    // 각 키별 독립 타이머
    private float nextUpTime = 0f;
    private float nextDownTime = 0f;
    private float nextLeftTime = 0f;
    private float nextRightTime = 0f;

    void Start() {
        UpdateHighlight();
    }

    void Update() {
        HandleNavigation();
        HandleAdjustment();
    }

    void OnEnable() {
        // 항상 첫 번째 슬라이더로 초기화
        currentIndex = 0;
        UpdateHighlight();
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

    // ◀▶ 슬라이더 조정
    void HandleAdjustment() {
        // ▶ 키
        if (Input.GetKeyDown(KeyCode.RightArrow)) {
            AdjustSliderValue(step);
            nextRightTime = Time.time + initialDelay;
        } else if (Input.GetKey(KeyCode.RightArrow) && Time.time >= nextRightTime) {
            AdjustSliderValue(step);
            nextRightTime = Time.time + repeatRate;
        }

        // ◀ 키
        if (Input.GetKeyDown(KeyCode.LeftArrow)) {
            AdjustSliderValue(-step);
            nextLeftTime = Time.time + initialDelay;
        } else if (Input.GetKey(KeyCode.LeftArrow) && Time.time >= nextLeftTime) {
            AdjustSliderValue(-step);
            nextLeftTime = Time.time + repeatRate;
        }
    }

    void MoveSelection(int direction) {
        currentIndex = (currentIndex + direction + items.Length) % items.Length;
        UpdateHighlight();
        AudioManager.Instance.PlayUI(1);
    }

    void AdjustSliderValue(float amount) {
        Slider slider = items[currentIndex].slider;

        if (slider == null) return;

        // 이벤트를 그대로 호출하면서 값 조정
        slider.value = Mathf.Clamp(slider.value + amount, slider.minValue, slider.maxValue);
    }

    void UpdateHighlight() {
        for (int i = 0; i < items.Length; i++) {
            items[i].label.color = (i == currentIndex) ? highlightColor : normalColor;
        }
    }
}
