using UnityEngine;
// 1. Add the Input System namespace at the top
using UnityEngine.InputSystem; 

public class RoboticArmController : MonoBehaviour
{
    [Header("Wrist Twist Settings")]
    public Transform wristTwistBone; // Assign Bone.006
    public float twistSpeed = 90f;   // Degrees per second

    [Header("Claw Pinch Settings")]
    public Transform leftClawBone;   // Assign Bone.007
    public Transform rightClawBone;  // Assign Bone.008
    public float pinchSpeed = 5f;    // Interpolation speed
    
    public Vector3 leftClawOpenRotation;
    public Vector3 leftClawClosedRotation;
    public Vector3 rightClawOpenRotation;
    public Vector3 rightClawClosedRotation;

    private bool isPinching = false;

    void Update()
    {
        HandleWristTwist();
        HandleClawPinch();
    }

    void HandleWristTwist()
    {
        if (wristTwistBone == null) return;

        // 2. Read the keyboard directly using the New Input System
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        // Press 'Q' to rotate left, 'E' to rotate right
        if (keyboard.qKey.isPressed)
        {
            wristTwistBone.Rotate(Vector3.up * twistSpeed * Time.deltaTime, Space.Self);
        }
        else if (keyboard.eKey.isPressed)
        {
            wristTwistBone.Rotate(Vector3.down * twistSpeed * Time.deltaTime, Space.Self);
        }
    }

    void HandleClawPinch()
    {
        if (leftClawBone == null || rightClawBone == null) return;

        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        // 3. wasPressedThisFrame handles the toggle button tap cleanly
        if (keyboard.spaceKey.wasPressedThisFrame)
        {
            isPinching = !isPinching;
        }

        Quaternion targetLeft = Quaternion.Euler(isPinching ? leftClawClosedRotation : leftClawOpenRotation);
        Quaternion targetRight = Quaternion.Euler(isPinching ? rightClawClosedRotation : rightClawOpenRotation);

        leftClawBone.localRotation = Quaternion.Slerp(leftClawBone.localRotation, targetLeft, Time.deltaTime * pinchSpeed);
        rightClawBone.localRotation = Quaternion.Slerp(rightClawBone.localRotation, targetRight, Time.deltaTime * pinchSpeed);
    }
}