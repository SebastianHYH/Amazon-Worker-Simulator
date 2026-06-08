using UnityEngine;
using UnityEngine.XR; // XR input

public class RoboticsArmControllerVR : MonoBehaviour
{
    [Header("IK Target Control")]
    public Transform ikTarget;       // Assign your IK_Target GameObject here
    public float targetMoveSpeed = 3f; // Units per second (keyboard fallback)

    [Header("VR Teleoperation")]
    public bool useVRInput = true;
    public Transform rightController; // Assign the Right Controller GameObject from your XR Rig
    [Tooltip("Hold the grip button to move the arm. Release to reposition your hand freely.")]
    public bool requireClutch = true;
    [Tooltip("How much the arm moves relative to your physical hand movement.")]
    public float positionScale = 5f;

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

    // VR Input Tracking
    private InputDevice rightHand;
    private bool lastTriggerState = false;
    private bool clutchEngaged = false;
    private Vector3 controllerPosAnchor;
    private Vector3 targetPosAnchor;

    void Update()
    {
        EnsureRightHand();
        HandleIKTargetControl();
        HandleWristTwist();
        HandleClawPinch();
    }

    void EnsureRightHand()
    {
        if (!useVRInput) return;
        if (!rightHand.isValid)
        {
            rightHand = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        }
    }

    public bool IsPinching()
    {
        return isPinching;
    }

    void HandleIKTargetControl()
    {
        if (ikTarget == null) return;

        // --- VR Control ---
        if (useVRInput && rightController != null && rightHand.isValid)
        {
            bool engage = true;
            if (requireClutch)
            {
                rightHand.TryGetFeatureValue(CommonUsages.gripButton, out engage);
            }

            if (engage && !clutchEngaged)
            {
                controllerPosAnchor = rightController.position;
                targetPosAnchor = ikTarget.position;
            }
            clutchEngaged = engage;

            if (clutchEngaged)
            {
                Vector3 delta = rightController.position - controllerPosAnchor;
                ikTarget.position = targetPosAnchor + delta * positionScale;
            }
            return;
        }

        // --- Keyboard Fallback ---
        var keyboard = UnityEngine.InputSystem.Keyboard.current; // Explicitly defined to prevent namespace collision
        if (keyboard == null) return;

        Vector3 moveDirection = Vector3.zero;

        if (keyboard.upArrowKey.isPressed) moveDirection += Vector3.forward;
        if (keyboard.downArrowKey.isPressed) moveDirection += Vector3.back;
        if (keyboard.leftArrowKey.isPressed) moveDirection += Vector3.left;
        if (keyboard.rightArrowKey.isPressed) moveDirection += Vector3.right;
        if (keyboard.pageUpKey.isPressed) moveDirection += Vector3.up;
        if (keyboard.pageDownKey.isPressed) moveDirection += Vector3.down;

        ikTarget.position += moveDirection.normalized * targetMoveSpeed * Time.deltaTime;
    }

    void HandleWristTwist()
    {
        if (wristTwistBone == null) return;

        // --- VR Control ---
        if (useVRInput && rightHand.isValid)
        {
            if (rightHand.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 thumbstick))
            {
                if (Mathf.Abs(thumbstick.x) > 0.1f)
                {
                    wristTwistBone.Rotate(Vector3.up * thumbstick.x * twistSpeed * Time.deltaTime, Space.Self);
                    return;
                }
            }
        }

        // --- Keyboard Fallback ---
        var keyboard = UnityEngine.InputSystem.Keyboard.current;
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

        // --- VR Control ---
        if (useVRInput && rightHand.isValid)
        {
            if (rightHand.TryGetFeatureValue(CommonUsages.triggerButton, out bool currentTriggerState))
            {
                if (currentTriggerState && !lastTriggerState)
                {
                    isPinching = !isPinching;
                }
                lastTriggerState = currentTriggerState;
            }
        }

        // --- Keyboard Fallback ---
        var keyboard = UnityEngine.InputSystem.Keyboard.current;
        if (keyboard != null && keyboard.spaceKey.wasPressedThisFrame)
        {
            isPinching = !isPinching;
        }

        // --- Apply Rotations ---
        Quaternion targetLeft = Quaternion.Euler(isPinching ? leftClawClosedRotation : leftClawOpenRotation);
        Quaternion targetRight = Quaternion.Euler(isPinching ? rightClawClosedRotation : rightClawOpenRotation);

        leftClawBone.localRotation = Quaternion.Slerp(leftClawBone.localRotation, targetLeft, Time.deltaTime * pinchSpeed);
        rightClawBone.localRotation = Quaternion.Slerp(rightClawBone.localRotation, targetRight, Time.deltaTime * pinchSpeed);
    }
}