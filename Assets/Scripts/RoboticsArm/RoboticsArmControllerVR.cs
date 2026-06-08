using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;            // XR controller polling (Meta Quest)

public class RoboticArmControllerVR : MonoBehaviour
{
    [Header("IK Target Control")]
    public Transform ikTarget;       // Assign your IK_Target GameObject here
    public float targetMoveSpeed = 3f; // Units per second (keyboard fallback only)

    [Header("VR Teleoperation (Right Controller)")]
    public bool useVRInput = true;
    public Transform controllerTransform; // The Right Controller GameObject from the XR Origin rig
    [Tooltip("Hold grip to move the arm; release to reposition your hand freely.")]
    public bool requireClutch = true;
    [Tooltip("1 = 1:1 hand motion. <1 = arm moves less than your hand (good for a large arm).")]
    public float positionScale = 10f;
    [Tooltip("Also match the controller's orientation (so the claw points/rolls like your wrist).")]
    public bool followRotation = true;
    [Tooltip("Source proxy used by wrist's multi-rotation constraint")]
    public Transform wristRotationSource;

    [Header("Reset Arm")]
    [Tooltip("Hold B button for this many seconds to reset arm to its starting pose")]
    public float resetHoldSeconds = 2f;
    [Tooltip("Radial Image on Controller that fills as reset is in progress")]
    public Image resetFillIndicator;
    [Tooltip("Optional Container show while holding B. Defaults to fill image's own object")]
    public GameObject resetIndicatorRoot;

    [Header("Wrist Twist Settings")]
    public Transform wristTwistBone; // Assign Bone.006
    public float twistSpeed = 90f;   // Degrees per second (keyboard Q/E fallback)

    [Header("Claw Pinch Settings")]
    public Transform leftClawBone;   // Assign Bone.007
    public Transform rightClawBone;  // Assign Bone.008
    public float pinchSpeed = 10f;    // Interpolation speed

    public Vector3 leftClawOpenRotation;
    public Vector3 leftClawClosedRotation;
    public Vector3 rightClawOpenRotation;
    public Vector3 rightClawClosedRotation;

    private bool isPinching = false;

    // Right-hand controller + clutch / edge-detect state
    private InputDevice rightHand;
    // private bool triggerWasPressed = false;
    private bool clutchEngaged = false;
    private Vector3 controllerPosAnchor;
    private Vector3 targetPosAnchor;
    private Quaternion controllerRotAnchor;
    private Quaternion SourceRotAnchor;

    // original position
    private Vector3 initialTargetPos;
    private Quaternion initialTargetRot;
    private Quaternion initialSourceRot;
    private bool hasInitialState = false;
    private float resetHoldTimer = 0f;
    private bool resetFired = false;

    private void Start()
    {
        // Capture arm starting pose
        if (ikTarget != null)
        {
            initialTargetPos = ikTarget.position;
            initialTargetRot = ikTarget.rotation;
        }
        if (wristRotationSource != null)
        {
            initialSourceRot = wristRotationSource.rotation;
        }
        hasInitialState = true;
    }

    void Update()
    {
        EnsureRightHand();
        HandleResetArmPosition();
        HandleIKTargetControl();
        HandleWristTwist();
        HandleClawPinch();
    }

    // (Re)acquire the right controller; it can disconnect/reconnect at runtime.
    void EnsureRightHand()
    {
        if (!useVRInput) return;
        if (!rightHand.isValid)
            rightHand = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
    }

    // Paste this inside your RoboticArmController class so other scripts can read the claw state
    public bool IsPinching()
    {
        return isPinching;
    }

    void HandleIKTargetControl()
    {
        if (ikTarget == null) return;

        // --- VR: the IK target follows the physical controller pose ---
        if (useVRInput && controllerTransform != null && rightHand.isValid)
        {
            // Clutch lets you move the arm only while engaged, then reposition your
            // real hand without dragging the arm along.
            bool engage = true;
            if (requireClutch)
                rightHand.TryGetFeatureValue(CommonUsages.gripButton, out engage);

            // On the frame we (re)engage, snap anchors so motion is relative from here.
            if (engage && !clutchEngaged)
            {
                controllerPosAnchor = controllerTransform.position;
                targetPosAnchor = ikTarget.position;
                controllerRotAnchor = controllerTransform.rotation;
                if (wristRotationSource != null)
                {
                    SourceRotAnchor = wristRotationSource.rotation;
                }
            }
            clutchEngaged = engage;

            if (clutchEngaged)
            {
                Vector3 delta = controllerTransform.position - controllerPosAnchor;
                ikTarget.position = targetPosAnchor + delta * positionScale;

                if (followRotation && wristRotationSource != null)
                {
                    Quaternion deltaRot = controllerTransform.rotation * Quaternion.Inverse(controllerRotAnchor);
                    wristRotationSource.rotation = deltaRot * SourceRotAnchor;
                }
            }
            return; // VR owns the target; skip keyboard.
        }

        // --- Keyboard fallback (no headset / editor testing) ---
        var keyboard = UnityEngine.InputSystem.Keyboard.current;
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
        // 1. If VR is active and driving the rotation, skip keyboard fallback entirely
        if (useVRInput && controllerTransform != null && rightHand.isValid && followRotation)
            return;

        // 2. Keyboard Fallback (Only runs if VR isn't actively overriding rotation)
        var keyboard = UnityEngine.InputSystem.Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.qKey.isPressed)
            wristTwistBone.Rotate(Vector3.up * twistSpeed * Time.deltaTime, Space.Self);
        else if (keyboard.eKey.isPressed)
            wristTwistBone.Rotate(Vector3.down * twistSpeed * Time.deltaTime, Space.Self);
    }

    void HandleClawPinch()
    {
        if (leftClawBone == null || rightClawBone == null) return;

        // Check if we should use VR input actively
        if (useVRInput && rightHand.isValid)
        {
            // VR Mode: Hold trigger to pinch
            if (rightHand.TryGetFeatureValue(CommonUsages.triggerButton, out bool triggerHeld))
            {
                isPinching = triggerHeld;
            }
        }
        else
        {
            // Keyboard Mode: Space bar toggles pinch on/off
            var keyboard = UnityEngine.InputSystem.Keyboard.current;
            if (keyboard != null && keyboard.spaceKey.wasPressedThisFrame)
            {
                isPinching = !isPinching;
            }
        }

        // Apply smooth visual rotations based on final isPinching state
        Quaternion targetLeft = Quaternion.Euler(isPinching ? leftClawClosedRotation : leftClawOpenRotation);
        Quaternion targetRight = Quaternion.Euler(isPinching ? rightClawClosedRotation : rightClawOpenRotation);

        leftClawBone.localRotation = Quaternion.Slerp(leftClawBone.localRotation, targetLeft, Time.deltaTime * pinchSpeed);
        rightClawBone.localRotation = Quaternion.Slerp(rightClawBone.localRotation, targetRight, Time.deltaTime * pinchSpeed);
    }

    void HandleResetArmPosition()
    {
        if (!hasInitialState) return;

        bool resetHeld = false;
        if (useVRInput && rightHand.isValid)
            rightHand.TryGetFeatureValue(CommonUsages.secondaryButton, out resetHeld); // B button

        if (resetHeld)
        {
            resetHoldTimer += Time.deltaTime;
            UpdateResetIndicator(Mathf.Clamp01(resetHoldTimer / resetHoldSeconds), true);

            if (!resetFired && resetHoldTimer >= resetHoldSeconds)
            {
                ResetArm();
                resetFired = true; // one reset per hold; release to arm it again
            }
        }
        else
        {
            resetHoldTimer = 0f;
            resetFired = false;
            UpdateResetIndicator(0f, false);
        }
    }

    void UpdateResetIndicator(float progress, bool visible)
    {
        GameObject root = resetIndicatorRoot != null ? resetIndicatorRoot
                        : (resetFillIndicator != null ? resetFillIndicator.gameObject : null);
        if (root != null && root.activeSelf != visible)
            root.SetActive(visible);

        if (resetFillIndicator != null)
            resetFillIndicator.fillAmount = progress;
    }

    void ResetArm()
    {
        if (ikTarget != null)
        {
            ikTarget.position = initialTargetPos;
            ikTarget.rotation = initialTargetRot;
        }
        if (wristRotationSource != null)
            wristRotationSource.rotation = initialSourceRot;

        // Drop the clutch so the next grip re-anchors from the reset pose
        // (otherwise the held grip would immediately drag the arm back).
        clutchEngaged = false;

        // Short haptic buzz to confirm the reset landed.
        if (rightHand.isValid)
            rightHand.SendHapticImpulse(0u, 0.6f, 0.15f);
    }
}