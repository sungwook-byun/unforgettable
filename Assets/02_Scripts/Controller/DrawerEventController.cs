using UnityEngine;
using static Contants;

public class DrawerEventController : MonoBehaviour {
    public DialogueEvent DrawerDialogueEvent;
    
    private bool firstDialogueDone = false;
    private bool secondDigalogueDone = false;

    private InteractManager interactManager;

    void Awake() {
        interactManager = FindFirstObjectByType<InteractManager>();
    }

    void Start() {
        if (DrawerDialogueEvent != null) {
            DrawerDialogueEvent.dialogues[0].Condition = () => grandmaTocuhed && !recordPlayerTouched;
            DrawerDialogueEvent.dialogues[0].OnDialogueComplete = () => {
                Debug.Log("낡은 책들과 여러 잡동사니가 진열되어 있다.");
            };
            DrawerDialogueEvent.dialogues[1].Condition = () => grandmaTocuhed && recordPlayerTouched && !firstDialogueDone && grandmaRecordPlayerEventStarted;
            DrawerDialogueEvent.dialogues[1].OnDialogueComplete = () => {
                firstDialogueDone = true;
                recordPlayerHasDisc = true;
                Debug.Log("낡은 책들과 어려 잡동사니들을 뒤로 한 채, 오래된 레코드 판을 찾았다.");
            };
            DrawerDialogueEvent.dialogues[2].Condition = () => grandmaTocuhed && recordPlayerTouched && !recordPlayerEventStarted && grandmaRecordPlayerEventStarted;
            DrawerDialogueEvent.dialogues[2].OnDialogueComplete = () => {
                firstDialogueDone = true;
                Debug.Log("레코드 판을 가져가 작동시켜보자.");
            };
        }
    }

    void Update() {
        DrawerDialogueEvent.dialogues[0].Condition = () => grandmaTocuhed && !recordPlayerTouched;
        DrawerDialogueEvent.dialogues[1].Condition = () => grandmaTocuhed && recordPlayerTouched && !firstDialogueDone && grandmaRecordPlayerEventStarted;
        DrawerDialogueEvent.dialogues[2].Condition = () => grandmaTocuhed && recordPlayerTouched && !recordPlayerEventStarted && grandmaRecordPlayerEventStarted;
    }
}
