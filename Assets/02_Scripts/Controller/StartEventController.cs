using UnityEngine;
using static Contants;

public class StartEventController : MonoBehaviour {
    public DialogueEvent startDialogueEvent;

    private InteractManager interactManager;

    [Header("BGM Settings")]
    [SerializeField] private AudioClip startingBGM;

    private void Awake() {
        interactManager = FindFirstObjectByType<InteractManager>();
    }

    void Start() {
        if (AudioManager.Instance != null) {
            AudioManager.Instance.PlayBGM(startingBGM);
        }
        // 시작 이벤트 처리
        if (startDialogueEvent != null) {
            if (posterEventCompleted) {
                interactManager.SetPosterObj(); // 포스터 오브젝트 설정
            }

            var data = SaveManager.Instance.Load();
            if (itemTouched && grandmaTocuhed) {
                itemTouched = false;
                grandmaTocuhed = false;
            }
            if (data != null) {
                int day = data.currentDay;
                startDialogueEvent.dialogues[0].Condition = () => false;
                if (data.isDiaryMode) {
                    interactManager.ResetTagsAndLayouts();
                }
                startDialogueEvent.dialogues[1].Condition = () => day == 1;
                startDialogueEvent.dialogues[2].Condition = () => day > 1;
            } else {
                startDialogueEvent.dialogues[0].Condition = () => true;
                startDialogueEvent.dialogues[1].Condition = () => false;
            }
            startDialogueEvent.dialogues[0].OnDialogueComplete = () => {
                Debug.Log("할머니한테 말을 걸어보자.");
                interactManager.ResetTagsAndLayouts();
            };
        }

        startDialogueEvent.OnInteract();
    }

    void Update() {
        var data = SaveManager.Instance.Load();
        if (data != null) {
            startDialogueEvent.dialogues[0].Condition = () => false;
            if (data.isDiaryMode) {
                interactManager.SetTagAndLayoutGrandma(false);
            } else {
                interactManager.SetTagAndLayoutGrandma(true, false);
            }
        } else
            startDialogueEvent.dialogues[0].Condition = () => true;
    }
}
