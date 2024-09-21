using UnityEngine;
using Oculus;
using Meta.XR;

public class VRLocomotionController : MonoBehaviour
{
    private enum JumpState
    {
        NotCharging,
        Charging,
        InAir
    }

    private JumpState jumpState = JumpState.NotCharging;  // Stato iniziale
    private bool isJumping = false;

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
    public float maxJumpChargeTime = 3f;  // Impostato a 3 come specificato
    public float reducedGravity = 0.5f; // Gravità ridotta per il salto
    private float jumpChargeTime = 0f;

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
        HandleJumpState();  // Gestisce il salto con la macchina a stati finiti
        HandleSnapTurn();
    }

    // Metodo per controllare se il giocatore è a terra usando OverlapSphere
    void CheckIfGrounded()
    {
        Vector3 groundCheckPosition = playerCollider.bounds.center - new Vector3(0, playerCollider.bounds.extents.y, 0);
        bool wasGrounded = isGrounded; // Memorizza lo stato precedente
        isGrounded = Physics.OverlapSphere(groundCheckPosition, groundCheckRadius, groundLayers).Length > 0;

        // Se il giocatore atterra, resetta lo stato di salto
        if (!wasGrounded && isGrounded)
        {
            jumpState = JumpState.NotCharging;
            isJumping = false;
            jumpChargeTime = 0f;

            // Ferma la vibrazione nel caso in cui sia ancora attiva
            OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.RTouch);
        }
    }

    void ApplyGravity()
    {
        if (!isGrounded)
        {
            Vector3 gravity = Physics.gravity * (jumpState == JumpState.InAir ? reducedGravity : 1f);
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

    // Gestione dello stato del salto usando la macchina a stati finiti
    void HandleJumpState()
    {
        switch (jumpState)
        {
            case JumpState.NotCharging:
                if (OVRInput.Get(OVRInput.Button.One) && isGrounded)
                {
                    // Inizia a caricare il salto
                    jumpState = JumpState.Charging;
                    jumpChargeTime = 0f;  // Reset del tempo di carica

                    // Inizia la vibrazione
                    float vibrationStrength = Mathf.Lerp(vibrationStartIntensity, vibrationMaxIntensity, jumpChargeTime / maxJumpChargeTime);
                    OVRInput.SetControllerVibration(0.5f, vibrationStrength, OVRInput.Controller.RTouch);
                }
                else
                {
                    // Assicura che la vibrazione sia fermata
                    OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.RTouch);
                }
                break;

            case JumpState.Charging:
                if (!OVRInput.Get(OVRInput.Button.One))
                {
                    // Il tasto è stato rilasciato
                    if (isGrounded)
                    {
                        // Esegui il salto
                        float appliedJumpForce = Mathf.Lerp(minJumpForce, maxJumpForce, jumpChargeTime / maxJumpChargeTime);
                        appliedJumpForce = Mathf.Clamp(appliedJumpForce, minJumpForce, maxJumpForce);

                        // Calcola la velocità orizzontale corrente
                        Vector3 horizontalVelocity = currentMovementDirection * currentSpeed;

                        // Imposta la velocità del rigidbody includendo sia la componente orizzontale che verticale
                        playerRigidbody.velocity = new Vector3(horizontalVelocity.x, appliedJumpForce, horizontalVelocity.z);

                        // Passa allo stato InAir
                        jumpState = JumpState.InAir;
                        isJumping = true;
                    }
                    else
                    {
                        // Non può saltare perché non è a terra
                        jumpState = JumpState.NotCharging;
                    }

                    // Resetta il tempo di carica
                    jumpChargeTime = 0f;

                    // Ferma la vibrazione
                    OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.RTouch);
                }
                else if (!isGrounded)
                {
                    // Il giocatore ha lasciato il terreno durante il caricamento, annulla il salto
                    jumpState = JumpState.NotCharging;
                    jumpChargeTime = 0f;

                    // Ferma la vibrazione
                    OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.RTouch);
                }
                else
                {
                    // Continua a caricare il salto
                    jumpChargeTime += Time.fixedDeltaTime;
                    jumpChargeTime = Mathf.Clamp(jumpChargeTime, 0, maxJumpChargeTime);

                    // Aggiorna la vibrazione
                    float vibrationStrength = Mathf.Lerp(vibrationStartIntensity, vibrationMaxIntensity, jumpChargeTime / maxJumpChargeTime);
                    OVRInput.SetControllerVibration(0.5f, vibrationStrength, OVRInput.Controller.RTouch);
                }
                break;

            case JumpState.InAir:
                if (isGrounded)
                {
                    // Il giocatore è atterrato
                    jumpState = JumpState.NotCharging;
                    isJumping = false;
                    jumpChargeTime = 0f;

                    // Assicura che la vibrazione sia fermata
                    OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.RTouch);
                }
                break;
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
