using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameSceneManager : Singleton<GameSceneManager>
{
    private string nextSceneName;
    private string currentSceneName;
    private AsyncOperation loadOperation;

    protected override void Awake()
    {
        base.Awake();
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // 씬 로드 완료 후 SaveManager의 복원 코루틴을 호출하도록 변경
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        currentSceneName = scene.name;

        // scene.name == SceneNames.DevPuzzle3rd 추가하였음 윤
        if (scene.name == SceneNames.TestGrandmaRoom || scene.name == SceneNames.TestMemoryWorld || scene.name == SceneNames.Day3MemoryWorld)
        {
            // 씬 로드 직후 즉시 위치 복원 (이동 장면 안 보이게)
            SaveManager.Instance.ApplyPositionImmediately();

            // 이후 한 프레임 뒤 추가 복원 및 자동 저장 실행
            StartCoroutine(SaveManager.Instance.RestoreAfterSceneLoad());
            StartCoroutine(ApplySaveDataThenAutoSave());
        }
    }

    // 씬 로드 후 데이터 복원 및 자동 저장 처리
    private IEnumerator ApplySaveDataThenAutoSave()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForSeconds(0.3f);

        var saveMgr = SaveManager.Instance;
        if (saveMgr == null)
            yield break;

        var data = saveMgr.Load();
        if (data == null)
            yield break;

        string currentScene = SceneManager.GetActiveScene().name;

        // scene.name == SceneNames.DevPuzzle3rd 추가하였음 윤
        if (currentScene == SceneNames.TestMemoryWorld || currentScene == SceneNames.Day3MemoryWorld)
            yield break;

        if (data.isDiaryMode)
            yield break;

        saveMgr.AutoSave(true);
    }

    public void LoadSceneWithLoading(string targetScene)
    {
        nextSceneName = targetScene;
        StartCoroutine(LoadSceneAsyncRoutine());
    }

    private IEnumerator LoadSceneAsyncRoutine()
    {
        if (loadOperation != null)
            yield break;

        SceneManager.LoadScene(SceneNames.Loading);
        yield return null;

        loadOperation = SceneManager.LoadSceneAsync(nextSceneName);
        loadOperation.allowSceneActivation = false;

        SaveManager.Instance.PreloadData();

        while (loadOperation.progress < 0.9f)
            yield return null;

        yield return new WaitUntil(() => LoadingController.HasClicked);

        loadOperation.allowSceneActivation = true;
        loadOperation = null;
    }

    public void OnIntroFinished()
    {
        SceneManager.LoadScene(SceneNames.MainMenu);
    }
}
