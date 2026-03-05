using UnityEngine;
using static Contants;

public class RecordPlayerEventController : MonoBehaviour {
    public DialogueEvent recordPlayerDialogueEvent;

    private InteractManager interactManager;

    private bool firstDialogueDone = false;
    private bool secondDialogueDone = false;

    [SerializeField] private AudioClip recordPlayBgm;

    void Awake() {
        interactManager = FindFirstObjectByType<InteractManager>();
    }

    void Start() {
        // 레코드 플레이어 상호작용 이벤트 처리
        if (recordPlayerDialogueEvent != null) {
            recordPlayerDialogueEvent.dialogues[0].Condition = () => grandmaTocuhed && !firstDialogueDone && !itemTouched;
            recordPlayerDialogueEvent.dialogues[0].OnDialogueComplete = () => {
                Debug.Log("레코드 플레이어 상호작용 완료.");
                firstDialogueDone = true;
                recordPlayerTouched = true; // 포스터 처음 클릭 여부 업데이트
                itemTouched = true;   // 물건을 건드린 상태 업데이트
                interactManager.SetTageAndLayoutRecordPlayer();
            };
            recordPlayerDialogueEvent.dialogues[1].Condition = () => grandmaTocuhed && !grandmaRecordPlayerEventStarted;
            recordPlayerDialogueEvent.dialogues[1].OnDialogueComplete = () => {
                Debug.Log("할머니한테 레코드 플레이어에 대해 물어보자.");
            };
            recordPlayerDialogueEvent.dialogues[2].Condition = () => grandmaTocuhed && grandmaRecordPlayerEventStarted && !recordPlayerHasDisc;
            recordPlayerDialogueEvent.dialogues[2].OnDialogueComplete = () => {
                Debug.Log("레코드 판을 먼저 찾아야 해.");
            };
            recordPlayerDialogueEvent.dialogues[3].Condition = () => grandmaTocuhed && grandmaRecordPlayerEventStarted && recordPlayerHasDisc && !secondDialogueDone;
            recordPlayerDialogueEvent.dialogues[3].OnDialogueStart = () => {
                if (AudioManager.Instance != null) {
                    AudioManager.Instance.StopBGM();
                    AudioManager.Instance.PlayBGM(recordPlayBgm);
                }
            };
            recordPlayerDialogueEvent.dialogues[3].OnDialogueComplete = () => {
                Debug.Log("...");
                interactManager.SetTagAndLayoutGrandma(false);
                recordPlayerEventCompleted = true; // 레코드 플레이어 이벤트 완료 여부 업데이트
                interactManager.SetActivePortal();
            };
        }
    }

    void Update() {
        recordPlayerDialogueEvent.dialogues[0].Condition = () => grandmaTocuhed && !firstDialogueDone && !itemTouched;
        recordPlayerDialogueEvent.dialogues[1].Condition = () => grandmaTocuhed && !grandmaRecordPlayerEventStarted;
        recordPlayerDialogueEvent.dialogues[2].Condition = () => grandmaTocuhed && grandmaRecordPlayerEventStarted && !recordPlayerHasDisc;
        recordPlayerDialogueEvent.dialogues[3].Condition = () => grandmaTocuhed && grandmaRecordPlayerEventStarted && recordPlayerHasDisc && !secondDialogueDone;
    }
}
