using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("이동 설정")]
    public float moveSpeed = 3f;
    public float rotationSpeed = 10f;

    private Rigidbody rb;
    private Camera mainCam;
    private bool isFacingBack; // 뒤돌아있는 상태 여부

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        // 추가 - 게임 시작 시 저장된 위치를 즉시 복원
        var saveMgr = SaveManager.Instance;
        if (saveMgr != null)
        {
            var data = saveMgr.Load();
            if (data != null && data.scenePositions != null)
            {
                string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
                var pos = data.scenePositions.Find(s => s.sceneName == currentScene);
                if (pos != null)
                {
                    transform.position = pos.position; // 위치 즉시 복원
                }
            }
        }
    }

    void Start()
    {
        mainCam = Camera.main;
        SaveManager.Instance.RegisterPlayer(transform);
    }

    void FixedUpdate()
    {
        Move();
    }

    private void Move()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 input = new Vector3(h, 0, v).normalized;
        if (input.sqrMagnitude < 0.01f)
            return;

        Vector3 moveDir = GetMoveDir(input);
        rb.MovePosition(rb.position + moveDir * moveSpeed * Time.fixedDeltaTime);
        HandleRotation(h, v, moveDir);
    }

    private void HandleRotation(float h, float v, Vector3 moveDir)
    {
        Vector3 camForward = mainCam.transform.forward;
        camForward.y = 0f;
        camForward.Normalize();

        Vector3 camRight = mainCam.transform.right;
        camRight.y = 0f;
        camRight.Normalize();

        Vector3 camBack = -camForward;

        Quaternion targetRot = transform.rotation;

        if (Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.D))
        {
            Vector3 dir = (camForward + camRight).normalized;
            targetRot = Quaternion.LookRotation(dir);
            isFacingBack = false;
        }
        else if (Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.A))
        {
            Vector3 dir = (camForward - camRight).normalized;
            targetRot = Quaternion.LookRotation(dir);
            isFacingBack = false;
        }
        else if (Input.GetKey(KeyCode.S) && Input.GetKey(KeyCode.D))
        {
            Vector3 dir = (camBack + camRight).normalized;
            targetRot = Quaternion.LookRotation(dir);
            isFacingBack = true;
        }
        else if (Input.GetKey(KeyCode.S) && Input.GetKey(KeyCode.A))
        {
            Vector3 dir = (camBack - camRight).normalized;
            targetRot = Quaternion.LookRotation(dir);
            isFacingBack = true;
        }
        else if (Input.GetKey(KeyCode.W))
        {
            targetRot = Quaternion.LookRotation(camForward);
            isFacingBack = false;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            targetRot = Quaternion.LookRotation(camBack);
            isFacingBack = true;
        }
        else if (Input.GetKey(KeyCode.A))
        {
            Vector3 leftDir = -camRight;
            targetRot = Quaternion.LookRotation(leftDir);
            isFacingBack = false;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            Vector3 rightDir = camRight;
            targetRot = Quaternion.LookRotation(rightDir);
            isFacingBack = false;
        }
        else if (h != 0 || v != 0)
        {
            targetRot = Quaternion.LookRotation(moveDir);
        }

        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotationSpeed);
    }

    private Vector3 GetMoveDir(Vector3 input)
    {
        if (mainCam == null)
            mainCam = Camera.main;

        Vector3 camForward = mainCam.transform.forward;
        Vector3 camRight = mainCam.transform.right;

        camForward.y = 0;
        camRight.y = 0;

        camForward.Normalize();
        camRight.Normalize();

        return (camForward * input.z + camRight * input.x).normalized;
    }

    public bool IsFacingBack()
    {
        return isFacingBack;
    }
}
