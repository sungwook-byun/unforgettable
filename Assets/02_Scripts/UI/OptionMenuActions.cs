using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class OptionMenuActions : MonoBehaviour {

    [Header("UI Panels")]
    [SerializeField] private GameObject optionsPanel; // 옵션 패널
    [SerializeField] private GameObject mainMenuUI; // 메인 메뉴 UI
    [SerializeField] private GameObject saveAndLoadUI; // 세이브 & 로드 UI

    [Header("Options Menu Buttons")]
    [SerializeField] private Button ADButton; // 오디오 & 디스플레이 버튼
    [SerializeField] private Button controlButton; // 조작 버튼
    [SerializeField] private Button environmentButton; // 환경 버튼

    [Header("Options UI Panels")]
    [SerializeField] private GameObject audioDisplayPanel; // 오디오 & 디스플레이 패널
    [SerializeField] private GameObject controlPanel; // 조작 패널
    [SerializeField] private GameObject environmentPanel; // 환경 패널

    private ButtonGroupActions groupManager;

    void Awake() {
        ADButton.onClick.AddListener(AudioAndDisplay);
        controlButton.onClick.AddListener(Control);
        environmentButton.onClick.AddListener(Environment);
        groupManager = GetComponentInChildren<ButtonGroupActions>();
        saveAndLoadUI.SetActive(false); // 시작 시 세이브 & 로드 UI 비활성화
    }

    void OnEnable() {
        // 기본으로 오디오 & 디스플레이 패널 활성화
        AudioAndDisplay();
    }

    void Update() {
        if (ControlSettings.IsWaitingForInput || SaveAndLoadSettings.IsOpenedPanel) return; // 키 입력 중이면 메뉴 이동 무시
        if (Keyboard.current.escapeKey.wasPressedThisFrame) {
            QuitOptions();
        }
    }

    public void AudioAndDisplay() {
        audioDisplayPanel.SetActive(true);
        controlPanel.SetActive(false);
        environmentPanel.SetActive(false);
    }

    public void Control() {
        audioDisplayPanel.SetActive(false);
        controlPanel.SetActive(true);
        environmentPanel.SetActive(false);
    }

    public void Environment() {
        audioDisplayPanel.SetActive(false);
        controlPanel.SetActive(false);
        environmentPanel.SetActive(true);
    }

    public void QuitOptions() {
        optionsPanel.SetActive(false);
        mainMenuUI.SetActive(true);
    }
}
