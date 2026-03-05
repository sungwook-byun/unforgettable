using UnityEngine;

[System.Serializable]
public class DialogueChoice {
    [TextArea(1, 3)] public string choiceText; // 버튼에 표시될 텍스트
    public int nextIndex;                     // 이 선택 시 이동할 다음 대사 인덱스
}

[System.Serializable]
public class DialogueData {
    public int speakerIndex;
    [TextArea(2, 5)] public string dialogue;
    public float typingSpeed = 0.05f;
    public Sprite customPortrait;

    [Header("선택지 (선택적)")]
    public DialogueChoice[] choices;

    [Header("특수 옵션")]
    public bool isChoiceOnly = false;

    [Header("다음 인덱스 설정 (선택적)")]
    public int nextDialogueIndex = -1;

    [Header("필터 옵션")]
    public FilterUI.FilterType filterType = FilterUI.FilterType.None; // 필터 추가
}