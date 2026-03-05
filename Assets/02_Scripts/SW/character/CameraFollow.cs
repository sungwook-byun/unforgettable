using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("필수 설정")]
    public Transform target;

    [Header("카메라 설정")]
    public float distance = 6f;
    public float height = 3f;
    public float smoothSpeed = 5f;
    public float rotationSmooth = 3f;
    public float yawLerpSpeed = 3f; // 좌우 회전 반응 속도 조절용

    private Vector3 currentVelocity;
    private float fixedYaw; // W,S 시야 고정 기준

    // 첫 프레임 무빙 방지용
    private bool initialized = false;

    void Start()
    {
        if (target == null) return;
        fixedYaw = target.eulerAngles.y; // 초기 시야 기준 저장
    }

    void LateUpdate()
    {
        if (target == null) return;

        // 첫 프레임에 플레이어가 순간이동될 때 카메라가 튀는 문제 방지
        if (!initialized)
        {
            transform.position = target.position + new Vector3(0, height, -distance);
            transform.LookAt(target.position + Vector3.up * 1.5f);
            initialized = true;
            return;
        }

        bool wPressed = Input.GetKey(KeyCode.W);
        bool sPressed = Input.GetKey(KeyCode.S);
        bool aPressed = Input.GetKey(KeyCode.A);
        bool dPressed = Input.GetKey(KeyCode.D);

        float targetYaw;

        // W나 S를 누를 때는 시야 고정
        if (wPressed || sPressed)
        {
            targetYaw = fixedYaw;
        }
        // A, D를 누를 때만 카메라 회전 허용
        else if (aPressed || dPressed)
        {
            fixedYaw = Mathf.LerpAngle(fixedYaw, target.eulerAngles.y, Time.deltaTime * yawLerpSpeed);
            targetYaw = fixedYaw;
        }
        else
        {
            targetYaw = fixedYaw;
        }

        Quaternion rotation = Quaternion.Euler(15f, targetYaw, 0f);
        Vector3 offset = rotation * new Vector3(0f, height, -distance);
        Vector3 desiredPosition = target.position + offset;

        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPosition,
            ref currentVelocity,
            1f / smoothSpeed
        );

        Vector3 lookPos = target.position + Vector3.up * 1.5f;
        Quaternion lookRot = Quaternion.LookRotation(lookPos - transform.position);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            lookRot,
            Time.deltaTime * rotationSmooth
        );
    }
}
