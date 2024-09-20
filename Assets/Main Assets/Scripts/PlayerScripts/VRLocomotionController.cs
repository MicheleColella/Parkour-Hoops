using UnityEngine;
using Oculus;
using Meta.XR;

public class VRLocomotionController : MonoBehaviour
{
    [Header("References")]
    public Transform playerCamera;
    public Transform leftController;
    public Transform rightController;
    public Rigidbody playerRigidbody;
    public CapsuleCollider playerCollider;

    [Header("Movement Settings")]
    public float baseWalkSpeed = 2f;
    public float maxWalkSpeed = 15f;
    public float movementSensitivity = 0.02f;
    public float movementLerpSpeed = 10f;
    public float inertiaDuration = 0.5f;
    public float decelerationRate = 2f;
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

    void FixedUpdate()
    {
        ApplyGravity();
        HandleArmSwingMovement();
        ApplyInertia();
        HandleJumpCharging();
        HandleSnapTurn();
    }

    void HandleArmSwingMovement()
    {
        Vector3 leftMovement = leftController.localPosition - leftPreviousPosition;
        Vector3 rightMovement = rightController.localPosition - rightPreviousPosition;

        leftPreviousPosition = leftController.localPosition;
        rightPreviousPosition = rightController.localPosition;

        float leftSpeed = leftMovement.magnitude / Time.fixedDeltaTime;
        float rightSpeed = rightMovement.magnitude / Time.fixedDeltaTime;
        float averageHandSpeed = (leftSpeed + rightSpeed) / 2f;

        if (Mathf.Abs(leftMovement.y) > movementSensitivity && Mathf.Abs(rightMovement.y) > movementSensitivity && isGrounded)
        {
            Vector3 forwardDirection = playerCamera.forward;
            forwardDirection.y = 0;
            forwardDirection.Normalize();

            float speedFactor = Mathf.Clamp(averageHandSpeed * movementMultiplier, baseWalkSpeed, maxWalkSpeed);
            currentSpeed = Mathf.Lerp(currentSpeed, speedFactor, movementLerpSpeed * Time.fixedDeltaTime);

            currentMovementDirection = Vector3.Lerp(currentMovementDirection, forwardDirection, movementLerpSpeed * Time.fixedDeltaTime);

            inertiaTime = inertiaDuration;
        }

        if (inertiaTime > 0)
        {
            Vector3 movement = currentMovementDirection * currentSpeed * Time.fixedDeltaTime;

            // Se è a mezz'aria, continua il movimento attuale senza fermarsi
            if (!isGrounded)
            {
                playerRigidbody.velocity = new Vector3(movement.x, playerRigidbody.velocity.y, movement.z);
            }
            else
            {
                // Movimento fluido a terra
                playerRigidbody.MovePosition(playerRigidbody.position + movement);
            }
        }
    }

    void ApplyInertia()
    {
        if (inertiaTime > 0 && isGrounded)
        {
            currentSpeed = Mathf.Lerp(currentSpeed, 0, decelerationRate * Time.fixedDeltaTime);
            inertiaTime -= Time.fixedDeltaTime;
        }
        else if (isGrounded)
        {
            currentMovementDirection = Vector3.zero;
            currentSpeed = baseWalkSpeed;
        }
    }


    void ApplyGravity()
    {
        if (!isGrounded)
        {
            Vector3 gravity = Physics.gravity * (isJumping ? reducedGravity : 1f);
            playerRigidbody.AddForce(gravity, ForceMode.Acceleration);
        }
    }

    void HandleJumpCharging()
    {
        if (OVRInput.Get(OVRInput.Button.One) && isGrounded && !isJumping)
        {
            isChargingJump = true;
            jumpChargeTime += Time.fixedDeltaTime;
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

            playerRigidbody.AddForce(Vector3.up * appliedJumpForce, ForceMode.VelocityChange);
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

    private void OnCollisionEnter(Collision collision)
    {
        // Controlla se il giocatore tocca il terreno
        if (collision.contacts[0].normal.y > 0.5f)
        {
            isGrounded = true;
            isJumping = false;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        // Quando il giocatore lascia il terreno
        if (collision.contacts.Length > 0 && collision.contacts[0].normal.y > 0.5f)
        {
            isGrounded = false;
        }
    }

    private void OnDisable()
    {
        OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.RTouch);
    }
}
