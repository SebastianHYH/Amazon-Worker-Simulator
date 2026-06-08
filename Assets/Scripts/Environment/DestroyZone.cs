using UnityEngine;

public class DestroyZone : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Grabbable")) {
            Destroy(other.gameObject);
        }
    }
}
