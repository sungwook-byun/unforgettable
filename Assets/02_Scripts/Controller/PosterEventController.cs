using UnityEngine;
using static Contants;

public class PosterEventController : MonoBehaviour {
    public DialogueEvent posterDialogueEvent;
    private bool firstDialogueDone = false;
    private bool secondDigalogueDone = false;

    private InteractManager interactManager;

    private Animator animator;

    private void Awake() {
        interactManager = FindFirstObjectByType<InteractManager>();
        animator = GetComponent<Animator>();
    }

    void Start() {
        // 조건 및 완료 함수 연결
        if (posterDialogueEvent != null) {
            posterDialogueEvent.dialogues[0].Condition = () => grandmaTocuhed && !firstDialogueDone && !itemTouched;
            posterDialogueEvent.dialogues[0].OnDialogueComplete = () => {
                firstDialogueDone = true;
                posterTouched = true; // 포스터 처음 클릭 여부 업데이트
                itemTouched = true;   // 물건을 건드린 상태 업데이트
                interactManager.SetTageAndLayoutPoster(); // 포스터 상호작용 가능한 오브젝트 활성화
            };
            posterDialogueEvent.dialogues[1].Condition = () => grandmaTocuhed && !grandmaPosterEventStarted;
            posterDialogueEvent.dialogues[1].OnDialogueComplete = () => {
                Debug.Log("할머니한테 액자에 대해 물어보자.");
            };
            posterDialogueEvent.dialogues[2].Condition = () => grandmaTocuhed && !secondDigalogueDone && grandmaPosterEventStarted;
            posterDialogueEvent.dialogues[2].OnDialogueStart = () => {
                animator.SetTrigger("Move");
            };
            posterDialogueEvent.dialogues[2].OnDialogueComplete = () => {
                Debug.Log("액자를 제대로 걸어놨다.");
                secondDigalogueDone = true;
                posterEventCompleted = true; // 이벤트 처리 완료 여부 업데이트
            };
        }
    }

    void Update() {
        posterDialogueEvent.dialogues[0].Condition = () => grandmaTocuhed && !firstDialogueDone && !itemTouched;
        posterDialogueEvent.dialogues[1].Condition = () => grandmaTocuhed && !grandmaPosterEventStarted;
        posterDialogueEvent.dialogues[2].Condition = () => grandmaTocuhed && !secondDigalogueDone && grandmaPosterEventStarted;
    }

    private void HandleNextEvent() {
        // 두 번째 단계 이벤트 처리
        Debug.Log("다음 이벤트 실행!");
    }
}
