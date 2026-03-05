using UnityEngine;
using static Contants;

public class DreamGrandmaEventController : MonoBehaviour {
    public DialogueEvent grandmaDialogueEvent;

    // private bool firstDialogueDone = false;
    // private bool secondDialogueDone = false;
    // private bool thirdDialogueDone = false;
    
    private int count = 0; 

    private Day4Controller _day4Controller;

    private void Awake() {
        _day4Controller = FindFirstObjectByType<Day4Controller>();
    }

    private void Start() {
        // 조건 및 완료 함수 연결
        if (grandmaDialogueEvent != null) {
            grandmaDialogueEvent.dialogues[0].Condition = () => count == 0 || count == 2;
            grandmaDialogueEvent.dialogues[0].OnDialogueComplete = () => {
                count++;
            };
            grandmaDialogueEvent.dialogues[1].Condition = () => count == 1 || count == 3;
            grandmaDialogueEvent.dialogues[1].OnDialogueComplete = () => {
                count++;
            };

            // 포스터 관련 조건 업데이트
            grandmaDialogueEvent.dialogues[2].Condition = () => count >= 4;
            grandmaDialogueEvent.dialogues[2].OnDialogueComplete = () => {
                _day4Controller.SetWardrobeOn();
            };
        }
    }
}
