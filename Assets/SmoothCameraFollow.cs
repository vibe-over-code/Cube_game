using UnityEngine;

public class SmoothCameraFollow : MonoBehaviour
{
    public Transform target; // the player's transform
    public float smoothSpeed = 0.3f; // the speed of the camera's movement
    public Vector3 offset = new Vector3(0, 5, -10); // the camera's offset from the player
    public float rotationSpeed = 5f; // the speed of the camera's rotation
    public float rotationDamping = 0.5f; // the damping of the camera's rotation

    private Transform cameraTransform; // the camera's transform
    private Quaternion targetRotation; // the target rotation of the camera

    void Start()
    {
        cameraTransform = transform;
        targetRotation = cameraTransform.rotation;
    }

    void LateUpdate()
    {
        Vector3 targetPosition = target.position + offset;
        cameraTransform.position = Vector3.Lerp(cameraTransform.position, targetPosition, smoothSpeed * Time.deltaTime);

        // Rotate the camera to follow the player's rotation
        float targetAngle = target.eulerAngles.y;
        float cameraAngle = cameraTransform.eulerAngles.y;
        float angleDiff = Mathf.DeltaAngle(cameraAngle, targetAngle);
        cameraTransform.eulerAngles = new Vector3(0, cameraAngle + angleDiff * rotationSpeed * Time.deltaTime, 0);

        // Damp the camera's rotation to return to its original orientation
        Quaternion currentRotation = cameraTransform.rotation;
        cameraTransform.rotation = Quaternion.Lerp(currentRotation, targetRotation, rotationDamping * Time.deltaTime);

        cameraTransform.LookAt(target);
    }
}