using DG.Tweening;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class MiniGameContext
{
    public static int gameIndex;
    public static int gameLevel;
}

public class GameButtonUI : MonoBehaviour
{
    [Header("Lobby")]
    [SerializeField] MiniGameDatabase database;
    [SerializeField] AudioClip lobbyBgm;

    [Header("Buttons")]
    [SerializeField] Button leftButton, rightButton, randomButton, playButton;
    [SerializeField] RectTransform buttonGroup;
    [SerializeField] Button backButton; // SW-25.10.28 돌아가기 버튼 추가
    [SerializeField] Image holdGauge;

    List<RectTransform> buttonList = new List<RectTransform>();
    int cursor;
    float buttonSize, spacing;
    bool moving;

    float holdTimer;
    float maxHold = 1;
    GameObject holdGaugeParent;
    bool holding;
    KeyControl holdingKey;

    [Header("UI")]
    [SerializeField] TextMeshProUGUI titleText, descriptionText;
    [SerializeField] GameObject[] stars;

    private void Start()
    {
        AudioManager.Instance.PlayBGM(lobbyBgm, true);

        // 직계 자식만 리스트에 추가
        buttonList = buttonGroup.transform
            .Cast<Transform>()
            .Select(t => (RectTransform)t)
            .ToList();

        buttonSize = buttonList[0].sizeDelta.x;
        spacing = GetComponentInChildren<HorizontalLayoutGroup>().spacing;

        for(int i = 0; i < buttonList.Count; i++) 
        {
            Button button = buttonList[i].GetComponent<Button>();
            button.onClick.AddListener(OnClickGameButton);
            button.interactable = false;
        }
        leftButton.onClick.AddListener(() => OnClickArrowButton(-1));
        rightButton.onClick.AddListener(() => OnClickArrowButton(1));
        randomButton.onClick.AddListener(OnClickRandomGameButton);
        playButton.onClick.AddListener(OnClickGameButton);

        holdGaugeParent = holdGauge.transform.parent.gameObject;

        cursor = PlayerPrefs.GetInt("PrevGameIndex", 0);
        for (int i = 0; i < cursor; i++)
        {
            buttonList[0].SetAsLastSibling(); buttonList.Add(buttonList[0]); buttonList.RemoveAt(0);
        }
        UpdateUI();

        DoScaleButton(GetCenterButton(), Vector3.one * 1.5f, true);
        GetCenterButton().GetComponent<Button>().interactable = true;
    }

    private void Update()
    {
        if (Keyboard.current.leftArrowKey.wasPressedThisFrame) OnClickArrowButton(-1);
        else if (Keyboard.current.rightArrowKey.wasPressedThisFrame) OnClickArrowButton(1);
        else if (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.enterKey.wasPressedThisFrame) OnClickGameButton();
        
        if (!holding && Keyboard.current.rKey.wasPressedThisFrame)
        {
            holdGaugeParent.SetActive(true);
            holding = true;
            holdingKey = Keyboard.current.rKey;
        }
        else if (!holding && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            holdGaugeParent.SetActive(true);
            holding = true;
            holdingKey = Keyboard.current.escapeKey;
        }
        if ((holding && Keyboard.current.rKey.wasReleasedThisFrame && holdingKey == Keyboard.current.rKey) ||
            (holding && Keyboard.current.escapeKey.wasReleasedThisFrame && holdingKey == Keyboard.current.escapeKey))
        {
            ResetHoldGauge();
        }

        if (holding)
        {
            holdTimer += Time.deltaTime;
            holdGauge.fillAmount = holdTimer / maxHold;
            if (holdTimer >= maxHold)
            {
                var finishedKey = holdingKey;
                ResetHoldGauge();
                if (finishedKey == Keyboard.current.rKey) OnClickRandomGameButton();
                else if (finishedKey == Keyboard.current.escapeKey) OnClickBackButton();
            }
        }
    }

    void ResetHoldGauge()
    {
        holding = false;
        holdTimer = 0;
        holdGauge.fillAmount = 0;
        holdGaugeParent.SetActive(false);
        holdingKey = null;
    }

    RectTransform GetCenterButton()
    {
        return buttonList[buttonList.Count / 2];
    }

    void UpdateUI()
    {
        titleText.text = database.miniGames[cursor].title;
        descriptionText.text = database.miniGames[cursor].description;

        for (int i = 0; i < stars.Length; i++) stars[i].SetActive(false);
        if (PlayerPrefs.HasKey("MiniGameClear_" + cursor))
        {
            int star = PlayerPrefs.GetInt("MiniGameClear_" + cursor);
            for (int i = 0; i < star; i++) stars[i].SetActive(true);
        }
    }

    void OnClickArrowButton(int dir)
    {
        if (moving) return;

        moving = true;
        GetCenterButton().DOKill();
        DoScaleButton(GetCenterButton(), Vector3.one);
        GetCenterButton().GetComponent<Button>().interactable = false;

        if (dir > 0) { buttonList[0].SetAsLastSibling(); buttonList.Add(buttonList[0]); buttonList.RemoveAt(0); }
        else { buttonList[^1].SetAsFirstSibling(); buttonList.Insert(0, buttonList[^1]); buttonList.RemoveAt(buttonList.Count - 1); }
        cursor = (cursor + dir + database.miniGames.Length) % database.miniGames.Length;

        GetCenterButton().GetComponent<Button>().interactable = true;
        DoScaleButton(GetCenterButton(), Vector3.one * 1.5f, true);
        buttonGroup.anchoredPosition += new Vector2((spacing + buttonSize) * dir, 0);
        buttonGroup.DOAnchorPos(Vector2.zero, 0.5f).SetLink(buttonGroup.gameObject, LinkBehaviour.KillOnDestroy)
                   .OnComplete(() => { moving = false; });

        UpdateUI();
    }

    void DoScaleButton(RectTransform button, Vector3 scale, bool isLoop = false)
    {
        button.DOScale(scale, 0.5f).SetLink(button.gameObject, LinkBehaviour.KillOnDestroy)
              .OnUpdate(() =>
              {
                  Canvas.ForceUpdateCanvases();
                  LayoutRebuilder.ForceRebuildLayoutImmediate(buttonGroup);
              })
              .OnComplete(() =>
              {
                  if (isLoop)
                  {
                      button.DOScale(1.25f, 0.5f).SetDelay(0.1f).SetLink(button.gameObject, LinkBehaviour.KillOnDestroy)
                      .OnComplete(() => button.DOScale(1.5f, 0.5f).SetLoops(-1, LoopType.Yoyo).SetLink(button.gameObject, LinkBehaviour.KillOnDestroy));
                  }
              });
    }

    void OnClickGameButton()
    {
        Debug.Log(database.miniGames[cursor].title + " 게임 시작");
        MiniGameContext.gameIndex = cursor;
        PlayerPrefs.SetInt("Played" + cursor, 1);
        PlayerPrefs.SetInt("PrevGameIndex", cursor);
        GameSceneManager.Instance.LoadSceneWithLoading(SceneNames.FreePlayGame);
    }

    void OnClickRandomGameButton()
    {
        List<int> newGameList = new List<int>();
        for(int i = 0; i < database.miniGames.Length; i++) { if (!PlayerPrefs.HasKey("Played" + i)) newGameList.Add(i); }
        int indexToPlay;
        if (newGameList.Count == 0) 
        {
            List<int> unclearGameList = new List<int>();
            for (int i = 0; i < database.miniGames.Length; i++) { if (!PlayerPrefs.HasKey("MiniGameClear_" + i))  unclearGameList.Add(i); }
            if (unclearGameList.Count ==  0) indexToPlay = Random.Range(0, database.miniGames.Length);
            else indexToPlay = unclearGameList[Random.Range(0, unclearGameList.Count)];
        }
        else indexToPlay = newGameList[Random.Range(0, newGameList.Count)];
        cursor = indexToPlay;
        OnClickGameButton();
    }

    // SW-25.10.28 돌아가기 메서드 추가 (코드에서 자동 할당 방식이 아닌 유니티 에디터에서 수동 할당 방식)
    public void OnClickBackButton()
    {
        //GameSceneManager.Instance.LoadSceneWithLoading(GameSceneManager.Instance.GetMainMenuSceneName());
        GameSceneManager.Instance.LoadSceneWithLoading(SceneNames.MainMenu); // SH=25.11.03 GetMainMenuSceneName 메소드가 없어서 임시 작성 
    }
    // SW-25.10.28 두트윈 파괴 (BackButton을 눌러서 씬 전환시 두트윈이 남아서 경고를 일으키는걸 막는 기능)
    //void OnDestroy()
    //{
    //    // 버튼 그룹 및 버튼 리스트 트윈 정리
    //    if (buttonGroup != null)
    //        DOTween.Kill(buttonGroup);

    //    if (buttonList != null)
    //    {
    //        foreach (var btn in buttonList)
    //        {
    //            if (btn != null)
    //                DOTween.Kill(btn);
    //        }
    //    }
    //}
}
