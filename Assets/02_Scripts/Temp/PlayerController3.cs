using DG.Tweening.Core.Easing;
using TMPro.Examples;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(CharacterController))]
public class PlayerController3 : MonoBehaviour {
    [Header("이동 설정")]
    [SerializeField] private float moveSpeed = 2f;     // 이동 속도
    [SerializeField] private Transform headTransform;  // 카메라 기준 위치

    private Animator _animator;
    private PlayerInput _playerInput;
    private CharacterController _characterController;

    private Vector2 _moveInput;
    private float _velocityY;
    private float _currentSpeed;

    private float Gravity = -9.81f;

    private static readonly int Speed = Animator.StringToHash("Speed");

    private void Awake() {
        _animator = GetComponent<Animator>();
        _playerInput = GetComponent<PlayerInput>();
        _characterController = GetComponent<CharacterController>();
    }

    private void OnEnable() {
        // 카메라 세팅
        _playerInput.camera = Camera.main;
        if (_playerInput.camera != null) {
            _playerInput.camera.GetComponent<CameraController>()?.SetTarget(headTransform, _playerInput);
        }
    }

    private void Update() {
        HandleMovement();
    }

    private void HandleMovement() {
        // 이동 입력
        _moveInput = _playerInput.actions["Move"].ReadValue<Vector2>();

        // 카메라 기준 이동 방향 계산
        Vector3 forward = _playerInput.camera.transform.forward;
        Vector3 right = _playerInput.camera.transform.right;
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        Vector3 moveDirection = forward * _moveInput.y + right * _moveInput.x;

        // 이동 속도 계산
        float targetSpeed = moveDirection.magnitude * moveSpeed;
        _currentSpeed = Mathf.Lerp(_currentSpeed, targetSpeed, Time.deltaTime * 10f); // 부드럽게 변화

        // Animator Speed 업데이트
        _animator.SetFloat(Speed, _currentSpeed);

        // 중력 처리
        if (_characterController.isGrounded) {
            _velocityY = -0.1f;
        } else {
            _velocityY += Gravity * Time.deltaTime;
        }

        // 실제 이동
        Vector3 move = moveDirection.normalized * _currentSpeed * Time.deltaTime;
        move.y = _velocityY * Time.deltaTime;

        _characterController.Move(move);
    }
}
