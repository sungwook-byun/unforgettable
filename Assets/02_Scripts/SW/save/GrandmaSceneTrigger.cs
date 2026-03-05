using UnityEngine;
using System.Collections;

public class GrandmaSceneTrigger : MonoBehaviour
{
    [Header("참조 설정")]
    [SerializeField] private GrandmaEventManager2 grandmaEventManager;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Transform grandmaTransform;
    [SerializeField] private float interactDistance = 2f; // 플레이어가 할머니와 접근해야 하는 거리

    private bool canInteract = false;
    private bool inputLock = false;

    void Update()
    {
        if (playerTransform == null || grandmaTransform == null || grandmaEventManager == null) return; // 참조가 없으면 종료하는 예외처리

        float distance = Vector3.Distance(playerTransform.position, grandmaTransform.position); // 플레이어와 할머니 간 거리를 계산
        canInteract = distance <= interactDistance; // 거리가 설정된 interactDistance 이하라면 상호작용 true 상태로 전환

        if (inputLock) return; // 씬 이동 도중 중복 입력 방지

        // 거리를 충족하고 e 키를 눌렀을 때 씬 전환 시도
        if (canInteract && Input.GetKeyDown(KeyCode.E))
        {
            TrySceneChange();
        }
    }

    void TrySceneChange()
    {
        if (grandmaEventManager.CanChangeScene())
        {
            Debug.Log("씬 이동 조건 충족 Memory World로 이동합니다");
            GameSceneManager.Instance.LoadSceneWithLoading(SceneNames.TestMemoryWorld);
        }
    }
}