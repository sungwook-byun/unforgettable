using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

[RequireComponent(typeof(UnityEngine.UI.Button))]
public class HoverButton : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler {

    private TextMeshProUGUI label;
    private ButtonGroupActions groupManager;
    private bool isSelected = false; // Arrow, QE, 클릭 공용 플래그

    [Header("Hover Effect")]
    [SerializeField] private float hoverScale = 1.05f; // 글씨 크기 배율
    private Vector3 originalScale;

    private void Awake() {
        groupManager = GetComponentInParent<ButtonGroupActions>();
        label = GetComponentInChildren<TextMeshProUGUI>();
        if (label != null)
            originalScale = label.transform.localScale;
    }

    //private void OnEnable() {
    //    if (groupManager != null)
    //        groupManager.Register(this);
    //}

    //private void OnDisable() {
    //    if (groupManager != null)
    //        groupManager.Unregister(this);
    //}

    // 색상 + 크기 적용
    public void SetColor(Color c, bool isHover = false) {
        if (label != null) {
            label.color = c;
            label.transform.localScale = isHover ? originalScale * hoverScale : originalScale;
        }
    }

    public void SetColor(Color color, bool highlight, bool select = false) {
        if (label != null) label.color = color;
        transform.localScale = highlight ? Vector3.one * 1.05f : Vector3.one;

        if (select) isSelected = true;
    }

    public bool IsSelected() => isSelected;
    public void ResetSelection() => isSelected = false;

    // 마우스가 들어오면 현재 선택 버튼으로 활성화
    public void OnPointerEnter(PointerEventData eventData) {
        if (groupManager != null) {
            if (groupManager.buttonsGroup != null && groupManager.buttonsGroup.alpha < 1f) return; // Fade 진행 중 무시
            groupManager.OnButtonHovered(this); // 이동 효과음 호출
            groupManager.Activate(this);
        }
    }

    // 마우스 클릭 시
    public void OnPointerClick(PointerEventData eventData) {
        if (groupManager != null) {
            if (groupManager.buttonsGroup != null && groupManager.buttonsGroup.alpha < 1f) return; // Fade 진행 중 무시
            groupManager.OnUIButtonClicked(); // 클릭 효과음 호출
            groupManager.Activate(this, true); // 클릭 강조 + 즉시 실행
        }
    }
}
