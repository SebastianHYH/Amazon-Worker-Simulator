using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class TreeMaterialOverride : MonoBehaviour
{
    public Material trunkMaterial;
    public Material leavesMaterial;

    void Awake()
    {
        GetComponent<MeshRenderer>().materials = new Material[] { trunkMaterial, leavesMaterial };
    }
}
