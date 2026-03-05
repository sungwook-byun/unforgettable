using UnityEngine;
using static Contants;

public class DeskEventController : MonoBehaviour {
    public DialogueEvent DeskDialogueEvent;

    private bool firstDialogueDone = false;
    private bool secondDigalogueDone = false;

    private InteractManager interactManager;

    void Awake() {
        interactManager = FindFirstObjectByType<InteractManager>();
    }

    void Start() {
        if (DeskDialogueEvent != null) {
            DeskDialogueEvent.dialogues[0].Condition = () => grandmaTocuhed && !wardrobeTouched;
            DeskDialogueEvent.dialogues[0].OnDialogueComplete = () => {
                Debug.Log("책상엔 널부러진 종이들과 여러 잡동사니들이 놓여져 있다.");
            };
        }
        DeskDialogueEvent.dialogues[1].Condition = () => grandmaTocuhed && wardrobeTouched && !firstDialogueDone;
        DeskDialogueEvent.dialogues[1].OnDialogueComplete = () => {
            firstDialogueDone = true;
            wardrobeHasPencil = true;
            Debug.Log("책상엔 널부러진 종이들 사이로 작은 연필이 보였다.");
        };
        DeskDialogueEvent.dialogues[2].Condition = () => grandmaTocuhed && wardrobeTouched && !wardrobeEventStarted;
        DeskDialogueEvent.dialogues[2].OnDialogueComplete = () => {
            firstDialogueDone = true;
            Debug.Log("더 이상 볼일이 없다. 장롱으로 가자.");
        };
    }

    void Update() {
        DeskDialogueEvent.dialogues[0].Condition = () => grandmaTocuhed && !wardrobeTouched;
        DeskDialogueEvent.dialogues[1].Condition = () => grandmaTocuhed && wardrobeTouched && !firstDialogueDone;
        DeskDialogueEvent.dialogues[2].Condition = () => grandmaTocuhed && wardrobeTouched && !wardrobeEventStarted;
    }
}
