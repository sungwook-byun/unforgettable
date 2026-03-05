using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class IntroScene : MonoBehaviour
{
    [Header("UI 참조")]
    public Image logoImage;
    public TMP_Text[] teamNames;

    [Header("시간 설정")]
    public float logoDropTime = 0.8f;       // 로고가 떨어지는 연출 시간
    public float nameFadeInterval = 0.2f;   // 다음 이름이 나타나기까지의 간격
    public float endWait = 0.8f;            // 모든 연출이 끝난 뒤 대기 시간

    [Header("로고 시작 위치 설정")]
    public Vector3 logoStartOffset = new Vector3(0f, 200f, 0f); // 로고가 위쪽에서 시작되도록 설정

    private Canvas _canvas;

    private bool _isInit; // 매니저들 초기화 완료 여부
    
    private void Awake()
    {
        StartCoroutine(InitManagers());
    }

    // 매니저 초기화 코루틴
    private IEnumerator InitManagers()
    {
        // 테이블 매니저 활성화
        var table = FindFirstObjectByType<TableManager>(FindObjectsInactive.Include);
        table.gameObject.SetActive(true);
        yield return new WaitUntil(() => table.gameObject.activeSelf);
        Debug.Log("테이블 매니저 활성화");
        
        // 로컬라이제이션 매니저 활성화
        var localization = FindFirstObjectByType<LocalizationManager>(FindObjectsInactive.Include);
        localization.gameObject.SetActive(true);
        yield return new WaitUntil(() => localization.gameObject.activeSelf);
        Debug.Log("로컬라이제이션 매니저 활성화");
        
        // 씬 매니저 활성화
        var scene = FindFirstObjectByType<GameSceneManager>(FindObjectsInactive.Include);
        scene.gameObject.SetActive(true);
        yield return new WaitUntil(() => scene.gameObject.activeSelf);
        Debug.Log("씬 매니저 활성화");

        // 오디오 매니저 활성화
        var audio = FindFirstObjectByType<AudioManager>(FindObjectsInactive.Include);
        audio.gameObject.SetActive(true);
        yield return new WaitUntil(() => audio.gameObject.activeSelf);
        Debug.Log("오디오 매니저 활성화");

        // 매니저 초기화 완료
        _isInit = true;
    }

    void Start()
    {
        StartCoroutine(PlayIntroSequence());
    }

    // 인트로 연출의 전체 흐름을 제어하는 코루틴
    private IEnumerator PlayIntroSequence()
    {
        // 매니저와 초기화될때까지 대기
        yield return new WaitUntil(() => _isInit);
        
        // 캔버스 활성화
        //var canvas = FindFirstObjectByType<Canvas>(FindObjectsInactive.Include);
        logoImage.transform.root.gameObject.SetActive(true);
        
        // RectTransform을 한 번만 가져와서 재사용
        RectTransform rect = logoImage.rectTransform;

        // 현재 로고의 목표 위치를 저장
        Vector3 targetPos = rect.anchoredPosition;

        // 시작 시 로고를 위쪽으로 이동시켜 놓고 투명하게 만듦
        rect.anchoredPosition = targetPos + logoStartOffset;
        logoImage.color = new Color(1, 1, 1, 0);

        // 팀 이름 텍스트도 모두 투명하게 초기화
        foreach (var t in teamNames)
            t.color = new Color(1, 1, 1, 0);

        // 로고가 위에서 천천히 떨어지며 등장하는 연출
        yield return StartCoroutine(DropLogo(rect, targetPos, logoDropTime));

        // 팀 이름이 순차적으로 나타나는 연출
        foreach (var t in teamNames)
        {
            yield return StartCoroutine(FadeText(t, 0f, 1f, 0.25f));
            yield return new WaitForSeconds(nameFadeInterval);
        }

        // 모든 연출이 끝나면 endWait후에 다음 씬으로 이동
        yield return new WaitForSeconds(endWait);
        GameSceneManager.Instance.OnIntroFinished();
    }

    // 로고가 위에서 아래로 떨어지며 등장하는 연출
    private IEnumerator DropLogo(RectTransform rect, Vector3 targetPos, float time)
    {
        if (time <= 0f) yield break; // 시간값이 잘못된 경우 즉시 종료하는 예외처리

        float elapsed = 0f;
        Vector3 startPos = rect.anchoredPosition;

        // 로고의 색상 변화용 변수
        Color c = logoImage.color;

        while (elapsed < time)
        {
            // 진행 비율 계산
            float t = elapsed / time;

            // 부드럽게 감속하며 멈추는 보간 방식
            t = Mathf.Sin(t * Mathf.PI * 0.5f);

            // 위치 보간
            rect.anchoredPosition = Vector3.Lerp(startPos, targetPos, t);

            // 투명도 보간
            logoImage.color = new Color(1, 1, 1, t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // 마지막 위치와 색상 고정
        rect.anchoredPosition = targetPos;
        logoImage.color = Color.white;
    }

    // 텍스트를 서서히 나타나게 하는 연출
    private IEnumerator FadeText(TMP_Text txt, float from, float to, float time)
    {
        if (time <= 0f) yield break; // 방어 코드, 잘못된 시간값 처리

        float elapsed = 0f;

        while (elapsed < time)
        {
            // 보간된 투명도 계산
            float a = Mathf.Lerp(from, to, elapsed / time);

            // 새로운 색상 적용
            txt.color = new Color(1, 1, 1, a);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // 마지막 알파값으로 고정
        txt.color = new Color(1, 1, 1, to);
    }
}
