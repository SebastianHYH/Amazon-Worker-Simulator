using UnityEngine;

public class DestroyZone : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Barrel_Normal") || other.CompareTag("Barrel_EMBEGEH")) {
            Destroy(other.gameObject);
        }
    }
}
