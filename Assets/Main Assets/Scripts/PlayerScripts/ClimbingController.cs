using UnityEngine;

public class ClimbingController : MonoBehaviour
{
    public Rigidbody playerRigidbody;
    public HandPhysics leftHandPhysics;
    public HandPhysics rightHandPhysics;
    public MovementController movementController;
    public JumpController jumpController;
    public float climbingSpeedMultiplier = 1f;
    public float inertiaMultiplier = 0.1f; // Regolato per un effetto di inerzia moderato
    public float velocityThreshold = 1f; // Soglia per movimenti veloci

    private bool isClimbing = false;
    private Vector3 leftHandPrevControllerLocalPos;
    private Vector3 rightHandPrevControllerLocalPos;
    private Vector3 leftHandVelocity = Vector3.zero;
    private Vector3 rightHandVelocity = Vector3.zero;

    private void Start()
    {
        if (playerRigidbody == null)
        {
            playerRigidbody = GetComponent<Rigidbody>();
        }
    }

    void FixedUpdate()
    {
        bool leftGrabbing = leftHandPhysics.isGrabbing;
        bool rightGrabbing = rightHandPhysics.isGrabbing;

        if (leftGrabbing || rightGrabbing)
        {
            if (!isClimbing)
            {
                isClimbing = true;
                movementController.enabled = false;
                jumpController.enabled = false;
                playerRigidbody.useGravity = false;
                playerRigidbody.velocity = Vector3.zero;
                leftHandPrevControllerLocalPos = playerRigidbody.transform.InverseTransformPoint(leftHandPhysics.ControllerTransform.position);
                rightHandPrevControllerLocalPos = playerRigidbody.transform.InverseTransformPoint(rightHandPhysics.ControllerTransform.position);
            }

            Vector3 climbingMovement = Vector3.zero;

            if (leftGrabbing)
            {
                climbingMovement += CalculateClimbingMovement(leftHandPhysics, ref leftHandPrevControllerLocalPos, ref leftHandVelocity);
            }
            if (rightGrabbing)
            {
                climbingMovement += CalculateClimbingMovement(rightHandPhysics, ref rightHandPrevControllerLocalPos, ref rightHandVelocity);
            }

            // Movimento fluido con Lerp
            Vector3 targetPosition = playerRigidbody.position - climbingMovement * climbingSpeedMultiplier;
            playerRigidbody.position = Vector3.Lerp(playerRigidbody.position, targetPosition, Time.fixedDeltaTime * 10f);

            // Applicazione dell'inerzia
            ApplyInertia();
        }
        else
        {
            if (isClimbing)
            {
                isClimbing = false;
                movementController.enabled = true;
                jumpController.enabled = true;
                playerRigidbody.useGravity = true;
                playerRigidbody.velocity = Vector3.zero;
            }
        }
    }

    private Vector3 CalculateClimbingMovement(HandPhysics handPhysics, ref Vector3 handPrevControllerLocalPos, ref Vector3 handVelocity)
    {
        Vector3 currentControllerLocalPos = playerRigidbody.transform.InverseTransformPoint(handPhysics.ControllerTransform.position);
        Vector3 handDelta = currentControllerLocalPos - handPrevControllerLocalPos;
        handVelocity = handDelta / Time.fixedDeltaTime;
        handPrevControllerLocalPos = currentControllerLocalPos;
        return handDelta;
    }

    private void ApplyInertia()
    {
        Vector3 totalHandVelocity = Vector3.zero;
        if (leftHandPhysics.isGrabbing && leftHandVelocity.magnitude > velocityThreshold)
        {
            totalHandVelocity += leftHandVelocity;
        }
        if (rightHandPhysics.isGrabbing && rightHandVelocity.magnitude > velocityThreshold)
        {
            totalHandVelocity += rightHandVelocity;
        }

        if (totalHandVelocity != Vector3.zero)
        {
            Vector3 inertiaForce = totalHandVelocity * inertiaMultiplier;
            playerRigidbody.AddForce(-inertiaForce, ForceMode.VelocityChange);
        }
    }
}
