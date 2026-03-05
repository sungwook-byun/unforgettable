using UnityEngine;
using UnityEngine.InputSystem;

public class InputReader : MonoBehaviour
{
    public Vector2 Move { get; private set; }
    public Vector2 Move_WSAD { get; private set; }
    public Vector2 Move_Arrow { get; private set; }
    public bool JumpDown { get; private set; }
    public bool ActionDown { get; private set; }
    public Vector2 Pointer { get; private set; }
    public bool PointerDown { get; private set; }
    public bool PointerUp { get; private set; }
    public bool PointerHold { get; private set; }

    public Vector3 PointerWorldPos
    {
        get
        {
            Vector2 screenPos = Mouse.current.position.ReadValue();
            Vector3 world = Camera.main.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 0f));
            world.z = 0f; // 2D면 z고정
            return world;
        }
    }

    void Update()
    {
        var kb = Keyboard.current;

        // --- WASD / Arrow를 분리해서 계산 ---
        Vector2 wasd = Vector2.zero;
        Vector2 arrows = Vector2.zero;

        if (kb != null)
        {
            // WASD 전용
            if (kb.wKey.isPressed) wasd.y += 1;
            if (kb.sKey.isPressed) wasd.y -= 1;
            if (kb.aKey.isPressed) wasd.x -= 1;
            if (kb.dKey.isPressed) wasd.x += 1;

            // 화살표 전용
            if (kb.upArrowKey.isPressed) arrows.y += 1;
            if (kb.downArrowKey.isPressed) arrows.y -= 1;
            if (kb.leftArrowKey.isPressed) arrows.x -= 1;
            if (kb.rightArrowKey.isPressed) arrows.x += 1;
        }

        // 각자 클램프해 보관
        Move_WSAD = Vector2.ClampMagnitude(wasd, 1f);
        Move_Arrow = Vector2.ClampMagnitude(arrows, 1f);

        // 기존 Move는 "둘의 합"을 클램프(지금과 동일한 효과)
        Vector2 move = Move_WSAD + Move_Arrow;
        Move = Vector2.ClampMagnitude(move, 1f);

        //// --- Keyboard 방향 입력 ---
        //Vector2 move = Vector2.zero;

        //var kb = Keyboard.current;

        //if (kb != null)
        //{
        //    if (kb.wKey.isPressed || kb.upArrowKey.isPressed) move.y += 1;
        //    if (kb.sKey.isPressed || kb.downArrowKey.isPressed) move.y -= 1;
        //    if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) move.x -= 1;
        //    if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) move.x += 1;
        //}

        //Move = Vector2.ClampMagnitude(move, 1f);

        // --- Jump / Action 키 ---
        JumpDown = (kb != null && kb.spaceKey.wasPressedThisFrame);

        ActionDown = (kb != null && kb.enterKey.wasPressedThisFrame); 

        // --- Pointer / Click (Mouse or Touch) ---
        var mouse = Mouse.current;
        var touch = Touchscreen.current;

        if (mouse != null)
        {
            Pointer = mouse.position.ReadValue();
            PointerDown = mouse.leftButton.wasPressedThisFrame;
            PointerUp = mouse.leftButton.wasReleasedThisFrame;
            PointerHold = mouse.leftButton.isPressed;
        }
        else if (touch != null)
        {
            if (touch.primaryTouch.press.isPressed)
            {
                Pointer = touch.primaryTouch.position.ReadValue();
                PointerDown = touch.primaryTouch.press.wasPressedThisFrame;
            }
            else
            {
                PointerDown = false;
            }
        }
    }
}
