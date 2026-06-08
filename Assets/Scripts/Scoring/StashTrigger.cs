using UnityEngine;

public class StashTrigger : MonoBehaviour
{
    [Tooltip("The barrel tag that scores +1 here. Any other barrel tag scores -1.")]
    public string correctBarrelTag = "Barrel_Normal";

    void OnTriggerEnter(Collider other)
    {
        string tag = other.tag;
        if (tag != "Barrel_Normal" && tag != "Barrel_EMBEGEH") return;

        if (ScoreManager.Instance != null)
        {
            if (tag == correctBarrelTag)
                ScoreManager.Instance.AddPoint();
            else
                ScoreManager.Instance.RemovePoint();
        }

        // Destroy the whole barrel (root object), not just the child trigger node
        Destroy(other.transform.root.gameObject);
    }
}
