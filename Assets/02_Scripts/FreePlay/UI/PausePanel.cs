using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PausePanel : MonoBehaviour
{
    [SerializeField] Button returnToGameButton, retryButton_pause, lobbyButton_pause, pauseButton;
    Action OnRetry, OnLobby;

    public void SetPanel(Action onPause, Action onRetry, Action onLobby)
    {
        returnToGameButton.onClick.AddListener(() => onPause?.Invoke());
        retryButton_pause.onClick.AddListener(() => onRetry?.Invoke());
        lobbyButton_pause.onClick.AddListener(() => onLobby?.Invoke());
        pauseButton.onClick.AddListener(() => onPause?.Invoke());
        OnRetry = onRetry;
        OnLobby = onLobby;
    }

    private void Update()
    {
        if (Keyboard.current.enterKey.wasPressedThisFrame ||
            Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            OnRetry?.Invoke();
        }

        if (Keyboard.current.qKey.wasPressedThisFrame)
        {
            OnLobby?.Invoke();
        }
    }
}
