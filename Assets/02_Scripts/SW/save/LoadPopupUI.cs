using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LoadPopupUI : MonoBehaviour
{
    [Header("UI 참조")]
    [SerializeField] private TextMeshProUGUI saveTimeText;
    [SerializeField] private TextMeshProUGUI sceneNameText;
    [SerializeField] private GameObject hasSaveGroup;
    [SerializeField] private GameObject noSaveGroup;

    [Header("버튼 참조")]
    [SerializeField] private Button continueButton;
    [SerializeField] private Button backButton;

    void Start()
    {
        continueButton.onClick.AddListener(OnContinueClicked);
        backButton.onClick.AddListener(OnBackClicked);
    }

    void OnEnable()
    {
        UpdateSaveInfo();
    }

    // 세이브 상태에 따라 그룹 표시 전환
    public void UpdateSaveInfo()
    {
        bool hasSave = SaveManager.Instance.HasSaveData();

        if (hasSave)
        {
            SaveData data = SaveManager.Instance.Load();
            if (data != null)
            {
                saveTimeText.text = "저장 시간 : " + data.localTimeString;
                sceneNameText.text = "마지막 위치 : " + data.sceneName;
            }
            else
            {
                saveTimeText.text = "-";
                sceneNameText.text = "-";
            }

            hasSaveGroup.SetActive(true);
            noSaveGroup.SetActive(false);
        }
        else
        {
            hasSaveGroup.SetActive(false);
            noSaveGroup.SetActive(true);
        }
    }

    public void OnContinueClicked()
    {
        SaveData data = SaveManager.Instance.Load();
        if (data != null && !string.IsNullOrEmpty(data.sceneName))
        {
            GameSceneManager.Instance.LoadSceneWithLoading(data.sceneName);
        }
    }

    public void OnBackClicked()
    {
        gameObject.SetActive(false);
    }
}