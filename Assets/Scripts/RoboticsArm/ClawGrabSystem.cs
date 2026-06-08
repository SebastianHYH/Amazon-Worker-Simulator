using UnityEngine;

public class ClawGrabSystem : MonoBehaviour
{
    [Header("Grab Settings")]
    public string grabbableTag = "Grabbable";
    public Transform attachPoint; // Usually Bone.006 or Claw_Grab_Zone itself

    private GameObject objectInZone = null;
    private GameObject grabbedObject = null;
    private bool wasPinchingLastFrame = false;

    // We'll automatically fetch your main controller script to watch the pinch state
    private RoboticArmControllerVR armController;

    void Start()
    {
        // Find the main arm controller script on the root object
        armController = GetComponentInParent<RoboticArmControllerVR>();
        
        // If you don't explicitly assign an attach point, it defaults to this trigger's position
        if (attachPoint == null)
        {
            attachPoint = this.transform;
        }
    }

    void Update()
    {
        if (armController == null) return;

        // Get the current pinch state from our main controller
        bool isPinchingNow = armController.IsPinching();

        // Detect the exact frame the user toggled the pinch to "Closed"
        if (isPinchingNow && !wasPinchingLastFrame)
        {
            TryGrab();
        }
        // Detect the exact frame the user toggled the pinch to "Open"
        else if (!isPinchingNow && wasPinchingLastFrame)
        {
            TryRelease();
        }

        wasPinchingLastFrame = isPinchingNow;
    }

    private void TryGrab()
    {
        // If there's an item hovering inside our box collider and we aren't holding anything yet
        if (objectInZone != null && grabbedObject == null)
        {
            grabbedObject = objectInZone;

            // Handle physics properties so it doesn't fight the robotic movement
            Rigidbody rb = grabbedObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true; // Turn off physics gravity/forces while held
            }

            // Snap the item smoothly to our collection zone and parent it
            grabbedObject.transform.SetParent(attachPoint);
            grabbedObject.transform.localPosition = Vector3.zero; 
            
            // Optional: If you want to match the twist/rotation of the wrist, uncomment below:
            grabbedObject.transform.localRotation = Quaternion.identity;
        }
    }

    private void TryRelease()
    {
        if (grabbedObject != null)
        {
            // Restore physics back to the object so it falls naturally
            Rigidbody rb = grabbedObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
            }

            // Unparent the object completely so it stays in the world space
            grabbedObject.transform.SetParent(null);
            grabbedObject = null;
        }
    }

    // --- Trigger Detection ---
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(grabbableTag))
        {
            objectInZone = other.gameObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(grabbableTag) && objectInZone == other.gameObject)
        {
            objectInZone = null;
        }
    }
}