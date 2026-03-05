using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Animator))]
public class PlayerController_Dream : MonoBehaviour
{
    [Header("이동 설정")]
    public float moveSpeed = 3f;
    public float rotationSpeed = 10f;

    [Header("패널티 설정 (FootprintSpawner가 제어)")]
    [SerializeField] [Range(0f, 2f)] private float wobbleAmount = 0.7f; // 비틀거림의 강도
    [SerializeField] private float wobbleFrequency = 4f; // 비틀거림의 속도

    private Rigidbody rb;
    private Camera mainCam;
    private bool isFacingBack; 
    private Animator animator;
    
    private bool isInputReversed = false; // 키 반전 상태
    private bool isWobbly = false; // 비틀거림 상태

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        
        rb.constraints = RigidbodyConstraints.FreezePositionY | 
                         RigidbodyConstraints.FreezeRotationX | 
                         RigidbodyConstraints.FreezeRotationZ;

        animator = GetComponent<Animator>();

        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.RegisterPlayer(transform);
        }
    }

    void Start()
    {
        mainCam = Camera.main;
    }

    void FixedUpdate()
    {
        if (DialogueSystem.Instance != null && DialogueSystem.Instance.IsDialogueActive)
        {
            animator.SetBool("IsMoving", false);
            //  [수정 1] velocity -> linearVelocity 
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0); 
            return; 
        }
        
        Move();
    }

    private void Move()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        if (isInputReversed)
        {
            h *= -1f;
        }

        if (isWobbly)
        {
            h += Mathf.Sin(Time.time * wobbleFrequency) * wobbleAmount;
        }
        
        Vector3 input = new Vector3(h, 0, v).normalized;
        bool isMoving = input.sqrMagnitude > 0.01f;
        animator.SetBool("IsMoving", isMoving);

        if (input.sqrMagnitude < 0.01f)
        {
            //  [수정 2] velocity -> linearVelocity 
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0); // 멈출 때 속도 0으로
            return;
        }

        Vector3 moveDir = GetMoveDir(input);
        

        rb.MovePosition(rb.position + moveDir * moveSpeed * Time.fixedDeltaTime);
        
        HandleRotation(h, v, moveDir);
    }

    private void HandleRotation(float h, float v, Vector3 moveDir) {
        Vector3 camForward = mainCam.transform.forward; camForward.y = 0f; camForward.Normalize();
        Vector3 camRight = mainCam.transform.right; camRight.y = 0f; camRight.Normalize();
        Vector3 camBack = -camForward;
        Quaternion targetRot = transform.rotation;
        if (Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.D)) { Vector3 dir = (camForward + camRight).normalized; targetRot = Quaternion.LookRotation(dir); isFacingBack = false; }
        else if (Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.A)) { Vector3 dir = (camForward - camRight).normalized; targetRot = Quaternion.LookRotation(dir); isFacingBack = false; }
        else if (Input.GetKey(KeyCode.S) && Input.GetKey(KeyCode.D)) { Vector3 dir = (camBack + camRight).normalized; targetRot = Quaternion.LookRotation(dir); isFacingBack = true; }
        else if (Input.GetKey(KeyCode.S) && Input.GetKey(KeyCode.A)) { Vector3 dir = (camBack - camRight).normalized; targetRot = Quaternion.LookRotation(dir); isFacingBack = true; }
        else if (Input.GetKey(KeyCode.W)) { targetRot = Quaternion.LookRotation(camForward); isFacingBack = false; }
        else if (Input.GetKey(KeyCode.S)) { targetRot = Quaternion.LookRotation(camBack); isFacingBack = true; }
        else if (Input.GetKey(KeyCode.A)) { Vector3 leftDir = -camRight; targetRot = Quaternion.LookRotation(leftDir); isFacingBack = false; }
        else if (Input.GetKey(KeyCode.D)) { Vector3 rightDir = camRight; targetRot = Quaternion.LookRotation(rightDir); isFacingBack = false; }
        else if (h != 0 || v != 0) { targetRot = Quaternion.LookRotation(moveDir); }
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotationSpeed);
    }
    private Vector3 GetMoveDir(Vector3 input) {
        if (mainCam == null) mainCam = Camera.main;
        Vector3 camForward = mainCam.transform.forward; Vector3 camRight = mainCam.transform.right;
        camForward.y = 0; camRight.y = 0; camForward.Normalize(); camRight.Normalize();
        return (camForward * input.z + camRight * input.x).normalized;
    }
    public bool IsFacingBack() { return isFacingBack; }
    
    public void SetInputReversal(bool reversed)
    {
        isInputReversed = reversed;
    }

    public void SetWobble(bool wobble)
    {
        isWobbly = wobble;
    }
}