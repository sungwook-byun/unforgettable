using UnityEngine;

public class PortalEventController : MonoBehaviour {
    public DialogueEvent portalDialogueEvent;

    private bool firstDialogueDone = false;

    private InteractManager interactManager;
    private DialogueSystem dialogueSystem;

    void Awake() {
        interactManager = FindFirstObjectByType<InteractManager>();
        dialogueSystem = FindFirstObjectByType<DialogueSystem>();
    }

    void Start() {
        // 조건 및 완료 함수 연결
        if (portalDialogueEvent != null) {
            portalDialogueEvent.dialogues[0].Condition = () => !firstDialogueDone;
            portalDialogueEvent.dialogues[0].OnDialogueStart = () => {
               interactManager.ResetTagsAndLayouts(); // 모든 상호작용 오브젝트 비활성화
                dialogueSystem.StartFadeIn(2);
            };
            portalDialogueEvent.dialogues[0].OnDialogueComplete = () => {
                firstDialogueDone = true;
                AudioManager.Instance.StopBGM();
                // 이름 바꿨음 윤 
                GameSceneManager.Instance.LoadSceneWithLoading(SceneNames.Day3MemoryWorld);
            };
        }
    }
}
