using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class ButtonGroupActions : MonoBehaviour {
    [SerializeField] private List<HoverButton> buttons = new List<HoverButton>();

    [Header("Button Colors")]
    public Color normalColor = new Color(0.6f, 0.6f, 0.6f);
    public Color hoverColor = Color.white;

    public enum NavigationMode { Arrow, QE, None }

    [Header("Navigation Settings")]
    [SerializeField] private NavigationMode navigationMode = NavigationMode.Arrow;

    [Header("UI Fade Objects")]
    public CanvasGroup buttonsGroup;

    private int selectedIndex = 0;

    public bool IsFadeInPlaying { get; set; } = false; // 페이드 진행 여부

    void Start() {
        if (buttons.Count > 0) {
            Activate(buttons[selectedIndex], true);
            EventSystem.current.SetSelectedGameObject(buttons[selectedIndex].gameObject);
        }
    }

    void OnEnable() {
        if (buttons.Count > 0) {
            Activate(buttons[0], true);
            EventSystem.current.SetSelectedGameObject(buttons[0].gameObject);
        }
    }

    void Update() {
        if (ControlSettings.IsWaitingForInput || IsFadeInPlaying || SaveAndLoadSettings.IsOpenedPanel) return;
        if (buttonsGroup != null && buttonsGroup.alpha < 1f) return; // Fade 진행 중
        HandleKeyboardInput();
    }

    private void HandleKeyboardInput() {
        if (Keyboard.current == null || buttons.Count == 0) return;

        switch (navigationMode) {
            case NavigationMode.Arrow:
                // 다음 버튼으로 이동
                if (Keyboard.current.downArrowKey.wasPressedThisFrame) MoveNext();
                // 이전 버튼으로 이동
                else if (Keyboard.current.upArrowKey.wasPressedThisFrame) MovePrevious();
                // Space 키로 현재 버튼 실행
                if (Keyboard.current.spaceKey.wasPressedThisFrame) {
                    var btn = buttons[selectedIndex];
                    AudioManager.Instance.PlayUI(0); // 클릭 효과음
                    btn.GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
                }
                break;
            case NavigationMode.QE:
                // Q 키로 이전 버튼으로 이동, E 키로 다음 버튼으로 이동
                if (Keyboard.current.eKey.wasPressedThisFrame) MoveNext();
                else if (Keyboard.current.qKey.wasPressedThisFrame) MovePrevious();
                break;
            case NavigationMode.None:
                // 키 네비게이션 비활성
                break;
        }
    }

    private void MoveNext() {
        selectedIndex = (selectedIndex + 1) % buttons.Count;
        Activate(buttons[selectedIndex]);

        if (navigationMode == NavigationMode.QE) {
            // QE 모드: 즉시 버튼 실행, 강조
            buttons[selectedIndex].GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
            Activate(buttons[selectedIndex], true);
        }

        AudioManager.Instance.PlayUI(1); // 이동 효과음
        EventSystem.current.SetSelectedGameObject(buttons[selectedIndex].gameObject);
    }

    private void MovePrevious() {
        selectedIndex = (selectedIndex - 1 + buttons.Count) % buttons.Count;
        Activate(buttons[selectedIndex]);

        if (navigationMode == NavigationMode.QE) {
            // QE 모드: 즉시 버튼 실행, 강조
            buttons[selectedIndex].GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
            Activate(buttons[selectedIndex], true);
        }

        AudioManager.Instance.PlayUI(1); // 이동 효과음
        EventSystem.current.SetSelectedGameObject(buttons[selectedIndex].gameObject);
    }

    public void Activate(HoverButton active, bool isForce = false) {
        for (int i = 0; i < buttons.Count; i++) {
            bool isActive = buttons[i] == active;

            if (navigationMode == NavigationMode.Arrow) {
                buttons[i].SetColor(isActive ? hoverColor : normalColor, isActive);
            } else if (navigationMode == NavigationMode.QE && isForce) {
                if (isActive) buttons[i].SetColor(hoverColor, true, true);
                else buttons[i].SetColor(normalColor, false, false);
            }

            if (isActive) {
                selectedIndex = i;
            }
        }
    }

    public void OnButtonHovered(HoverButton hoveredButton) {
        // 이미 선택된 버튼이면 소리 재생하지 않음
        if (navigationMode == NavigationMode.Arrow && hoveredButton != buttons[selectedIndex]) {
            AudioManager.Instance.PlayUI(1); // 이동 효과음
            Activate(hoveredButton); // 선택 상태 업데이트
        }
    }

    public void OnUIButtonClicked() {
        if (navigationMode == NavigationMode.Arrow)
            AudioManager.Instance.PlayUI(0); // 클릭 효과음
        else if (navigationMode == NavigationMode.QE)
            AudioManager.Instance.PlayUI(1); // 이동 효과음
    }

    public void DeactivateAll() {
        foreach (var b in buttons)
            b.SetColor(normalColor);
    }
}
