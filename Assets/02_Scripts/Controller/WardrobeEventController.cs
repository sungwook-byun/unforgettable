using UnityEngine;
using static Contants;

public class WardrobeEventController : MonoBehaviour {
    public DialogueEvent wardrobeDialogueEvent;

    private InteractManager interactManager;

    private bool firstDialogueDone = false;
    private bool secondDialogueDone = false;

    void Awake() {
        interactManager = FindFirstObjectByType<InteractManager>();
    }

    void Start() {
        if (wardrobeDialogueEvent != null) {
            wardrobeDialogueEvent.dialogues[0].Condition = () => grandmaTocuhed && !firstDialogueDone && !itemTouched;
            wardrobeDialogueEvent.dialogues[0].OnDialogueComplete = () => {
                Debug.Log("너무 낡아 열기도 힘들어 보이는 오래된 장롱이다.");
                firstDialogueDone = true;
                wardrobeTouched = true; // 옷장 처음 열림 여부 업데이트
                itemTouched = true;   // 물건을 건드린 상태 업데이트
                interactManager.SetTageAndLayoutWardrobe();
            };
            wardrobeDialogueEvent.dialogues[1].Condition = () => grandmaTocuhed && !wardrobeHasPencil;
            wardrobeDialogueEvent.dialogues[1].OnDialogueComplete = () => {
                Debug.Log("책상을 살펴보자.");
                secondDialogueDone = true;
            };
            wardrobeDialogueEvent.dialogues[2].Condition = () => grandmaTocuhed && wardrobeHasPencil && !secondDialogueDone;
            wardrobeDialogueEvent.dialogues[2].OnDialogueComplete = () => {
                Debug.Log("이거라도 내가 할 수 있을 거야");
                interactManager.SetTagAndLayoutGrandma(false);
                wardrobeEventCompleted = true; // 옷장 이벤트 완료 여부 업데이트
                interactManager.SetActivePortal();
            };
        }
    }

    void Update() {
        wardrobeDialogueEvent.dialogues[0].Condition = () => grandmaTocuhed && !firstDialogueDone && !itemTouched;
        wardrobeDialogueEvent.dialogues[1].Condition = () => grandmaTocuhed && !wardrobeHasPencil;
        wardrobeDialogueEvent.dialogues[2].Condition = () => grandmaTocuhed && wardrobeHasPencil && !secondDialogueDone;
    }
}
