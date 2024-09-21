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

    [Header("Ground Check Settings")]
    public LayerMask groundLayers;  // I layer che rappresentano il pavimento
    public float groundCheckRadius = 0.3f;  // Il raggio dell'OverlapSphere per controllare il terreno

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
        CheckIfGrounded(); // Aggiorna lo stato di isGrounded usando OverlapSphere
        ApplyGravity();
        HandleArmSwingMovement();
        ApplyInertia();
        HandleJumpCharging();
        HandleSnapTurn();
    }

    // Metodo per controllare se il giocatore è a terra usando OverlapSphere
    void CheckIfGrounded()
    {
        Vector3 groundCheckPosition = playerCollider.bounds.center - new Vector3(0, playerCollider.bounds.extents.y, 0);
        bool wasGrounded = isGrounded; // Memorizza lo stato precedente
        isGrounded = Physics.OverlapSphere(groundCheckPosition, groundCheckRadius, groundLayers).Length > 0;

        // Se il giocatore era in aria e ora è a terra, resetta isJumping
        if (!wasGrounded && isGrounded)
        {
            isJumping = false;
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

    void HandleArmSwingMovement()
    {
        Vector3 leftMovement = leftController.localPosition - leftPreviousPosition;
        Vector3 rightMovement = rightController.localPosition - rightPreviousPosition;

        leftPreviousPosition = leftController.localPosition;
        rightPreviousPosition = rightController.localPosition;

        float leftSpeed = leftMovement.magnitude / Time.fixedDeltaTime;
        float rightSpeed = rightMovement.magnitude / Time.fixedDeltaTime;
        float averageHandSpeed = (leftSpeed + rightSpeed) / 2f;

        if (Mathf.Abs(leftMovement.y) > movementSensitivity && Mathf.Abs(rightMovement.y) > movementSensitivity)
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
            Vector3 movement = currentMovementDirection * currentSpeed;

            // Imposta la velocità del rigidbody mantenendo la componente verticale
            playerRigidbody.velocity = new Vector3(movement.x, playerRigidbody.velocity.y, movement.z);
        }
    }

    void ApplyInertia()
    {
        if (inertiaTime > 0)
        {
            currentSpeed = Mathf.Lerp(currentSpeed, 0, decelerationRate * Time.fixedDeltaTime);
            inertiaTime -= Time.fixedDeltaTime;
        }
        else
        {
            currentMovementDirection = Vector3.zero;
            currentSpeed = baseWalkSpeed;
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

            // Calcola la velocità orizzontale corrente
            Vector3 horizontalVelocity = currentMovementDirection * currentSpeed;

            // Imposta la velocità del rigidbody includendo sia la componente orizzontale che verticale
            playerRigidbody.velocity = new Vector3(horizontalVelocity.x, appliedJumpForce, horizontalVelocity.z);

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

    // Disegna Gizmo per visualizzare il controllo dell'OverlapSphere
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        if (playerCollider != null)
        {
            Vector3 groundCheckPosition = playerCollider.bounds.center - new Vector3(0, playerCollider.bounds.extents.y, 0);
            Gizmos.DrawWireSphere(groundCheckPosition, groundCheckRadius);
        }
    }

    // Getter per le variabili che servono ad altri script
    public bool IsGrounded()
    {
        return isGrounded;
    }

    public float GetCurrentSpeed()
    {
        return currentSpeed;
    }

    public bool IsJumping()
    {
        return isJumping;
    }

    public float GetJumpChargeTime()
    {
        return jumpChargeTime;
    }
}
