using UnityEngine;
using UnityEngine.Serialization;
using static Contants;

public class DreamWormEventController : MonoBehaviour {
    public DialogueEvent dialogueEvent;

    private void Start() {
        // 조건 및 완료 함수 연결
        if (dialogueEvent != null) {
            dialogueEvent.dialogues[0].Condition = () => true;
            dialogueEvent.dialogues[0].OnDialogueComplete = () => {
            };
        }
    }
}
