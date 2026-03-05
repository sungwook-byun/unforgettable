using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrandmaEventManager2 : MonoBehaviour
{
    [SerializeField] private HighlightSystem2 highlightSystem;
    [SerializeField] private float delayTime = 5f;
    [SerializeField] private List<RequestEvent> requestEvents = new List<RequestEvent>();

    private List<RequestEvent> remainingRequests = new List<RequestEvent>();
    private RequestEvent currentRequest;

    private bool isRequest = false;
    private bool canChangeScene = false;
    private int requestCountInThisScene = 0;
    private const int maxRequestsPerScene = 2;

    // 밤 상태 전환 시점 조기 적용
    void Awake()
    {
        var data = SaveManager.Instance.Load();
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        // Memory World → Grandma Room 으로 복귀할 때만 밤으로 설정
        if (data != null && data.sceneName == SceneNames.TestMemoryWorld && currentScene == SceneNames.TestGrandmaRoom)
        {
            var light = FindFirstObjectByType<Light>(FindObjectsInactive.Include);
            if (light != null)
                light.color = Color.gray;
        }
    }


    void Start()
    {
        StartCoroutine(DelayedInit());
    }

    private IEnumerator DelayedInit()
    {
        yield return new WaitForSeconds(0.3f);

        var data = SaveManager.Instance.Load();
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        // 세이브 데이터가 없을 경우 새 게임 초기화 (첫 진입은 낮)
        if (data == null)
        {
            remainingRequests = new List<RequestEvent>(requestEvents);
            yield return new WaitForSeconds(delayTime);
            StartRequest(); // 첫 Grandma Room 진입 시 바로 미션 시작
            yield break;
        }

        // MemoryWorld → GrandmaRoom 복귀만 밤으로 처리
        if (data.sceneName == SceneNames.TestMemoryWorld && currentScene == SceneNames.TestGrandmaRoom)
        {
            data.requestsClearedThisScene = 0;
            data.isRequestActive = false;
            data.currentRequestId = -1;
            data.currentObjectName = "";
            data.isDiaryMode = true; // 복귀 시 밤으로 전환
            SaveManager.Instance.SaveData(data);
            SaveManager.Instance.SetCurrentData(data);
            Debug.Log("Memory World에서 복귀 - 밤 상태로 설정");
        }
        else
        {
            // 단순 로드는 세이브된 상태 그대로 유지
            SaveManager.Instance.SetCurrentData(data);
        }

        // 밤 상태라면 미션 시작 생략
        if (data.isDiaryMode)
        {
            Debug.Log("밤 상태로 진입 - 미션은 시작하지 않습니다");
            yield break;
        }

        // 완료된 요청 리스트 불러오기
        List<string> cleared = data.clearedEvents ?? new List<string>();

        remainingRequests.Clear();
        foreach (var r in requestEvents)
        {
            if (!cleared.Contains(r.objectName))
                remainingRequests.Add(r);
        }

        requestCountInThisScene = data.requestsClearedThisScene;

        if (requestCountInThisScene >= maxRequestsPerScene)
        {
            canChangeScene = true;
            Debug.Log("이미 두 개의 요청을 완료했습니다 Memory World로 이동이 가능합니다");
            yield break;
        }

        if (data.isRequestActive && data.currentRequestId >= 0)
        {
            RestartPreviousRequest(data.currentRequestId, data.currentObjectName);
        }
        else
        {
            Debug.Log("씬 진입 완료 미션은 낮으로 전환 시점에 시작됩니다");
            yield return new WaitForSeconds(delayTime);
            StartRequest();
        }
    }

    IEnumerator RequestRoutine()
    {
        yield return new WaitForSeconds(delayTime);

        if (requestCountInThisScene >= maxRequestsPerScene)
        {
            Debug.Log("이번 씬에서 더 이상 요청이 없습니다");
            yield break;
        }

        StartRequest();
    }

    void StartRequest()
    {
        if (isRequest) return;
        if (remainingRequests.Count == 0)
        {
            Debug.Log("모든 요청을 완료했습니다 더 이상 요청이 없습니다");
            return;
        }

        isRequest = true;

        int randomIndex = Random.Range(0, remainingRequests.Count);
        currentRequest = remainingRequests[randomIndex];
        remainingRequests.RemoveAt(randomIndex);

        highlightSystem.ResetTimer();
        highlightSystem.TargetObject();

        SaveCurrentMissionState();
        Debug.Log("할머니 요청 발생 " + currentRequest.text);
    }

    public void CompleteRequest()
    {
        if (!isRequest || currentRequest == null) return;

        isRequest = false;

        // 이 시점의 요청 이름을 명시적으로 찍기
        Debug.Log("요청 완료됨: " + currentRequest.objectName);

        var data = SaveManager.Instance.GetCurrentData();
        if (data != null && currentRequest != null)
        {
            if (!data.clearedEvents.Contains(currentRequest.objectName))
                data.clearedEvents.Add(currentRequest.objectName);

            if (currentRequest.pickUp)
            {
                GameObject target = GameObject.Find(currentRequest.objectName);
                if (target != null)
                {
                    SaveManager.Instance.MarkObjectRemoved(target.name);
                    target.SetActive(false);
                }
            }
        }

        requestCountInThisScene++;
        SaveCurrentMissionState();

        if (requestCountInThisScene >= maxRequestsPerScene)
        {
            StartCoroutine(EnableSceneChangeAfterDelay());
            return;
        }

        StartCoroutine(RequestRoutine());
    }

    private IEnumerator EnableSceneChangeAfterDelay()
    {
        Debug.Log("미션 완료 일정 시간 후 씬 이동 가능 상태로 전환됩니다");

        float timer = 0f;
        bool logged = false; // 추가 - 씬 이동 가능 로그가 중복되지 않도록 제어

        while (timer < delayTime)
        {
            // 수정 - 오브젝트가 파괴되기 전까지 타이머를 유지하도록 보강
            if (this == null || gameObject == null)
            {
                Debug.LogWarning("GrandmaEventManager2 파괴 감지됨, 코루틴 중단 전 로그 보정");
                break;
            }

            timer += Time.deltaTime;
            yield return null;
        }

        // 수정 - 코루틴이 중단되기 직전에도 로그를 남김
        if (!logged)
        {
            Debug.Log("씬 이동이 가능합니다");
            logged = true;
        }

        canChangeScene = true;
    }

    private void RestartPreviousRequest(int id, string objectName)
    {
        currentRequest = requestEvents.Find(r => r.id == id || r.objectName == objectName);
        if (currentRequest == null)
        {
            Debug.LogWarning("이전 미션 재시작 실패 요청을 찾을 수 없습니다");
            isRequest = false;
            return;
        }

        isRequest = true;

        highlightSystem.ResetTimer();
        highlightSystem.TargetObject();

        Debug.Log("할머니 요청 발생 " + currentRequest.text);

        SaveCurrentMissionState();
    }

    private void SaveCurrentMissionState()
    {
        var data = SaveManager.Instance.GetCurrentData();
        if (data == null) return;

        data.isRequestActive = isRequest;
        data.currentRequestId = currentRequest != null ? currentRequest.id : -1;
        data.currentObjectName = currentRequest != null ? currentRequest.objectName : "";
        data.requestsClearedThisScene = requestCountInThisScene;

        SaveManager.Instance.SaveData(data);
    }

    // 낮 전환 시 다이어리 매니저에서 호출할 함수
    public void StartNextDayMission()
    {
        requestCountInThisScene = 0;
        canChangeScene = false;
        isRequest = false;

        var data = SaveManager.Instance.GetCurrentData();
        if (data != null)
        {
            data.requestsClearedThisScene = 0;
            data.isRequestActive = false;
            data.currentRequestId = -1;
            data.currentObjectName = "";

            // 남은 요청 리스트를 클리어 후, 클리어되지 않은 요청만 추가
            remainingRequests.Clear();
            foreach (var r in requestEvents)
            {
                if (!data.clearedEvents.Contains(r.objectName))
                    remainingRequests.Add(r);
            }

            SaveManager.Instance.SaveData(data);
        }

        if (remainingRequests.Count == 0)
        {
            Debug.Log("모든 요청을 완료했습니다 더 이상 요청이 없습니다");
            return;
        }

        if (isRequest) return;

        Debug.Log("다음 날 미션 시작");
        StartCoroutine(RequestRoutine()); // 기존 요청 시작 로직 재사용
    }

    public bool IsRequest() => isRequest;
    public string GetObjectName() => currentRequest != null ? currentRequest.objectName : null;
    public bool GetPickUp() => currentRequest != null ? currentRequest.pickUp : false;
    public bool CanChangeScene() => canChangeScene;
}

[System.Serializable]
public class RequestEvent
{
    public int id;
    public string text;
    public string objectName;
    public bool pickUp;
}
