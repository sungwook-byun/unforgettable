using UnityEngine;

public class JournalEventController : MonoBehaviour {
    public DialogueEvent JournalDialogueEvent;

    [SerializeField] private GameObject diaryPanel;

    private InteractManager interactManager;

    void Awake() {
        interactManager = FindFirstObjectByType<InteractManager>();
    }

    void Start() {
        if (JournalDialogueEvent != null) {
            JournalDialogueEvent.dialogues[0].Condition = () => false;
            JournalDialogueEvent.dialogues[0].OnDialogueComplete = () => {
                Debug.Log("지금은 읽을 수만 있다. 밤이 되면 다이어리를 적을 게 있을 것 같다.");
                diaryPanel.SetActive(true);
                interactManager.SetTageAndLayoutJournal(false);

            };
            JournalDialogueEvent.dialogues[1].Condition = () => false;
            JournalDialogueEvent.dialogues[1].OnDialogueComplete = () => {
                Debug.Log("다이어리를 펼쳐보았다. 오늘 하루 있었던 일을 적어볼 수 있을 것 같다.");
                diaryPanel.SetActive(true);
                interactManager.SetTageAndLayoutJournal(false);
            };
        }
    }

    void Update() {
        var data = SaveManager.Instance.Load();
        if (data == null) {
            JournalDialogueEvent.dialogues[0].Condition = () => true;
            JournalDialogueEvent.dialogues[1].Condition = () => false;
        } else {
            JournalDialogueEvent.dialogues[0].Condition = () => !data.isDiaryMode;
            JournalDialogueEvent.dialogues[1].Condition = () => data.isDiaryMode;
        }
        if (!diaryPanel.activeSelf) {
            interactManager.SetTageAndLayoutJournal(true);
        } else {
            interactManager.SetTageAndLayoutJournal(false);
        }
    }
}
