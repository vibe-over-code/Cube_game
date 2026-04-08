using UnityEngine;

public class SmoothCameraFollow : MonoBehaviour
{
    public Transform target;
    public float smoothSpeed = 8f;
    public Vector2 offset = new Vector2(6f, 1.5f);
    public float cameraZ = -10f;
    public bool followX = true;
    public bool followY = false;
    public bool snapOnStart = true;
    public float orthographicSize = 5f;

    private Quaternion fixedRotation;

    void Start()
    {
        fixedRotation = Quaternion.identity;

        Camera cam = GetComponent<Camera>();
        if (cam != null)
        {
            cam.orthographic = true;
            cam.orthographicSize = orthographicSize;
        }

        transform.rotation = fixedRotation;

        if (snapOnStart && target != null)
        {
            transform.position = GetDesiredPosition();
        }
    }

    void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        Vector3 desiredPosition = GetDesiredPosition();

        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            smoothSpeed * Time.deltaTime
        );

        transform.rotation = fixedRotation;
    }

    private Vector3 GetDesiredPosition()
    {
        float targetX = followX ? target.position.x + offset.x : transform.position.x;
        float targetY = followY ? target.position.y + offset.y : offset.y;

        return new Vector3(targetX, targetY, cameraZ);
    }
}
