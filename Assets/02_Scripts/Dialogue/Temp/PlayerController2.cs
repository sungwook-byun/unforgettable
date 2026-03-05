using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController2 : MonoBehaviour {
    [Header("이동 설정")]
    public float moveSpeed = 3f;
    [SerializeField] private float rotationSpeed = 10f;

    private Rigidbody rb;
    private Camera mainCam;
    private Animator animator;

    private bool canMove = true;
    private Vector3 lastMoveDir;

    void Awake() {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        animator = GetComponentInChildren<Animator>();
    }

    void Start() {
        mainCam = Camera.main;
    }

    void FixedUpdate() {
        if (canMove) Move();
    }

    private void Move() {
        float h = Input.GetAxisRaw("Horizontal"); // A/D (-1~1)
        float v = Input.GetAxisRaw("Vertical");   // W/S (-1~1)
        Vector3 input = new Vector3(h, 0, v).normalized;

        if (input.sqrMagnitude < 0.01f) {
            rb.linearVelocity = Vector3.zero;
            animator?.SetFloat("Speed", 0f);
            return;
        }

        float speedValue = Mathf.Clamp(new Vector2(h, v).magnitude, 0f, 1f);
        speedValue *= Mathf.Sign(v != 0 ? v : h);
        animator?.SetFloat("Speed", speedValue);

        Vector3 moveDir = GetMoveDir(input);
        rb.linearVelocity = moveDir * moveSpeed;

        Quaternion targetRotation = Quaternion.LookRotation(moveDir);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);

        lastMoveDir = moveDir;
    }

    private Vector3 GetMoveDir(Vector3 input) {
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

    public void SetCanMove(bool value) {
        canMove = value;
        if (!value)
            animator?.SetFloat("Speed", 0f);
    }
}
