using UnityEngine;

public class ConveyorPhysics : MonoBehaviour
{
    
    private ConveyorScroller animatorScript; // to hold reference of Scroller

    [Tooltip("Direction in which the conveyor belt pushes objects. Currently set to -Z.")]
    private Vector3 pushDirection = Vector3.back;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animatorScript = GetComponent<ConveyorScroller>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnCollisionStay(Collision collision)
    {
        // Check if the object touching the belt has a Rigidbody (physics component)
        Rigidbody item = collision.gameObject.GetComponent<Rigidbody>();

        if (item != null)
        {
            // get beltSpeed based on scrollSpeed
            float beltSpeed = animatorScript.scrollSpeed * 2f;

            // Convert our local push direction to world space so it works even if the conveyor is rotated
            Vector3 worldDirection = transform.TransformDirection(pushDirection);

            // Calculate the movement for this frame
            Vector3 movement = worldDirection * beltSpeed * Time.fixedDeltaTime;

            // Safely move the Rigidbody to its new position
            item.MovePosition(item.position + movement);
        }
    }
}
