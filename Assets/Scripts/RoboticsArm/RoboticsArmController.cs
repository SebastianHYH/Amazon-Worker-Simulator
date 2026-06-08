using UnityEngine;
using UnityEngine.InputSystem; 

public class RoboticArmController : MonoBehaviour
{
    [Header("IK Target Control")]
    public Transform ikTarget;       // Assign your IK_Target GameObject here
    public float targetMoveSpeed = 3f; // Units per second the target moves

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
        HandleIKTargetControl();
        HandleWristTwist();
        HandleClawPinch();
    }

    void HandleIKTargetControl()
    {
        if (ikTarget == null) return;

        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        Vector3 moveDirection = Vector3.zero;

        // Move horizontally along World Space X and Z axes using Arrow Keys
        if (keyboard.upArrowKey.isPressed)    moveDirection += Vector3.forward;
        if (keyboard.downArrowKey.isPressed)  moveDirection += Vector3.back;
        if (keyboard.leftArrowKey.isPressed)  moveDirection += Vector3.left;
        if (keyboard.rightArrowKey.isPressed) moveDirection += Vector3.right;

        // Move vertically along World Space Y axis using PageUp and PageDown
        if (keyboard.pageUpKey.isPressed)     moveDirection += Vector3.up;
        if (keyboard.pageDownKey.isPressed)   moveDirection += Vector3.down;

        // Apply movement to the target
        ikTarget.position += moveDirection.normalized * targetMoveSpeed * Time.deltaTime;
    }

    // Paste this inside your RoboticArmController class so other scripts can read the claw state
    public bool IsPinching()
    {
        return isPinching;
    }

    void HandleWristTwist()
    {
        if (wristTwistBone == null) return;

        var keyboard = Keyboard.current;
        if (keyboard == null) return;

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