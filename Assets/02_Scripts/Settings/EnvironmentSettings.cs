using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnvironmentSettings : MonoBehaviour {
	
	[Header("Save And Load Button")]
	[SerializeField] private Button saveLoadButton;
	[SerializeField] private Button resetButton;

    [Header("Save And Load UI")]
    [SerializeField] private GameObject saveLoadBackground;
    [SerializeField] private GameObject saveLoadUI;

    [Header("Language")]
	[SerializeField] private Button prevLangButton;
	[SerializeField] private Button nextLangButton;
	[SerializeField] private TextMeshProUGUI languageText;

    private string[] availableLanguages = { "한국어", "English", "Deutsch", "Türkçe" };
    private int currentLangIndex = 0;

    void Awake() {
		saveLoadButton.onClick.AddListener(OpenSaveLoad);
		resetButton.onClick.AddListener(ResetSave);
		prevLangButton.onClick.AddListener(PrevLanguage);
		nextLangButton.onClick.AddListener(NextLanguage);
    }

    private void OpenSaveLoad() {
        saveLoadBackground.SetActive(true);
        saveLoadUI.SetActive(true);
    }

    private void ResetSave() {
        Debug.Log("모든 저장 데이터 초기화");
    }

    private void PrevLanguage() {
        AudioManager.Instance.PlayUI(1); // 이동 효과음
        currentLangIndex--;
        if (currentLangIndex < 0)
            currentLangIndex = availableLanguages.Length - 1;
        UpdateLanguage();
    }

    private void NextLanguage() {
        AudioManager.Instance.PlayUI(1); // 이동 효과음
        currentLangIndex++;
        if (currentLangIndex >= availableLanguages.Length)
            currentLangIndex = 0;
        UpdateLanguage();
    }

    private void UpdateLanguage() {
        languageText.text = availableLanguages[currentLangIndex];
        Debug.Log($"현재 언어: {languageText.text}");

        // 실제 게임 내 언어 변경 처리 (LocalizationManager 등과 연동)
        // LocalizationManager.Instance.SetLanguage(languageText.text);
    }

}
