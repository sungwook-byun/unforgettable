using UnityEngine;
using UnityEngine.UI;

public class FilterUI : MonoBehaviour {
    [Header("필터 이미지 연결")]
    [SerializeField] private Image noneFilter;
    [SerializeField] private Image grayscaleFilter;
    [SerializeField] private Image vignetteFilter;

    [Header("Vignette Settings")]
    public float vignetteMin = 0.33f;
    public float vignetteMax = 0.56f;
    public float vignetteSpeed = 0.05f;


    public enum FilterType {
        None,       // 기본
        Grayscale, // 흑백
        Vignette    // 비네팅
    }

    private FilterType currentFilter = FilterType.None;
    private FilterEffect filterEffect;

    private void Awake() {
        filterEffect = Camera.main.GetComponent<FilterEffect>();
    }

    public void SetFilter(FilterType type) {
        currentFilter = type;

        // 모든 필터 비활성화
        noneFilter.gameObject.SetActive(false);
        grayscaleFilter.gameObject.SetActive(false);
        vignetteFilter.gameObject.SetActive(false);

        // 선택한 필터만 활성화
        switch (type) {
            case FilterType.None:
                noneFilter.gameObject.SetActive(true);
                filterEffect?.ApplyFilter(FilterType.None);
                break;

            case FilterType.Grayscale:
                grayscaleFilter.gameObject.SetActive(true);
                filterEffect?.ApplyFilter(FilterType.Grayscale);
                break;

            case FilterType.Vignette:
                vignetteFilter.gameObject.SetActive(true);
                filterEffect?.ApplyFilter(FilterType.Vignette);
                break;
        }
    }

    public FilterType GetCurrentFilter() => currentFilter;

    // 필요시 페이드 인/아웃 구현 가능
    public void FadeIn(float duration) {
        StartCoroutine(FadeCanvasGroup(GetActiveImageCanvasGroup(), 0f, 1f, duration));
    }

    public void FadeOut(float duration) {
        StartCoroutine(FadeCanvasGroup(GetActiveImageCanvasGroup(), 1f, 0f, duration));
    }

    private CanvasGroup GetActiveImageCanvasGroup() {
        Image active = null;
        switch (currentFilter) {
            case FilterType.None: active = noneFilter; break;
            case FilterType.Grayscale: active = grayscaleFilter; break;
            case FilterType.Vignette: active = vignetteFilter; break;
        }

        CanvasGroup cg = active.GetComponent<CanvasGroup>();
        if (cg == null) cg = active.gameObject.AddComponent<CanvasGroup>();
        return cg;
    }

    private System.Collections.IEnumerator FadeCanvasGroup(CanvasGroup cg, float start, float end, float duration) {
        float elapsed = 0f;
        while (elapsed < duration) {
            elapsed += Time.deltaTime;
            cg.alpha = Mathf.Lerp(start, end, elapsed / duration);
            yield return null;
        }
        cg.alpha = end;
    }
}
