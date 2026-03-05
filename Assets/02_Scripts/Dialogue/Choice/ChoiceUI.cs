using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;

public class ChoiceUI : MonoBehaviour {
    [Header("UI Elements")]
    [SerializeField] private GameObject choicePanel;        // Choice Panel
    [SerializeField] private Transform choiceContainer;     // 버튼이 들어갈 Content
    [SerializeField] private GameObject choiceButtonPrefab; // 버튼 프리팹

    private List<Button> buttons = new List<Button>();
    private List<TMP_Text> texts = new List<TMP_Text>();
    private List<GameObject> highlightBars = new List<GameObject>(); // 하이라이트 바
    private Action<int> onChoiceSelected;
    private int currentIndex = 0;

    private const float normalFontSize = 32f;
    private const float highlightFontSize = 36f;

    private void Awake() {
        choicePanel.SetActive(false);
    }

    /// <summary>
    /// 선택지 표시
    /// </summary>
    public void ShowChoices(DialogueChoice[] choices, Action<int> callback) {
        // 기존 버튼 제거
        foreach (Transform child in choiceContainer)
            Destroy(child.gameObject);
        buttons.Clear();
        texts.Clear();
        highlightBars.Clear();

        // 버튼 생성
        for (int i = 0; i < choices.Length; i++) {
            GameObject btnObj = Instantiate(choiceButtonPrefab, choiceContainer);

            // 배경 검은색, 알파 0
            Image img = btnObj.GetComponent<Image>();
            if (img != null) {
                Color c = Color.black;
                c.a = 0f; // 투명
                img.color = c;
            }

            // 텍스트 설정
            TMP_Text txt = btnObj.GetComponentInChildren<TMP_Text>();
            txt.text = choices[i].choiceText;
            txt.color = Color.white;
            txt.fontSize = normalFontSize;

            // HighlightBar 찾아서 저장
            Transform bar = btnObj.transform.Find("[Image] HighlightBar");
            if (bar != null) {
                bar.gameObject.SetActive(false); // 초기 상태 끔
                highlightBars.Add(bar.gameObject);
            } else {
                highlightBars.Add(null);
            }

            int index = i;
            Button btn = btnObj.GetComponent<Button>();
            btn.onClick.AddListener(() => OnButtonClicked(index));

            ChoiceButton choiceButton = btnObj.GetComponent<ChoiceButton>();
            if (choiceButton == null)
                choiceButton = btnObj.AddComponent<ChoiceButton>();

            choiceButton.Init(this, index);

            buttons.Add(btn);
            texts.Add(txt);
        }

        currentIndex = 0;
        HighlightButton(currentIndex); // 최소 1개 강조
        onChoiceSelected = callback;

        choicePanel.SetActive(true);
    }

    private void Update() {
        if (!choicePanel.activeSelf || buttons.Count == 0) return;

        // 방향키 이동
        if (Input.GetKeyDown(KeyCode.UpArrow)) {
            currentIndex = Mathf.Max(0, currentIndex - 1);
            HighlightButton(currentIndex);
        } else if (Input.GetKeyDown(KeyCode.DownArrow)) {
            currentIndex = Mathf.Min(buttons.Count - 1, currentIndex + 1);
            HighlightButton(currentIndex);
        }

        // 선택
        if (Input.GetKeyDown(KeyCode.E)) {
            if (buttons.Count > 0)
                buttons[currentIndex].onClick.Invoke();
        }
    }

    private void OnButtonClicked(int index) {
        choicePanel.SetActive(false);
        onChoiceSelected?.Invoke(index);
    }

    /// <summary>
    /// 현재 강조 중인 버튼 갱신 (키보드, 마우스 공용)
    /// </summary>
    public void HighlightButton(int index) {
        for (int i = 0; i < texts.Count; i++) {
            texts[i].fontSize = i == index ? highlightFontSize : normalFontSize;

            if (highlightBars[i] != null) {
                highlightBars[i].SetActive(i == index);
            }
        }

        currentIndex = index; // 방향키와 마우스 동기화
    }

    public bool IsActive() {
        return choicePanel.activeSelf;
    }
}
