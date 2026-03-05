using System.Collections;
using UnityEngine;

public class GrandmaEventManager3 : MonoBehaviour {
    [SerializeField] private float delayTime = 5f;

    void Start() {
        StartCoroutine(DelayedInit());
    }

    private IEnumerator DelayedInit() {
        yield return new WaitForSeconds(0.3f);

        var data = SaveManager.Instance.Load();
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        if (data == null) {
            // 세이브 데이터가 없을 경우 새 게임 초기화
            Debug.Log(data);
            yield break;
        } else {
            Debug.Log("Loaded Save Data: " + data.sceneName + ", Diary Mode: " + data.isDiaryMode);
        }

        // MemoryWorld → GrandmaRoom 복귀만 밤으로 처리
        if (data.sceneName == SceneNames.TestMemoryWorld && currentScene == SceneNames.GrandmaRoom) {
            data.isDiaryMode = true; // 복귀 시 밤으로 전환
            SaveManager.Instance.SaveData(data);
            SaveManager.Instance.SetCurrentData(data);
            Debug.Log("Memory World에서 복귀 - 밤 상태로 설정");
        } else {
            // 단순 로드는 세이브된 상태 그대로 유지
            SaveManager.Instance.SetCurrentData(data);
        }
    }
}
