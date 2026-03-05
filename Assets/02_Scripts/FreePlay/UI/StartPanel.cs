using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class StartPanel : MonoBehaviour
{
    [SerializeField] GameObject startBox, levelBox;
    [SerializeField] TextMeshProUGUI titleText_start, descriptionText;
    [SerializeField] Button startButton, lobbyButton_start, easyButton, normalButton, hardButton;
    [SerializeField] Image previewImage;
    Action OnLobby;

    public void SetPanel(MiniGameData data, Action<int> onSelectLevel, Action onLobby)
    {
        titleText_start.text = data.title;
        descriptionText.text = data.how;
        previewImage.sprite = data.sprite;

        startButton.onClick.AddListener(OnClickStartButton);
        easyButton.onClick.AddListener(() => onSelectLevel?.Invoke(0));
        normalButton.onClick.AddListener(() => onSelectLevel?.Invoke(1));
        hardButton.onClick.AddListener(() => onSelectLevel?.Invoke(2));
        lobbyButton_start.onClick.AddListener(() => onLobby?.Invoke());
        OnLobby = onLobby;

        gameObject.SetActive(true);    
    }

    void OnClickStartButton()
    {
        startBox.SetActive(false);
        levelBox.SetActive(true);
    }

    private void Update()
    {
        if (Keyboard.current.enterKey.wasPressedThisFrame ||
            Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            OnClickStartButton();
        }

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            OnLobby?.Invoke();
        }
    }
}
