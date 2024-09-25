using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ClimbingMovementController : MonoBehaviour
{
    [Header("References")]
    public ControllerCollisionDetector leftControllerDetector;
    public ControllerCollisionDetector rightControllerDetector;
    public Transform leftControllerTransform;
    public Transform rightControllerTransform;
    public Rigidbody playerRigidbody;

    [Header("Settings")]
    public float movementMultiplier = 1.0f; // Movement speed multiplier
    public float smoothingFactor = 0.05f;   // Smoothing factor for movement
    public float maxClimbSpeed = 5.0f;      // Maximum climb speed

    private Vector3 leftHandGrabPoint;
    private Vector3 rightHandGrabPoint;
    private bool isLeftHandGrabbing = false;
    private bool isRightHandGrabbing = false;

    private Vector3 currentVelocity = Vector3.zero; // For SmoothDamp

    // Store the previous player position
    private Vector3 previousPlayerPosition;

    void Start()
    {
        if (playerRigidbody == null)
        {
            playerRigidbody = GetComponent<Rigidbody>();
        }

        // Initialize previous player position
        previousPlayerPosition = transform.position;
    }

    void FixedUpdate()
    {
        HandleClimbingMovement();

        // Update the previous player position
        previousPlayerPosition = transform.position;
    }

    void HandleClimbingMovement()
    {
        // Check if the player is in the air
        bool isPlayerInAir = !GetComponent<ClimbingColliderAdjuster>().IsGrounded();

        // Check if either controller is touching a surface
        bool isLeftControllerTouching = leftControllerDetector.isTouchingSurface;
        bool isRightControllerTouching = rightControllerDetector.isTouchingSurface;

        // Handle left hand grabbing state
        if (isLeftControllerTouching && !isLeftHandGrabbing)
        {
            // Left hand starts grabbing
            isLeftHandGrabbing = true;
            leftHandGrabPoint = leftControllerTransform.position;
        }
        else if (!isLeftControllerTouching && isLeftHandGrabbing)
        {
            // Left hand stops grabbing
            isLeftHandGrabbing = false;
        }

        // Handle right hand grabbing state
        if (isRightControllerTouching && !isRightHandGrabbing)
        {
            // Right hand starts grabbing
            isRightHandGrabbing = true;
            rightHandGrabPoint = rightControllerTransform.position;
        }
        else if (!isRightControllerTouching && isRightHandGrabbing)
        {
            // Right hand stops grabbing
            isRightHandGrabbing = false;
        }

        // If the player is in the air and at least one hand is grabbing
        if (isPlayerInAir && (isLeftHandGrabbing || isRightHandGrabbing))
        {
            Vector3 totalMovement = Vector3.zero;

            // Calculate player movement based on left hand
            if (isLeftHandGrabbing)
            {
                Vector3 leftHandDelta = (leftHandGrabPoint - leftControllerTransform.position) - (transform.position - previousPlayerPosition);
                totalMovement += leftHandDelta;
            }

            // Calculate player movement based on right hand
            if (isRightHandGrabbing)
            {
                Vector3 rightHandDelta = (rightHandGrabPoint - rightControllerTransform.position) - (transform.position - previousPlayerPosition);
                totalMovement += rightHandDelta;
            }

            // Apply movement multiplier
            totalMovement *= movementMultiplier;

            // Apply smoothing to the movement
            Vector3 smoothedMovement = Vector3.SmoothDamp(Vector3.zero, totalMovement, ref currentVelocity, smoothingFactor);

            // Limit maximum climb speed
            smoothedMovement = Vector3.ClampMagnitude(smoothedMovement, maxClimbSpeed * Time.fixedDeltaTime);

            // Move the player
            playerRigidbody.MovePosition(playerRigidbody.position + smoothedMovement);
        }
        else
        {
            // Reset current velocity to avoid unwanted movement
            currentVelocity = Vector3.zero;
        }
    }
}
