using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MovementController : MonoBehaviour
{
    public VRLocomotionManager locomotionManager;
    public Transform playerCamera;
    public Transform leftController;
    public Transform rightController;

    private Rigidbody rb;
    private Vector3 leftPrevPos;
    private Vector3 rightPrevPos;
    private Vector3 currentMovementDirection;
    private float inertiaTime;
    private float currentSpeed;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        leftPrevPos = leftController.localPosition;
        rightPrevPos = rightController.localPosition;
        currentSpeed = locomotionManager.baseWalkSpeed;
    }

    void FixedUpdate()
    {
        HandleMovement();
        ApplyInertia();
    }

    void HandleMovement()
    {
        Vector3 leftMovement = leftController.localPosition - leftPrevPos;
        Vector3 rightMovement = rightController.localPosition - rightPrevPos;

        leftPrevPos = leftController.localPosition;
        rightPrevPos = rightController.localPosition;

        float leftSpeed = leftMovement.magnitude / Time.fixedDeltaTime;
        float rightSpeed = rightMovement.magnitude / Time.fixedDeltaTime;
        float averageHandSpeed = (leftSpeed + rightSpeed) / 2f;

        if (Mathf.Abs(leftMovement.y) > locomotionManager.movementSensitivity && Mathf.Abs(rightMovement.y) > locomotionManager.movementSensitivity)
        {
            Vector3 forwardDir = playerCamera.forward;
            forwardDir.y = 0;
            forwardDir.Normalize();

            float speedFactor = Mathf.Clamp(averageHandSpeed * locomotionManager.movementMultiplier, locomotionManager.baseWalkSpeed, locomotionManager.maxWalkSpeed);
            currentSpeed = Mathf.Lerp(currentSpeed, speedFactor, locomotionManager.movementLerpSpeed * Time.fixedDeltaTime);
            currentMovementDirection = Vector3.Lerp(currentMovementDirection, forwardDir, locomotionManager.movementLerpSpeed * Time.fixedDeltaTime);

            inertiaTime = locomotionManager.inertiaDuration;
        }

        // Aggiunta di interpolazione per la velocità verticale quando a terra
        if (IsGrounded())
        {
            Vector3 velocity = rb.velocity;
            velocity.y = Mathf.Lerp(velocity.y, 0, Time.fixedDeltaTime * 10f); // Interpola Y verso 0
            rb.velocity = velocity;
        }

        if (inertiaTime > 0)
        {
            Vector3 desiredVelocity = currentMovementDirection * currentSpeed;
            Vector3 newVelocity = new Vector3(desiredVelocity.x, rb.velocity.y, desiredVelocity.z);
            rb.velocity = Vector3.Lerp(rb.velocity, newVelocity, locomotionManager.movementLerpSpeed * Time.fixedDeltaTime);
            inertiaTime -= Time.fixedDeltaTime;
        }
    }

    void ApplyInertia()
    {
        if (inertiaTime > 0)
        {
            currentSpeed = Mathf.Lerp(currentSpeed, 0, locomotionManager.decelerationRate * Time.fixedDeltaTime);
        }
        else
        {
            currentMovementDirection = Vector3.zero;
            currentSpeed = locomotionManager.baseWalkSpeed;
        }
    }

    bool IsGrounded()
    {
        CapsuleCollider playerCollider = GetComponent<CapsuleCollider>();
        Vector3 groundCheckPos = playerCollider.bounds.center - new Vector3(0, playerCollider.bounds.extents.y, 0);
        return Physics.OverlapSphere(groundCheckPos, locomotionManager.groundCheckRadius, locomotionManager.groundLayers).Length > 0;
    }
}
