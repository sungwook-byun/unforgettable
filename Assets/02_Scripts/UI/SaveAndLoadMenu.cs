using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SaveAndLoadMenu : MonoBehaviour {
    [Header("버튼 목록")]
    [SerializeField] private Button[] buttons;

    [Header("UI 색상 설정")]
    [SerializeField, Range(0f, 1f)] private float highlightAlpha = 0.33f;
    [SerializeField] private float normalAlpha = 1f;

    [Header("입력 간격 설정")]
    [SerializeField] private float initialDelay = 0.3f;
    [SerializeField] private float repeatRate = 0.08f;

    private int currentIndex = 0;
    private float nextUpTime = 0f;
    private float nextDownTime = 0f;

    void Start() {
        UpdateHighlight();

        // 마우스 Hover 이벤트 연결
        foreach (var btn in buttons) {
            EventTrigger trigger = btn.gameObject.GetComponent<EventTrigger>();
            if (trigger == null)
                trigger = btn.gameObject.AddComponent<EventTrigger>();

            AddEventTrigger(trigger, EventTriggerType.PointerEnter, (eventData) => OnButtonHovered(btn));
        }
    }

    void OnEnable() {
        currentIndex = 0;
        UpdateHighlight();
    }

    void Update() {
        HandleNavigation();
        HandleSelection();
    }

    // ▲▼ 방향키 이동
    void HandleNavigation() {
        if (Input.GetKeyDown(KeyCode.UpArrow)) {
            MoveSelection(-1);
            nextUpTime = Time.time + initialDelay;
        } else if (Input.GetKey(KeyCode.UpArrow) && Time.time >= nextUpTime) {
            MoveSelection(-1);
            nextUpTime = Time.time + repeatRate;
        }

        if (Input.GetKeyDown(KeyCode.DownArrow)) {
            MoveSelection(1);
            nextDownTime = Time.time + initialDelay;
        } else if (Input.GetKey(KeyCode.DownArrow) && Time.time >= nextDownTime) {
            MoveSelection(1);
            nextDownTime = Time.time + repeatRate;
        }
    }

    // Enter 키로 버튼 클릭
    void HandleSelection() {
        if (Input.GetKeyDown(KeyCode.Return)) {
            var currentButton = buttons[currentIndex];
            if (currentButton != null) {
                currentButton.onClick.Invoke();
                AudioManager.Instance.PlayUI(0);
            }
        }
    }

    // 선택 이동
    void MoveSelection(int direction) {
        currentIndex = (currentIndex + direction + buttons.Length) % buttons.Length;
        UpdateHighlight();
        AudioManager.Instance.PlayUI(1);
    }

    // 마우스 Hover 시 호출
    void OnButtonHovered(Button hoveredButton) {
        int index = System.Array.IndexOf(buttons, hoveredButton);
        if (index >= 0 && index != currentIndex) {
            currentIndex = index;
            UpdateHighlight();
            AudioManager.Instance.PlayUI(1);
        }
    }

    // 알파 강조 업데이트
    void UpdateHighlight() {
        for (int i = 0; i < buttons.Length; i++) {
            var image = buttons[i].GetComponent<Image>();
            if (image != null) {
                var color = image.color;
                color.a = (i == currentIndex) ? highlightAlpha : normalAlpha;
                image.color = color;
            }
        }
    }

    // 유틸: EventTrigger 이벤트 추가
    void AddEventTrigger(EventTrigger trigger, EventTriggerType eventType, UnityEngine.Events.UnityAction<BaseEventData> action) {
        EventTrigger.Entry entry = new EventTrigger.Entry { eventID = eventType };
        entry.callback.AddListener(action);
        trigger.triggers.Add(entry);
    }
}
