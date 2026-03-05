using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering.VirtualTexturing;

public class MouseInteractionRaycastDreamWorld4 : MonoBehaviour {
    [Header("상호작용 설정")]
    public float rayDistance = 100f;
    public LayerMask interactLayer;
    public Color outlineColor = Color.magenta;
    public float outlineWidth = 7f;

    private Transform highlight; // 현재 마우스 오버 중인 오브젝트
    private RaycastHit hitInfo;
    
    [SerializeField] private Transform grandmaChild;
    [SerializeField] private Transform wormChild;
    [SerializeField] private Transform dyePotChild;
    [SerializeField] private Transform spinningWheelChild;

    private DialogueSystem system;

    private void Start() {
        system = FindFirstObjectByType<DialogueSystem>();
    }

    void Update() {
        HandleHighlight();
        HandleClick();
    }

    private void HandleHighlight() {
        // 기존 하이라이트 비활성화
        if (highlight != null) {
            Outline oldOutline = highlight.GetComponent<Outline>();
            if (oldOutline != null)
                oldOutline.enabled = false;
            highlight = null;
        }

        if (EventSystem.current.IsPointerOverGameObject()) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit[] hits = Physics.RaycastAll(ray, rayDistance, interactLayer);

        if (hits.Length > 0) {
            // ✅ 가까운 순으로 정렬
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

            // ✅ 제일 가까운 충돌체 선택
            hitInfo = hits[0];
            Transform target = hitInfo.transform;

            // 태그 검사
            if (target.CompareTag("Selectable") || target.CompareTag("Grandma")) {
                Transform outlineTarget = target;
                if (target.name.Contains("Worm"))
                    outlineTarget = wormChild;
                if (target.name.Contains("vase"))
                    outlineTarget = dyePotChild;
                if (target.name.Contains("Spinning"))
                    outlineTarget = spinningWheelChild;
                if (target.CompareTag("Grandma") && grandmaChild != null)
                    outlineTarget = grandmaChild;

                // Outline 컴포넌트 가져오기 또는 추가
                Outline outline = outlineTarget.GetComponent<Outline>();
                if (outline == null) {
                    MeshFilter meshFilter = outlineTarget.GetComponent<MeshFilter>();
                    if (meshFilter && meshFilter.sharedMesh && !meshFilter.sharedMesh.isReadable)
                        return;

                    outline = outlineTarget.gameObject.AddComponent<Outline>();
                }

                outline.OutlineColor = outlineColor;
                outline.OutlineWidth = outlineWidth;
                outline.OutlineMode = Outline.Mode.OutlineVisible;
                outline.enabled = true;

                highlight = outlineTarget;
            }
        }
    }

    private void HandleClick() {
        if (system.IsDialogueActive) return;
        if (Input.GetMouseButtonDown(0)) {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit[] hits = Physics.RaycastAll(ray, rayDistance, interactLayer);

            if (hits.Length > 0) {
                System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
                Transform target = hits[0].transform;
                Debug.Log($"클릭한 오브젝트: {target.name}");

                DialogueEvent dialogue = target.GetComponent<DialogueEvent>();
                if (dialogue != null) {
                    dialogue.OnInteract();
                }
                else if (target.name.Contains("Wardrobe"))
                {
                    Day4Controller day4Controller = FindFirstObjectByType<Day4Controller>();
                    day4Controller.SetHouseOff();
                }
                else if (target.name.Contains("Spinning"))
                {
                    Day4Controller day4Controller = FindFirstObjectByType<Day4Controller>();
                    day4Controller.SetLaughingOn();
                }
            }
        }
    }

    private void OnDrawGizmos() {
        if (Camera.main == null) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Vector3 endPoint = ray.origin + ray.direction * rayDistance;

        Gizmos.color = Color.yellow;
        if (Physics.Raycast(ray, rayDistance, interactLayer))
            Gizmos.color = Color.green;

        Gizmos.DrawLine(ray.origin, endPoint);
        Gizmos.DrawSphere(endPoint, 0.05f);
    }
}
