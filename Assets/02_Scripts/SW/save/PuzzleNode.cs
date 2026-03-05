using UnityEngine;

public class PuzzleNode : MonoBehaviour
{
    [Header("플레이어 설정")]
    [SerializeField] private Transform player;
    [SerializeField] private float interactDistance = 3f;

    [Header("Day 컨트롤러들")]
    [SerializeField] private Day1Controller day1Controller;
    [SerializeField] private Day2Controller day2Controller;
    [SerializeField] private Day3Controller day3Controller;
    // [SerializeField] private Day4Controller day4Controller;

    [Header("노드 위치 설정")]
    [SerializeField] private Transform[] triggerNodes; // Day1~4 각각 대응할 노드 위치

    private bool[] isUsed; // 각 노드별 사용 여부 캐싱
    private Transform lastActivatedNode; // 마지막으로 E로 활성화한 노드 기억

    void Start()
    {
        int length = triggerNodes.Length;
        isUsed = new bool[length];

        var data = SaveManager.Instance.GetCurrentData();
        if (data != null)
        {
            for (int i = 0; i < length; i++)
            {
                if (data.clearedEvents.Contains(triggerNodes[i].name))
                    isUsed[i] = true;
            }
        }
    }

    void Update()
    {
        if (player == null || !Input.GetKeyDown(KeyCode.E))
            return;

        CheckInteraction();
    }

    // 플레이어 주변의 트리거 노드들을 확인하고 해당 Day를 활성화
    private void CheckInteraction()
    {
        for (int i = 0; i < triggerNodes.Length; i++)
        {
            if (isUsed[i]) continue;

            float dist = Vector3.Distance(player.position, triggerNodes[i].position);
            if (dist <= interactDistance)
            {
                ActivateCorrespondingDay(i);
                isUsed[i] = true;
                SaveNodeState(triggerNodes[i].name);

                // 마지막으로 활성화한 노드 위치 저장
                lastActivatedNode = triggerNodes[i];
                SaveLastNodePosition(triggerNodes[i].position);

                break;
            }
        }
    }

    // 인덱스에 따라 해당 Day 컨트롤러 활성화
    private void ActivateCorrespondingDay(int index)
    {
        switch (index)
        {
            case 0:
                if (day1Controller != null) day1Controller.ActivateObjectsFromNode();
                break;
            case 1:
                if (day2Controller != null) day2Controller.ActivateObjectsFromNode();
                return;
            case 2:
                if (day3Controller != null) day3Controller.ActivateObjectsFromNode();
                return;
            case 3:
                return;
        }
    }

    // 노드 사용 여부를 세이브 데이터에 기록
    private void SaveNodeState(string nodeName)
    {
        var data = SaveManager.Instance.GetCurrentData();
        if (data != null && !data.clearedEvents.Contains(nodeName))
        {
            data.clearedEvents.Add(nodeName);
            SaveManager.Instance.SaveData(data);
        }
    }

    // 마지막으로 활성화한 노드의 위치를 세이브 데이터에 기록
    private void SaveLastNodePosition(Vector3 nodePosition)
    {
        var data = SaveManager.Instance.GetCurrentData();
        if (data != null)
        {
            // 항상 Y좌표를 1로 고정
            nodePosition.y = 1f;
            data.lastMemoryWorldPosition = nodePosition;
            SaveManager.Instance.SaveData(data);
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        if (triggerNodes == null) return;

        foreach (var t in triggerNodes)
        {
            if (t != null)
                Gizmos.DrawWireSphere(t.position, interactDistance);
        }
    }
}
