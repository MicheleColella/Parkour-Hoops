using UnityEngine;
using Oculus;
using Meta.XR;

public class VRLocomotionController : MonoBehaviour
{
    [Header("References")]
    public Transform playerCamera;
    public Transform leftController;
    public Transform rightController;
    public CharacterController characterController;

    [Header("Movement Settings")]
    public float baseWalkSpeed = 2f;
    public float maxWalkSpeed = 15f;
    public float movementSensitivity = 0.02f;
    public float movementLerpSpeed = 10f;
    public float inertiaDuration = 0.5f;
    public float decelerationRate = 2f;
    public float groundCheckDistance = 0.2f;
    public float movementMultiplier = 2f;

    [Header("Jump Settings")]
    public float minJumpForce = 3f;
    public float maxJumpForce = 15f;
    public float maxJumpChargeTime = 5f;
    public float reducedGravity = 0.5f; // Gravità ridotta per il salto
    private float jumpChargeTime = 0f;
    private bool isChargingJump = false;
    private bool isJumping = false;

    [Header("Vibration Settings")]
    public float vibrationStartIntensity = 0.2f;
    public float vibrationMaxIntensity = 1.0f;

    [Header("Snap Turn Settings")]
    public float snapTurnAngle = 45f;
    public float snapTurnCooldown = 0.5f;

    private bool isGrounded;
    private Vector3 playerVelocity;
    private Vector3 leftPreviousPosition;
    private Vector3 rightPreviousPosition;
    private Vector3 currentMovementDirection;
    private float inertiaTime;
    private float currentSpeed;
    private float lastSnapTime;

    void Start()
    {
        leftPreviousPosition = leftController.localPosition;
        rightPreviousPosition = rightController.localPosition;
        currentSpeed = baseWalkSpeed;
    }

    void Update()
    {
        HandleArmSwingMovement();
        HandleJumpCharging();
        HandleSnapTurn();
        ApplyGravity();
        ApplyInertia();
    }

    void HandleArmSwingMovement()
    {
        Vector3 leftMovement = leftController.localPosition - leftPreviousPosition;
        Vector3 rightMovement = rightController.localPosition - rightPreviousPosition;

        leftPreviousPosition = leftController.localPosition;
        rightPreviousPosition = rightController.localPosition;

        float leftSpeed = leftMovement.magnitude / Time.deltaTime;
        float rightSpeed = rightMovement.magnitude / Time.deltaTime;
        float averageHandSpeed = (leftSpeed + rightSpeed) / 2f;

        if (Mathf.Abs(leftMovement.y) > movementSensitivity && Mathf.Abs(rightMovement.y) > movementSensitivity && isGrounded)
        {
            Vector3 forwardDirection = playerCamera.forward;
            forwardDirection.y = 0;
            forwardDirection.Normalize();

            float speedFactor = Mathf.Clamp(averageHandSpeed * movementMultiplier, baseWalkSpeed, maxWalkSpeed);
            currentSpeed = Mathf.Lerp(currentSpeed, speedFactor, movementLerpSpeed * Time.deltaTime);

            currentMovementDirection = Vector3.Lerp(currentMovementDirection, forwardDirection, movementLerpSpeed * Time.deltaTime);

            inertiaTime = inertiaDuration;
        }

        if (!isGrounded)
        {
            characterController.Move(currentMovementDirection * currentSpeed * Time.deltaTime);
        }

        if (!isGrounded)
        {
            playerVelocity.y += Physics.gravity.y * reducedGravity * Time.deltaTime; // Gravità ridotta durante il salto
        }

        characterController.Move(playerVelocity * Time.deltaTime);
    }

    void ApplyInertia()
    {
        if (!isGrounded && isJumping)
        {
            characterController.Move(currentMovementDirection * currentSpeed * Time.deltaTime);
        }

        if (inertiaTime > 0 && isGrounded)
        {
            currentSpeed = Mathf.Lerp(currentSpeed, 0, decelerationRate * Time.deltaTime);
            characterController.Move(currentMovementDirection * currentSpeed * Time.deltaTime);
            inertiaTime -= Time.deltaTime;
        }
        else if (isGrounded)
        {
            currentMovementDirection = Vector3.zero;
            currentSpeed = baseWalkSpeed;
        }
    }

    void ApplyGravity()
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance + 0.1f, LayerMask.GetMask("Default"));

        if (isGrounded && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
            isJumping = false;
        }

        if (!isGrounded)
        {
            playerVelocity.y += Physics.gravity.y * Time.deltaTime;
            characterController.Move(playerVelocity * Time.deltaTime);
        }
    }

    void HandleJumpCharging()
    {
        if (OVRInput.Get(OVRInput.Button.One) && isGrounded && !isJumping)
        {
            isChargingJump = true;
            jumpChargeTime += Time.deltaTime;
            jumpChargeTime = Mathf.Clamp(jumpChargeTime, 0, maxJumpChargeTime);

            // Gestione della vibrazione durante il caricamento
            float vibrationStrength = Mathf.Lerp(vibrationStartIntensity, vibrationMaxIntensity, jumpChargeTime / maxJumpChargeTime);
            OVRInput.SetControllerVibration(0.5f, vibrationStrength, OVRInput.Controller.RTouch);
        }

        if (OVRInput.GetUp(OVRInput.Button.One) && isGrounded && isChargingJump)
        {
            isChargingJump = false;
            
            // Calcola la forza del salto correttamente limitata
            float appliedJumpForce = Mathf.Lerp(minJumpForce, maxJumpForce, jumpChargeTime / maxJumpChargeTime);
            appliedJumpForce = Mathf.Clamp(appliedJumpForce, minJumpForce, maxJumpForce); // Limita il salto massimo

            playerVelocity.y = appliedJumpForce;
            isJumping = true;

            OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.RTouch); // Ferma la vibrazione

            jumpChargeTime = 0f;
        }
    }

    void HandleSnapTurn()
    {
        Vector2 snapInput = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick);

        if (Time.time - lastSnapTime > snapTurnCooldown)
        {
            if (snapInput.x > 0.5f)
            {
                transform.Rotate(0, snapTurnAngle, 0);
                lastSnapTime = Time.time;
            }
            else if (snapInput.x < -0.5f)
            {
                transform.Rotate(0, -snapTurnAngle, 0);
                lastSnapTime = Time.time;
            }
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.normal.y > 0.5f)
        {
            isGrounded = true;
            playerVelocity.y = 0;
            isJumping = false;
        }
    }

    private void OnDisable()
    {
        OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.RTouch);
    }
}
