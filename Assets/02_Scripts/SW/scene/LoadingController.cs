using UnityEngine;
using TMPro;
using System.Collections;

public class LoadingController : MonoBehaviour
{
    [Header("UI 참조")]
    [SerializeField] private TextMeshProUGUI loadingText;
    [SerializeField] private TextMeshProUGUI clickText;

    [Header("설정")]
    [SerializeField] private float typingSpeed = 0.05f;
    [SerializeField] private float fadeSpeed = 1.5f;

    private string fullText;
    private Coroutine fadeRoutine;

    // 클릭 입력 여부를 다른 클래스가 확인할 수 있도록 static 변수로 관리
    public static bool HasClicked { get; private set; } = false;

    // 클릭 텍스트가 다 표시된 뒤부터 클릭을 허용하기 위한 변수
    public static bool IsReadyToClick { get; private set; } = false;

    void Start()
    {
        fullText = loadingText.text;
        loadingText.text = "";
        clickText.gameObject.SetActive(false);
        HasClicked = false;

        // 클릭 가능 여부 초기화
        IsReadyToClick = false;

        StartCoroutine(TypeAndWaitRoutine());
    }

    // 비동기 로드를 직접 수행하지 않고, GameSceneManager의 진행을 기다리는 형태로 변경
    private IEnumerator TypeAndWaitRoutine()
    {
        // 텍스트 타이핑 연출
        foreach (char c in fullText)
        {
            loadingText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        // 로딩씬 표시 후 잠시 대기
        yield return new WaitForSeconds(0.3f);

        // 클릭 텍스트 표시 및 점멸 시작
        clickText.gameObject.SetActive(true);
        fadeRoutine = StartCoroutine(FadeClickText());

        // 클릭 텍스트가 표시된 뒤 약간의 대기 후 클릭 허용
        yield return new WaitForSeconds(0.3f);
        IsReadyToClick = true;

        // 클릭 대기
        yield return new WaitUntil(() => Input.GetMouseButtonDown(0) && IsReadyToClick);

        // 클릭을 놓을 때까지 대기 (다음 씬에서 중복 클릭 방지)
        yield return new WaitUntil(() => Input.GetMouseButtonUp(0));

        // 클릭 시 점멸 정지 및 표시 유지
        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        var group = clickText.GetComponent<CanvasGroup>();
        if (group != null)
            group.alpha = 1f;

        // 클릭 신호를 GameSceneManager에 전달
        HasClicked = true;
    }

    // 클릭 텍스트 점멸 연출
    private IEnumerator FadeClickText()
    {
        CanvasGroup group = clickText.GetComponent<CanvasGroup>();
        if (group == null)
            group = clickText.gameObject.AddComponent<CanvasGroup>();

        float alpha = 0f;
        bool fadingIn = true;

        while (gameObject.activeInHierarchy)
        {
            if (fadingIn)
                alpha += Time.deltaTime * fadeSpeed;
            else
                alpha -= Time.deltaTime * fadeSpeed;

            alpha = Mathf.Clamp01(alpha);
            group.alpha = alpha;

            if (alpha >= 1f) fadingIn = false;
            else if (alpha <= 0f) fadingIn = true;

            yield return null;
        }
    }
}
