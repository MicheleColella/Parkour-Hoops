using UnityEngine;
using UnityEngine.Events;

public class LeverPositionTrigger : MonoBehaviour
{
    [Header("Lever Position Ranges")]
    [Tooltip("Minimum and maximum Z position range for triggering the first event.")]
    public float rangeMinZone1 = -5f;
    public float rangeMaxZone1 = -2f;

    [Tooltip("Minimum and maximum Z position range for triggering the second event.")]
    public float rangeMinZone2 = 2f;
    public float rangeMaxZone2 = 5f;

    [Header("Events")]
    [Tooltip("Event triggered when the lever enters the first position range.")]
    public UnityEvent onZone1Entered;

    [Tooltip("Event triggered when the lever enters the second position range.")]
    public UnityEvent onZone2Entered;

    [Header("Lever Child Transform")]
    [Tooltip("The child transform that represents the lever handle.")]
    public Transform leverHandle;

    [Header("Gizmos Settings")]
    [Tooltip("Height of the visual boxes for the zones.")]
    public float gizmoBoxHeight = 0.2f;

    [Tooltip("Vertical offset for the position of the gizmo boxes.")]
    public float gizmoBoxVerticalOffset = 0.0f;

    [Header("Debug")]
    [Tooltip("Current Z position of the lever handle, for debugging purposes.")]
    [SerializeField] private float currentLeverZPosition;

    void Update()
    {
        if (leverHandle != null)
        {
            // Clamping the lever's Z position within the allowed range between rangeMinZone1 and rangeMaxZone2
            float clampedZPosition = Mathf.Clamp(leverHandle.localPosition.z, rangeMinZone1, rangeMaxZone2);
            leverHandle.localPosition = new Vector3(leverHandle.localPosition.x, leverHandle.localPosition.y, clampedZPosition);

            // Update the current Z position for debugging
            currentLeverZPosition = leverHandle.localPosition.z;

            // Check if the lever handle is in the first zone range
            if (currentLeverZPosition >= rangeMinZone1 && currentLeverZPosition <= rangeMaxZone1)
            {
                Debug.Log("Lever entered Zone 1 range.");
                onZone1Entered?.Invoke();
            }

            // Check if the lever handle is in the second zone range
            if (currentLeverZPosition >= rangeMinZone2 && currentLeverZPosition <= rangeMaxZone2)
            {
                Debug.Log("Lever entered Zone 2 range.");
                onZone2Entered?.Invoke();
            }
        }
    }

    // Draws Gizmos for the position ranges and current position of the lever handle
    private void OnDrawGizmos()
    {
        if (leverHandle != null)
        {
            // Use leverHandle's position and rotation as the origin for Gizmos
            Vector3 gizmoOrigin = leverHandle.position;
            Quaternion gizmoRotation = leverHandle.rotation;

            // Draw first range box (Zone 1)
            Gizmos.color = new Color(1, 0, 0, 0.5f); // Red with transparency
            Gizmos.matrix = Matrix4x4.TRS(gizmoOrigin + leverHandle.up * gizmoBoxVerticalOffset, gizmoRotation, Vector3.one);
            Gizmos.DrawCube(new Vector3(0, 0, (rangeMinZone1 + rangeMaxZone1) / 2), new Vector3(0.1f, gizmoBoxHeight, rangeMaxZone1 - rangeMinZone1));

            // Draw second range box (Zone 2)
            Gizmos.color = new Color(0, 0, 1, 0.5f); // Blue with transparency
            Gizmos.DrawCube(new Vector3(0, 0, (rangeMinZone2 + rangeMaxZone2) / 2), new Vector3(0.1f, gizmoBoxHeight, rangeMaxZone2 - rangeMinZone2));

            // Draw lever handle position indicator (aligned with lever's rotation)
            Gizmos.color = Color.green;
            Gizmos.DrawCube(new Vector3(0, 0, currentLeverZPosition), new Vector3(0.1f, gizmoBoxHeight, 0f));
        }
    }
}
