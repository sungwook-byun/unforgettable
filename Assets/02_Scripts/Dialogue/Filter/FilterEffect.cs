using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class FilterEffect : MonoBehaviour {
    [Header("Post-Processing Volume 연결")]
    [SerializeField] private Volume volume;

    [Header("FilterUI 연결")]
    [SerializeField] private FilterUI filterUI; // FilterUI 연결

    private ColorAdjustments colorAdjustments;
    private Vignette vignette;

    private bool increasing = true;
    private Coroutine transitionCoroutine;

    private void Awake() {
        if (volume == null)
            volume = FindFirstObjectByType<Volume>();

        if (volume == null) {
            Debug.LogWarning("⚠️ Volume이 씬에 없습니다.");
            return;
        }

        volume.profile.TryGet(out colorAdjustments);
        volume.profile.TryGet(out vignette);

        if (Camera.main != null) {
            var camData = Camera.main.GetComponent<UniversalAdditionalCameraData>();
            if (camData != null) {
                camData.renderPostProcessing = true;
            }
        }

        // 초기 중립값
        ResetColorAdjustments();
        if (vignette != null) vignette.intensity.value = 0f;
        if (vignette != null) vignette.smoothness.value = 0.5f;
    }

    private void Update() {
        if (vignette != null && vignette.intensity.value > 0f) {
            AnimateVignette();
        }
    }

    public void ApplyFilter(FilterUI.FilterType type) {
        if (transitionCoroutine != null) StopCoroutine(transitionCoroutine);
        transitionCoroutine = StartCoroutine(FilterTransition(type));
    }

    private IEnumerator FilterTransition(FilterUI.FilterType type) {
        float duration = 0.5f; // 전환 시간
        float elapsed = 0f;

        float startSaturation = colorAdjustments != null ? colorAdjustments.saturation.value : 0f;
        float startVignette = vignette != null ? vignette.intensity.value : 0f;

        float targetSaturation = (type == FilterUI.FilterType.Grayscale) ? -100f : 0f;
        float targetVignette = (type == FilterUI.FilterType.Vignette) ? filterUI.vignetteMin : 0f;

        while (elapsed < duration) {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            if (colorAdjustments != null)
                colorAdjustments.saturation.value = Mathf.Lerp(startSaturation, targetSaturation, t);

            if (vignette != null)
                vignette.intensity.value = Mathf.Lerp(startVignette, targetVignette, t);

            yield return null;
        }

        if (colorAdjustments != null) colorAdjustments.saturation.value = targetSaturation;
        if (vignette != null) vignette.intensity.value = targetVignette;
    }

    private void AnimateVignette() {
        if (vignette == null) return;

        float min = filterUI.vignetteMin;
        float max = filterUI.vignetteMax;
        float speed = filterUI.vignetteSpeed;

        if (increasing) {
            vignette.intensity.value += speed * Time.deltaTime;
            if (vignette.intensity.value >= max) increasing = false;
        } else {
            vignette.intensity.value -= speed * Time.deltaTime;
            if (vignette.intensity.value <= min) increasing = true;
        }
    }

    private void ResetColorAdjustments() {
        if (colorAdjustments != null) {
            colorAdjustments.saturation.value = 0f;
            colorAdjustments.contrast.value = 0f;
            colorAdjustments.colorFilter.value = Color.white;
            colorAdjustments.postExposure.value = 0f;
        }
    }
}
