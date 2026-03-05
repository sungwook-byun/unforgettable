using UnityEngine;

// 3일차 퍼즐을 완료한 후 활성화되는 '출구' 스크립트입니다.
public class EndNodeTrigger : MonoBehaviour
{
    [Header("설정")]
    [SerializeField] private string interactPrompt = "E키를 눌러 나아가기";
    [Tooltip("이동할 다음 씬의 이름을 SceneNames에서 가져옵니다.")]
    
    // 할머니 방 씬 넘기기용 씬집어넣기
    [SerializeField] private string nextSceneName = SceneNames.TestGrandmaRoom; 

    private bool isPlayerInRange = false;
    private DialogueSystem dialogueSystem;

    private void Start()
    {
        dialogueSystem = DialogueSystem.Instance;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        isPlayerInRange = true;
        if (dialogueSystem != null)
            dialogueSystem.ShowInteractUI(interactPrompt);
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
        if (Time.time < dialogueSystem.InputLockedUntil) return; 
        if (dialogueSystem.IsDialogueActive) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            // ToDO 씬 넘어가기 전 페이드아웃
            // FadeInBackGround.Instance.StartFadeIn(1f); 
            
            // GameSceneManager를 통해 다음 씬을 로드합니다.
            if (GameSceneManager.Instance != null)
            {
                GameSceneManager.Instance.LoadSceneWithLoading(nextSceneName);
            }
        }
    }
}