using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("The target the camera will follow (the Player).")]
    public Transform target;

    [Header("Camera Settings")]
    [Tooltip("How smoothly the camera follows the target. Lower values are 'floatier', higher values are 'tighter'.")]
    public float smoothSpeed = 0.125f;
    [Tooltip("The offset from the target's position. This lets you position the camera perfectly.")]
    public Vector3 offset = new Vector3(0, 2, -10);

    // This function is called after all Update functions have been called.
    // This is the best place to put camera logic, as it ensures the target has finished moving for the frame.
    void LateUpdate()
    {
        // First, check if we have a valid target to follow.
        if (target == null)
        {
            Debug.LogWarning("Camera Controller is missing a target!");
            return; // Exit the function if there's no target.
        }

        // Calculate the desired position for the camera.
        // This is the target's current position plus our desired offset.
        Vector3 desiredPosition = target.position + offset;

        // Use Vector3.Lerp (Linear Interpolation) to smoothly move from the camera's current position
        // to the desired position. The smoothSpeed variable controls how quickly this happens.
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        // Apply the new, smoothed position to the camera's transform.
        transform.position = smoothedPosition;
    }
}