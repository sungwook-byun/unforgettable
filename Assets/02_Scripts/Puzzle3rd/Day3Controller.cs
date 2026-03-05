using UnityEngine;
using System.Collections.Generic;


[System.Serializable]
public class DioramaStep
{
    [Tooltip("이 오브젝트의 게임 오브젝트 이름 (정확히 일치해야 함)")]
    public string dioramaName;          
    public GameObject dioramaObject;      
    public SoundNode soundNode;           
    public DialogueScriptable successDialogue; 
    
    [Header("사운드 노드 설정")]
    [Tooltip("이 노드가 활성화될 때의 최대 볼륨 범위")]
    public float activeRange = 20f;
    [Tooltip("이 노드가 활성화될 때의 소리 소실 범위")]
    public float fadeRange = 40f;
}

public class Day3Controller : Singleton<Day3Controller>
{
    [Header("퍼즐 디오라마 (순서대로)")]
    [SerializeField] private List<DioramaStep> puzzleSteps;

    [Header("플레이어 및 리셋")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform puzzleStartPoint; // 퍼즐 실패 시 돌아올 위치

    [Header("퍼즐 연출 및 효과")]
    [SerializeField] private DialogueScriptable failDialogue; // 순서 틀렸을 때 나올 대사
    [SerializeField] private GameObject successVFXPrefab; 
    [SerializeField] private GameObject nextDayTriggerNode; // 4일차로 가는 노드

    private int currentStep = 0; // 0=시작, 1=사진완료, 2=재봉틀완료, 3=클리어
    private bool isPuzzleComplete = false;


    public bool IsPuzzleComplete() => isPuzzleComplete;

    public void ActivateObjectsFromNode()
    {
        ActivatePuzzle();
    }

    // 퍼즐 활성화 (외부 또는 Start에서 호출)
    public void ActivatePuzzle()
    {
        // (세이브 데이터 로드 로직이 있다면 여기서 currentStep을 불러옵니다)
        // 예: currentStep = SaveManager.Instance.GetCurrentData().day3Step;

        isPuzzleComplete = false;
        
        // 모든 디오라마 오브젝트를 활성화합니다.
        foreach (var step in puzzleSteps)
        {
            if(step.dioramaObject != null)
                step.dioramaObject.SetActive(true);
        }
        
        // 현재 단계(0단계)에 맞게 소리와 상호작용을 설정합니다.
        UpdatePuzzleState();
    }

    // Day3DioramaTrigger가 'E'키를 누르면 이 함수를 호출합니다.
    public void OnDioramaInteracted(string dioramaName)
    {
        // 이미 퍼즐이 끝났거나, 대화 중이면 무시
        if (isPuzzleComplete || (DialogueSystem.Instance != null && DialogueSystem.Instance.IsDialogueActive))
            return;

        // 1. 현재 단계의 정답 오브젝트 이름과 클릭한 오브젝트 이름이 일치하는지 확인
        if (currentStep < puzzleSteps.Count && dioramaName == puzzleSteps[currentStep].dioramaName)
        {
            // [정답]
            DialogueScriptable successDialogue = puzzleSteps[currentStep].successDialogue;
            
            // 성공 대사를 재생하고, 대사가 끝나면 다음 단계로 상태를 넘깁니다.
            if (successDialogue != null && DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.StartDialogue(successDialogue, () => {

                    currentStep++;
                    UpdatePuzzleState(); 
                });
            }
            else 
            {
                currentStep++;
                UpdatePuzzleState();
            }
        }
        else if (currentStep < puzzleSteps.Count)
        {
            
            ResetPuzzle();
        }
    }


    private void UpdatePuzzleState()
    {
        // (현재 단계를 세이브하는 로직이 있다면 여기에 추가)
        // 예: SaveManager.Instance.GetCurrentData().day3Step = currentStep;

        if (currentStep >= puzzleSteps.Count)
        {

            isPuzzleComplete = true;
            PuzzleComplete();
            return;
        }

        // 모든 사운드 노드를 끕니다.
        foreach (var step in puzzleSteps)
        {
            if (step.soundNode != null) 
                step.soundNode.gameObject.SetActive(false);
            

            Day3DioramaTrigger trigger = step.dioramaObject.GetComponent<Day3DioramaTrigger>();
            if (trigger != null) trigger.enabled = false;
        }

        // --- 현재 단계의 오브젝트만 켭니다 ---
        DioramaStep currentStepData = puzzleSteps[currentStep];

        // 1. 현재 단계의 소리 등대만 켭니다.
        if (currentStepData.soundNode != null)
        {
            currentStepData.soundNode.gameObject.SetActive(true);
            currentStepData.soundNode.activeRange = currentStepData.activeRange;
            currentStepData.soundNode.fadeRange = currentStepData.fadeRange;
        }
        
        // 2. 현재 단계의 Day3DioramaTrigger만 켭니다.
        Day3DioramaTrigger currentTrigger = currentStepData.dioramaObject.GetComponent<Day3DioramaTrigger>();
        if (currentTrigger != null) currentTrigger.enabled = true;
    }

    private void PuzzleComplete()
    {
        // 1. 모든 소리 끄기
        foreach (var step in puzzleSteps)
        {
            if (step.soundNode != null) 
                step.soundNode.gameObject.SetActive(false);
        }


        if (successVFXPrefab != null)
        {
            foreach (var step in puzzleSteps)
            {
                if (step.dioramaObject != null)
                {
                    Instantiate(successVFXPrefab, step.dioramaObject.transform.position, Quaternion.identity);
                }
            }
        }

        //4일차로 가는 노드 활성화
        if (nextDayTriggerNode != null) 
            nextDayTriggerNode.SetActive(true);
    }

    private void ResetPuzzle()
    {
        // "틀렸습니다" 대사를 출력
        if (failDialogue != null && DialogueSystem.Instance != null)
        {
            DialogueSystem.Instance.StartDialogue(failDialogue, () => {
                // 대사가 끝나면 플레이어를 시작 지점으로 되돌림
                if (player != null && puzzleStartPoint != null)
                {
                    player.position = puzzleStartPoint.position;
                }
                currentStep = 0;
                UpdatePuzzleState(); // 퍼즐 상태를 0단계로 초기화 (0번 소리 다시 켬)
            });
        }
    }
}