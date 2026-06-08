using UnityEngine;

public class StashTrigger : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Grabbable")) return;

        if (ScoreManager.Instance != null)
            ScoreManager.Instance.AddPoint();

        Destroy(other.gameObject);
    }
}
