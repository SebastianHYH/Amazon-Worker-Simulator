using UnityEngine;
using UnityEngine.InputSystem;

public class FlyCamera : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 10f;
    public float shiftMultiplier = 2.5f;

    [Header("Look Settings")]
    public float lookSensitivity = 0.2f;

    private Vector3 rotation;

    void Start()
    {
        // Initialize rotation with current camera angles
        rotation = transform.localRotation.eulerAngles;
    }

    void Update()
    {
        var keyboard = Keyboard.current;
        var mouse = Mouse.current;
        if (keyboard == null || mouse == null) return;

        // --- 1. LOOK AROUND (Hold Right Click) ---
        if (mouse.rightButton.isPressed)
        {
            // Hide cursor while looking around
            Cursor.lockState = CursorLockMode.Locked;

            Vector2 mouseDelta = mouse.delta.ReadValue() * lookSensitivity;
            
            rotation.y += mouseDelta.x;
            rotation.x -= mouseDelta.y;
            rotation.x = Mathf.Clamp(rotation.x, -90f, 90f); // Prevent flipping upside down

            transform.localRotation = Quaternion.Euler(rotation.x, rotation.y, 0f);
        }
        else
        {
            // Unlock cursor when right click is released
            Cursor.lockState = CursorLockMode.None;
        }

        // --- 2. MOVE AROUND ---
        Vector3 moveInput = Vector3.zero;

        if (keyboard.wKey.isPressed) moveInput += transform.forward;
        if (keyboard.sKey.isPressed) moveInput -= transform.forward;
        if (keyboard.aKey.isPressed) moveInput -= transform.right;
        if (keyboard.dKey.isPressed) moveInput += transform.right;
        
        // Vertical movement (E to go up, Q to go down)
        // Adjusting slightly so it doesn't conflict with your claw rotation keys if you aren't right-clicking
        if (keyboard.spaceKey.isPressed && !mouse.rightButton.isPressed) { /* Let claw handle space */ }
        else if (keyboard.spaceKey.isPressed) moveInput += Vector3.up; 

        // Apply Shift boost
        float currentSpeed = moveSpeed;
        if (keyboard.leftShiftKey.isPressed)
        {
            currentSpeed *= shiftMultiplier;
        }

        transform.position += moveInput.normalized * currentSpeed * Time.deltaTime;
    }
}