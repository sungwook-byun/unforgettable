using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

// 세이브 및 로드 담당 매니저 (UI 표시, 미션 로직 등은 별도 분리)
public class SaveManager : Singleton<SaveManager>
{
    private string savePath;
    private SaveData currentData;
    private SaveData preloadedData;

    private Transform playerTransform;

    // 저장 완료 시 호출되는 이벤트 (UI 등 외부에서 구독)
    public System.Action OnSaveComplete;

    protected override void Awake()
    {
        base.Awake();
        savePath = Path.Combine(Application.persistentDataPath, "save.json");

        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // 씬 로드 완료 시점에서는 자동 복원하지 않고 데이터만 미리 적용
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (preloadedData != null)
        {
            currentData = preloadedData;
            preloadedData = null;
        }
    }

    // 씬 로드 후 한 프레임 뒤에 복원하도록 별도 호출 (GameSceneManager가 호출)
    public IEnumerator RestoreAfterSceneLoad()
    {
        yield return new WaitForEndOfFrame();

        string currentScene = SceneManager.GetActiveScene().name;
        ApplyPlayerPosition(currentScene);
        ApplyRemovedObjects();
    }

    // 플레이어 등록 (위치 저장 및 복원용)
    public void RegisterPlayer(Transform player)
    {
        playerTransform = player;
    }

    // 자동 저장 실행 (코루틴으로 한 프레임 대기 후 안전 저장)
    public void AutoSave(bool isSceneTransition = false)
    {
        StartCoroutine(AutoSaveRoutine(isSceneTransition));
    }

    private IEnumerator AutoSaveRoutine(bool isSceneTransition)
    {
        yield return null;

        string currentScene = SceneManager.GetActiveScene().name;

        // 메인메뉴에서는 자동 저장을 수행하지 않음
        if (currentScene == SceneNames.MainMenu) yield break;

        if (currentData == null)
            currentData = new SaveData();

        currentData.sceneName = currentScene;
        currentData.localTimeString = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        // 플레이어 위치 저장
        if (playerTransform != null)
        {
            Vector3 pos = playerTransform.position;
            ScenePosition existing = currentData.scenePositions.Find(s => s.sceneName == currentScene);

            if (existing != null)
                existing.position = pos;
            else
                currentData.scenePositions.Add(new ScenePosition { sceneName = currentScene, position = pos });
        }

        string json = JsonUtility.ToJson(currentData, true);
        File.WriteAllText(savePath, json);
        OnSaveComplete?.Invoke();
    }

    // 세이브 파일 로드
    public SaveData Load()
    {
        if (!File.Exists(savePath))
            return null;

        string json = File.ReadAllText(savePath);
        currentData = JsonUtility.FromJson<SaveData>(json);
        return currentData;
    }

    // 로딩씬 진입 전 데이터 미리 읽어두기
    public void PreloadData()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            preloadedData = JsonUtility.FromJson<SaveData>(json);
        }
        else
        {
            preloadedData = null;
        }
    }

    // 세이브 데이터 즉시 적용 (Preload와 별개로 수동 복원 시 사용)
    public void ApplyLoadedData()
    {
        if (currentData == null)
        {
            Debug.LogWarning("세이브 데이터를 불러오지 못했습니다.");
            return;
        }

        string currentScene = SceneManager.GetActiveScene().name;
        ApplyPlayerPosition(currentScene);
        ApplyRemovedObjects();
    }

    // 플레이어 위치 복원 전용 함수
    private void ApplyPlayerPosition(string sceneName)
    {
        if (playerTransform == null)
            return;

        if (currentData == null)
            return;

        string activeScene = SceneManager.GetActiveScene().name;

        // MemoryWorld 복귀 시 문 위치(lastMemoryWorldPosition)로 복원
        if (activeScene == SceneNames.TestMemoryWorld)
        {
            if (currentData.lastMemoryWorldPosition != Vector3.zero)
            {
                Vector3 doorPos = currentData.lastMemoryWorldPosition;
                doorPos.y = 1f;
                playerTransform.position = doorPos;
            }
            return;
        }

        // GrandmaRoom에서는 씬 내부 저장 위치(scenePositions)로 복원
        if (activeScene == SceneNames.TestGrandmaRoom)
        {
            ScenePosition grandmaPos = currentData.scenePositions.Find(s => s.sceneName == SceneNames.TestGrandmaRoom);
            if (grandmaPos != null)
            {
                Vector3 pos = grandmaPos.position;
                pos.y = 1f;
                playerTransform.position = pos;
                return;
            }
        }

        // 기타 씬일 경우 마지막 저장 위치 사용
        if (currentData.lastMemoryWorldPosition != Vector3.zero)
        {
            Vector3 pos = currentData.lastMemoryWorldPosition;
            pos.y = 1f;
            playerTransform.position = pos;
        }
    }

    // 세이브 파일 삭제
    public void DeleteSave()
    {
        if (File.Exists(savePath))
            File.Delete(savePath);

        PlayerPrefs.DeleteAll();
        currentData = new SaveData();
    }

    // 제거된 오브젝트 기록 추가 (게임 로직에서 직접 호출)
    public void MarkObjectRemoved(string objectName)
    {
        if (currentData == null)
            currentData = new SaveData();

        if (!currentData.removedObjects.Contains(objectName))
            currentData.removedObjects.Add(objectName);
    }

    // 세이브 파일 존재 여부 확인
    public bool HasSaveData()
    {
        return File.Exists(savePath);
    }

    // 현재 세이브 데이터 직접 반환
    public SaveData GetCurrentData()
    {
        if (currentData == null)
            currentData = new SaveData();
        return currentData;
    }

    // 외부에서 전달된 데이터를 파일에 저장 (씬 이름, 시간, 위치까지 함께 기록)
    public void SaveData(SaveData data)
    {
        if (data == null)
            data = new SaveData();

        string currentScene = SceneManager.GetActiveScene().name;

        if (currentScene == SceneNames.MainMenu)
            return;

        currentData = data;
        currentData.sceneName = currentScene;
        currentData.localTimeString = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        if (playerTransform != null)
        {
            Vector3 pos = playerTransform.position;
            ScenePosition existing = currentData.scenePositions.Find(s => s.sceneName == currentScene);

            if (existing != null)
                existing.position = pos;
            else
                currentData.scenePositions.Add(new ScenePosition { sceneName = currentScene, position = pos });
        }

        string json = JsonUtility.ToJson(currentData, true);
        File.WriteAllText(savePath, json);
        OnSaveComplete?.Invoke();
    }

    // 제거된 오브젝트 복원 전용 함수
    private void ApplyRemovedObjects()
    {
        if (currentData == null || currentData.removedObjects == null)
            return;

        foreach (string objName in currentData.removedObjects)
        {
            GameObject target = GameObject.Find(objName) ?? GameObject.Find(objName + "(Clone)");
            if (target != null)
                target.SetActive(false);
        }
    }

    // 씬 로드 직후 한 프레임 기다리지 않고 즉시 플레이어 위치를 복원하는 함수
    public void ApplyPositionImmediately()
    {
        if (playerTransform == null)
            return;

        string currentScene = SceneManager.GetActiveScene().name;
        ApplyPlayerPosition(currentScene);
    }

    public void SetCurrentData(SaveData data)
    {
        currentData = data;
    }
}
