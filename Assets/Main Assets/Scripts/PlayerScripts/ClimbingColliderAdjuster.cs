using UnityEngine;
using System.Collections;

public class ClimbingColliderAdjuster : MonoBehaviour
{
    [Header("References")]
    public CapsuleCollider playerCollider;             // Reference to the player's CapsuleCollider
    public ControllerCollisionDetector leftController;  // Reference to the left controller's script
    public ControllerCollisionDetector rightController; // Reference to the right controller's script

    [Header("Collider Settings")]
    public float adjustedHeight = 1.0f;      // Height of the collider when climbing
    public float adjustSpeed = 2.0f;         // Speed at which the collider height adjusts

    private float originalHeight;
    private Vector3 originalCenter;

    private float targetHeight;
    private Vector3 targetCenter;

    void Start()
    {
        if (playerCollider == null)
        {
            playerCollider = GetComponent<CapsuleCollider>();
        }

        // Store the original height and center of the capsule collider
        originalHeight = playerCollider.height;
        originalCenter = playerCollider.center;

        // Initialize target values
        targetHeight = originalHeight;
        targetCenter = originalCenter;
    }

    void Update()
    {
        // Check if either controller is touching a surface
        bool isControllerTouchingSurface = leftController.isTouchingSurface || rightController.isTouchingSurface;

        // If the player is in the air and at least one controller is touching a surface, adjust the collider
        if (!IsGrounded() && isControllerTouchingSurface)
        {
            AdjustColliderHeight();
        }
        else
        {
            RestoreColliderHeight();
        }

        // Smoothly interpolate the collider's height and center to the target values
        playerCollider.height = Mathf.Lerp(playerCollider.height, targetHeight, Time.deltaTime * adjustSpeed);
        playerCollider.center = Vector3.Lerp(playerCollider.center, targetCenter, Time.deltaTime * adjustSpeed);
    }

    private void AdjustColliderHeight()
    {
        // Set the target height to the adjusted height
        targetHeight = adjustedHeight;

        // Calculate the center adjustment to shorten from the bottom
        float heightDifference = originalHeight - adjustedHeight;
        targetCenter = originalCenter + new Vector3(0, heightDifference / 2f, 0);
    }

    private void RestoreColliderHeight()
    {
        // Set the target height and center back to the original values
        targetHeight = originalHeight;
        targetCenter = originalCenter;
    }

    private bool IsGrounded()
    {
        // Use the adjusted capsule collider dimensions for ground check
        float radius = playerCollider.radius * 0.9f;
        Vector3 bottom = transform.position + playerCollider.center - Vector3.up * (playerCollider.height / 2f - radius);

        // Check if the capsule collider is colliding with the ground
        return Physics.CheckSphere(bottom, radius, LayerMask.GetMask("Ground"), QueryTriggerInteraction.Ignore);
    }

    private void OnDrawGizmos()
    {
        if (playerCollider != null)
        {
            Gizmos.color = Color.green;

            // Calculate top and bottom positions of the CapsuleCollider
            Vector3 colliderWorldCenter = transform.TransformPoint(playerCollider.center);
            float height = playerCollider.height;
            float radius = playerCollider.radius;
            float halfHeight = Mathf.Max(0, (height / 2f) - radius);

            Vector3 top = colliderWorldCenter + transform.up * halfHeight;
            Vector3 bottom = colliderWorldCenter - transform.up * halfHeight;

            // Draw the CapsuleCollider
            Gizmos.DrawWireSphere(top, radius);
            Gizmos.DrawWireSphere(bottom, radius);

            // Draw lines connecting the spheres
            Gizmos.DrawLine(top + transform.right * radius, bottom + transform.right * radius);
            Gizmos.DrawLine(top - transform.right * radius, bottom - transform.right * radius);
            Gizmos.DrawLine(top + transform.forward * radius, bottom + transform.forward * radius);
            Gizmos.DrawLine(top - transform.forward * radius, bottom - transform.forward * radius);
        }
    }
}
