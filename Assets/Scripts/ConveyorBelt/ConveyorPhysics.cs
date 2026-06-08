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
        ApplyBeltForce(collision.gameObject);
    }

    // Handles trigger-collider objects (e.g. barrels with isTrigger enabled)
    void OnTriggerStay(Collider other)
    {
        ApplyBeltForce(other.gameObject);
    }

    void ApplyBeltForce(GameObject obj)
    {
        Rigidbody item = obj.GetComponent<Rigidbody>();

        if (item != null)
        {
            float beltSpeed = animatorScript.scrollSpeed * 2f;
            Vector3 worldDirection = transform.TransformDirection(pushDirection);
            Vector3 movement = worldDirection * beltSpeed * Time.fixedDeltaTime;
            item.MovePosition(item.position + movement);
        }
    }
}