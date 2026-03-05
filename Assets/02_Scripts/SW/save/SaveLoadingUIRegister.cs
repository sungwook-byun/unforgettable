using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

// 씬마다 존재하는 세이브 UI 프리팹을 SaveManager에 연결
public class SaveLoadingUIRegister : MonoBehaviour
{
    private CanvasGroup canvasGroup;
    private bool isSubscribed = false; // 이벤트 중복 구독 방지용

    IEnumerator Start()
    {
        // 로딩씬에서는 등록하지 않음
        if (SceneManager.GetActiveScene().name == SceneNames.Loading)
            yield break;

        // SaveManager가 완전히 생성될 때까지 대기
        yield return new WaitUntil(() => SaveManager.InstanceExists && SaveManager.Instance != null);

        // 이벤트 구독 (한 번만)
        if (!isSubscribed)
        {
            SaveManager.Instance.OnSaveComplete += HandleSaveUIFade;
            isSubscribed = true;
        }

        // CanvasGroup 설정
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        canvasGroup.alpha = 0f;
    }

    void OnDestroy()
    {
        // 애플리케이션이 종료 중이거나 SaveManager가 이미 파괴된 경우 안전하게 종료
        if (!Application.isPlaying || !SaveManager.InstanceExists)
            return;

        if (isSubscribed && SaveManager.Instance != null)
        {
            SaveManager.Instance.OnSaveComplete -= HandleSaveUIFade;
            isSubscribed = false;
        }
    }

    // SaveManager에서 저장 완료 이벤트가 발생하면 호출
    private void HandleSaveUIFade()
    {
        if (canvasGroup == null) return;
        StopAllCoroutines();
        StartCoroutine(FadeUIRoutine());
    }

    // 페이드 인-유지-아웃 애니메이션 루틴
    private IEnumerator FadeUIRoutine()
    {
        float fadeInTime = 0.5f;
        float fadeOutTime = 0.5f;
        float visibleTime = 3f;
        float timer = 0f;

        // 페이드 인
        while (timer < fadeInTime)
        {
            timer += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, timer / fadeInTime);
            yield return null;
        }

        // 유지
        canvasGroup.alpha = 1f;
        yield return new WaitForSeconds(visibleTime);

        // 페이드 아웃
        timer = 0f;
        while (timer < fadeOutTime)
        {
            timer += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, timer / fadeOutTime);
            yield return null;
        }

        canvasGroup.alpha = 0f;
    }
}