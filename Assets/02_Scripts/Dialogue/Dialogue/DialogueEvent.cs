using System;
using UnityEngine;

[System.Serializable]
public class ConditionalDialogue {
    public DialogueScriptable dialogue;

    // 외부에서 설정할 함수들
    public Func<bool> Condition;             // 실행 조건
    public Action OnDialogueStart;           // 대화 시작 직전 실행
    public Action OnDialogueComplete;        // 대화 완료 시 실행
}

public class DialogueEvent : MonoBehaviour {
    public ConditionalDialogue[] dialogues;
    private DialogueSystem system;

    void Awake() {
        system = FindFirstObjectByType<DialogueSystem>();
    }

    public void OnInteract() {
        if (system == null) return;
        if (system.IsDialogueActive) return;
        if (Time.time < system.InputLockedUntil) return;

        foreach (var item in dialogues) {
            if (item.Condition != null && item.Condition()) {
                // 대화 시작 직전 실행
                item.OnDialogueStart?.Invoke();
                // 실제 대화 시작
                system.StartDialogue(item.dialogue, () => {
                    // 대화 종료 시 실행
                    item.OnDialogueComplete?.Invoke();
                });
                break;
            }
        }
    }
}
