using UnityEngine;
using static Contants;

public class GrandmaEventController : MonoBehaviour {
    public DialogueEvent grandmaDialogueEvent;

    private bool firstDialogueDone = false;
    private bool secondDialogueDone = false;
    private bool thirdDialogueDone = false;

    private InteractManager interactManager;

    private void Awake() {
        interactManager = FindFirstObjectByType<InteractManager>();
    }

    private void Start() {
        // 조건 및 완료 함수 연결
        if (grandmaDialogueEvent != null) {
            var data = SaveManager.Instance.Load();
            grandmaDialogueEvent.dialogues[0].Condition = () => !firstDialogueDone && !(data != null);
            grandmaDialogueEvent.dialogues[0].OnDialogueComplete = () => {
                Debug.Log("방을 둘러보자");
                firstDialogueDone = true;
                grandmaTocuhed = true; // 할머니 처음 클릭 여부 업데이트
                interactManager.SetTageAndLayoutAll();
            };
            grandmaDialogueEvent.dialogues[1].Condition = () => !itemTouched && firstDialogueDone;
            grandmaDialogueEvent.dialogues[1].OnDialogueComplete = () => {
                Debug.Log("아직 시간이 필요하다 방을 둘러보자");
                firstDialogueDone = true;
                grandmaTocuhed = true; // 할머니 처음 클릭 여부 업데이트
            };

            // 포스터 관련 조건 업데이트
            grandmaDialogueEvent.dialogues[2].Condition = () => posterTouched && !secondDialogueDone && !posterEventDone;
            grandmaDialogueEvent.dialogues[2].OnDialogueComplete = () => {
                Debug.Log("액자를 세워달라");
                secondDialogueDone = true;
                grandmaPosterEventStarted = true; // 할머니 포스터 이벤트 시작 여부 업데이트
            };
            grandmaDialogueEvent.dialogues[3].Condition = () => posterTouched && !posterEventCompleted && !posterEventDone;
            grandmaDialogueEvent.dialogues[3].OnDialogueComplete = () => {
                Debug.Log("이런 부탁해서 미안하지만 액자를 세워달라");
            };
            grandmaDialogueEvent.dialogues[4].Condition = () => posterTouched && posterEventCompleted && !posterEventDone;
            grandmaDialogueEvent.dialogues[4].OnDialogueComplete = () => {
                Debug.Log("할머니...");
                posterEventDone = true; // 포스터 이벤트 완료 여부 업데이트
                interactManager.SetTagAndLayoutGrandma(false);
                interactManager.SetActivePortal();
            };

            // 레코드 플레이어 관련 조건 업데이트
            grandmaDialogueEvent.dialogues[5].Condition = () => recordPlayerTouched && !thirdDialogueDone && !recordPlayerEventCompleted;
            grandmaDialogueEvent.dialogues[5].OnDialogueComplete = () => {
                Debug.Log("장롱 옆 서랍에서 레코드판을 찾아라.");
                thirdDialogueDone = true;
                grandmaRecordPlayerEventStarted = true; // 할머니 레코드 플레이어 이벤트 시작 여부 업데이트
            };

            // 공용 조건 업데이트
            grandmaDialogueEvent.dialogues[6].Condition = () => !firstDialogueDone && data != null;
            grandmaDialogueEvent.dialogues[6].OnDialogueComplete = () => {
                Debug.Log("방을 다시 둘러보자");
                firstDialogueDone = true;
                grandmaTocuhed = true; // 할머니 처음 클릭 여부 업데이트
                var data = SaveManager.Instance.Load();
                if (data != null) {
                    interactManager.SetTageAndLayoutAll();
                    Debug.Log("세이브 데이터 로드됨 - 상호작용 상태 복원");
                    if (posterEventCompleted) {
                        Debug.Log("포스터 이벤트 완료됨");
                        interactManager.SetNotTageAndLayoutPoster(); // 포스터 상호작용 비활성화
                    }
                    if (wardrobeEventCompleted) {
                        Debug.Log("옷장 이벤트 완료됨");
                        interactManager.SetNotTageAndLayoutWardrobe(); // 옷장 상호작용 비활성화
                    }
                    if (recordPlayerEventCompleted) {
                        Debug.Log("레코드 플레이어 이벤트 완료됨");
                        interactManager.SetNotTageAndLayoutRecordPlayer();
                    }
                }
            };
        }
    }

    void Update() {
        // 공용 조건 업데이트
        grandmaDialogueEvent.dialogues[1].Condition = () => !itemTouched && firstDialogueDone;

        // 포스터 관련 조건 업데이트
        grandmaDialogueEvent.dialogues[2].Condition = () => posterTouched && !secondDialogueDone && !posterEventDone;
        grandmaDialogueEvent.dialogues[3].Condition = () => posterTouched && !posterEventCompleted && !posterEventDone;
        grandmaDialogueEvent.dialogues[4].Condition = () => posterTouched && posterEventCompleted && !posterEventDone;

        // 레코드 플레이어 관련 조건 업데이트
        grandmaDialogueEvent.dialogues[5].Condition = () => recordPlayerTouched && !thirdDialogueDone && !recordPlayerEventCompleted;
    }
}
