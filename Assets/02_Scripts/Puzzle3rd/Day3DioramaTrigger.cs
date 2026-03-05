using UnityEngine;

// 3일차 디오라마 3개에만 부착할 전용 트리거입니다.
public class Day3DioramaTrigger : MonoBehaviour
{
    [SerializeField] private string interactPrompt = "E키를 눌러 상호작용하기";

    private bool isPlayerInRange = false;
    private DialogueSystem dialogueSystem; // UI 표시/숨기기용

    private void Start()
    {
        // DialogueSystem의 UI 기능만 사용하기 위해 참조
        dialogueSystem = DialogueSystem.Instance;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // Day3Controller가 켜져있고, 퍼즐이 아직 안 끝났을 때만
        if (Day3Controller.Instance != null && !Day3Controller.Instance.IsPuzzleComplete())
        {
            isPlayerInRange = true;
            if (dialogueSystem != null)
                dialogueSystem.ShowInteractUI(interactPrompt);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        isPlayerInRange = false;

        if (dialogueSystem != null)
            dialogueSystem.HideInteractUI();
    }

    private void Update()
    {
        if (!isPlayerInRange || dialogueSystem == null) return;
        if (Time.time < dialogueSystem.InputLockedUntil) return; // 입력 잠금 확인
        if (dialogueSystem.IsDialogueActive) return; // 대화 중에는 입력 방지

        if (Input.GetKeyDown(KeyCode.E))
        {
            // [핵심] DialogueSystem을 직접 호출하는 대신 Day3Controller를 호출합니다.
            if (Day3Controller.Instance != null)
            {
                // Day3Controller에게 이 오브젝트의 이름을 전달하여 퍼즐 로직 실행
                Day3Controller.Instance.OnDioramaInteracted(this.gameObject.name);

                // 상호작용 UI 즉시 숨기기 (Day3Controller가 대화를 띄울 것이므로)
                dialogueSystem.HideInteractUI();
                isPlayerInRange = false; // 상호작용 후 범위 밖으로 나간 것으로 처리
            }
        }
    }
}