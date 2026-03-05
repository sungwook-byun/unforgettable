using UnityEngine;

public class PlayerInteractionRaycast : MonoBehaviour {
    [Header("상호작용 설정")]
    public float interactDistance = 2f;   // Ray 길이
    public LayerMask interactLayer;       // 감지할 레이어 (예: Interactable)

    private RaycastHit hitInfo;

    void Update() {
        // Ray를 쏘기만 함 (입력 없이 감지)
        Ray ray = new Ray(transform.position, transform.forward);

        if (Physics.Raycast(ray, out hitInfo, interactDistance, interactLayer)) {
            Debug.Log("감지된 오브젝트: " + hitInfo.collider.name);
        }
    }

    // 씬 뷰에서 Ray 시각화
    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.yellow;

        // Ray 끝부분 계산
        Vector3 endPoint = transform.position + transform.forward * interactDistance;

        // 감지 중이면 색 변경
        if (hitInfo.collider != null)
            Gizmos.color = Color.green;

        Gizmos.DrawLine(transform.position, endPoint);
        Gizmos.DrawSphere(endPoint, 0.05f);
    }
}
