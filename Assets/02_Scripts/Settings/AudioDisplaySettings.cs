using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AudioDisplaySettings : MonoBehaviour {
    [Header("Audio Sliders")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider uiSlider;

    [Header("Audio Texts")]
    [SerializeField] private TMP_Text masterText;
    [SerializeField] private TMP_Text bgmText;
    [SerializeField] private TMP_Text sfxText;
    [SerializeField] private TMP_Text uiText;

    [Header("Brightness")]
    // 밝기 최대값(0~0.7 정도가 현실적)
    [Range(0f, 1f)]
    [SerializeField] private float maxDarkness = 0.9f;
    [SerializeField] private Image brightnessOverlay;

    [Header("Brightness Slider")]
    [SerializeField] private Slider brightnessSlider;

    [Header("Brightness Text")]
    [SerializeField] private TMP_Text brightnessValueText;

    private void Start() {
        if (AudioManager.Instance == null) return;

        // 초기 값 동기화
        masterSlider.value = AudioManager.Instance.masterVolume;
        bgmSlider.value = AudioManager.Instance.bgmVolume;
        sfxSlider.value = AudioManager.Instance.sfxVolume;
        uiSlider.value = AudioManager.Instance.uiVolume;
        brightnessSlider.value = 1f - (brightnessOverlay.color.a / maxDarkness);

        UpdateTexts();

        // 리스너 등록
        masterSlider.onValueChanged.AddListener(SetMasterVolume);
        bgmSlider.onValueChanged.AddListener(SetBGMVolume);
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        uiSlider.onValueChanged.AddListener(SetUIVolume);
        brightnessSlider.onValueChanged.AddListener(SetBrightness);
    }

    private void SetMasterVolume(float value) {
        AudioManager.Instance.SetMasterVolume(value);
        masterText.text = $"{Mathf.RoundToInt(value * 100)}";
    }

    private void SetBGMVolume(float value) {
        AudioManager.Instance.SetBGMVolume(value);
        bgmText.text = $"{Mathf.RoundToInt(value * 100)}";
    }

    private void SetSFXVolume(float value) {
        AudioManager.Instance.SetSFXVolume(value);
        sfxText.text = $"{Mathf.RoundToInt(value * 100)}";
    }

    private void SetUIVolume(float value) {
        AudioManager.Instance.SetUIVolume(value);
        uiText.text = $"{Mathf.RoundToInt(value * 100)}";
    }

    public void SetBrightness(float value) {
        // value = 1일 때 완전 밝음, 0일 때 최대 어두움(maxDarkness)
        float darkness = (1f - value) * maxDarkness;
        brightnessOverlay.color = new Color(0, 0, 0, darkness);

        // 텍스트 업데이트
        brightnessValueText.text = $"{Mathf.RoundToInt(value * 100)}";
    }

    private void UpdateTexts() {
        masterText.text = $"{Mathf.RoundToInt(masterSlider.value * 100)}";
        bgmText.text = $"{Mathf.RoundToInt(bgmSlider.value * 100)}";
        sfxText.text = $"{Mathf.RoundToInt(sfxSlider.value * 100)}";
        uiText.text = $"{Mathf.RoundToInt(uiSlider.value * 100)}";
    }
}
