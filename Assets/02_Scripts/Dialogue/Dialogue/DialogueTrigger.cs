using UnityEngine;

public class DialogueTrigger : MonoBehaviour {
    [SerializeField] private DialogueScriptable dialogueData;
    [SerializeField] private string interactPrompt = "E키를 눌러 대화하기";

    private bool isPlayerInRange = false;
    private DialogueSystem system;

    private void Start() {
        system = FindFirstObjectByType<DialogueSystem>();
    }

    private void OnTriggerEnter(Collider other) {
        if (!other.CompareTag("Player")) return;
        isPlayerInRange = true;

        if (system != null)
            system.ShowInteractUI(interactPrompt);
    }

    private void OnTriggerExit(Collider other) {
        if (!other.CompareTag("Player")) return;
        isPlayerInRange = false;

        if (system != null)
            system.HideInteractUI();
    }

    private void Update() {
        if (!isPlayerInRange || system == null) return;
        if (Time.time < system.InputLockedUntil) return;  // 대화 종료 직후 입력 잠금 보호
        if (!Input.GetKeyDown(KeyCode.E)) return;

        if (!system.IsDialogueActive)
            system.StartDialogue(dialogueData, OnDialogueEnd);
    }

    private void OnDialogueEnd() {
        if (isPlayerInRange && system != null)
            system.ShowInteractUI(interactPrompt);
    }
}
