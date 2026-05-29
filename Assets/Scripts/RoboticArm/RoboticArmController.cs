using UnityEngine;

public class RoboticArmController : MonoBehaviour
{
    [Header("IK Rig Targets")]
    [SerializeField] private Transform ikTarget; // Hand Target
    [SerializeField] private Transform vrControllerSource; // The VR controller transform that will drive the IK target (Test with Mock_RightHand)
    
    [Header("Physical Robot References")]
    [SerializeField] private Transform physicalClawBase;

    [Header("Movement Behavior")]
    [SerializeField] private float followSpeed = 15f;      
    [SerializeField] private float maxReachRadius = 3.5f;  

    private Vector3 mockStartPos;
    private Vector3 ikStartPos;
    private Quaternion mockStartRot;
    private Quaternion ikStartRot;

    private void Awake()
    {
        if (ikTarget != null && physicalClawBase != null)
        {
            ikTarget.position = physicalClawBase.position;
            ikTarget.rotation = physicalClawBase.rotation;
        }
    }

    private void Start()
    {
        if (vrControllerSource != null && ikTarget != null)
        {
            mockStartPos = vrControllerSource.position;
            ikStartPos = ikTarget.position;
            
            mockStartRot = vrControllerSource.rotation;
            ikStartRot = ikTarget.rotation;
        }
    }

    private void Update()
    {
        if (vrControllerSource != null && ikTarget != null)
        {
            TrackMockInput();
        }
    }

    private void TrackMockInput()
    {
        Vector3 positionDelta = vrControllerSource.position - mockStartPos;
        Vector3 targetPosition = ikStartPos + positionDelta;

        Quaternion rotationDelta = vrControllerSource.rotation * Quaternion.Inverse(mockStartRot);
        Quaternion targetRotation = rotationDelta * ikStartRot;

        float distanceFromBase = Vector3.Distance(transform.position, targetPosition);
        if (distanceFromBase > maxReachRadius)
        {
            Vector3 directionToTarget = (targetPosition - transform.position).normalized;
            targetPosition = transform.position + (directionToTarget * maxReachRadius);
        }

        ikTarget.position = Vector3.Lerp(ikTarget.position, targetPosition, Time.deltaTime * followSpeed);
        ikTarget.rotation = Quaternion.Slerp(ikTarget.rotation, targetRotation, Time.deltaTime * followSpeed);
    }
}