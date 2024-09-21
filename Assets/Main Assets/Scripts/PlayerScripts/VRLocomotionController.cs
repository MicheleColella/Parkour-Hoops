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
    public float movementMultiplier = 5f;  // Aumentato per ridurre la necessità di movimenti veloci

    [Header("Jump Settings")]
    public float minJumpForce = 5f;  // Aumentato per salti più alti con piccoli carichi
    public float maxJumpForce = 15f;
    public float maxJumpChargeTime = 1.5f;  // Ridotto per salti più reattivi
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

    // Nuove variabili per la gestione delle collisioni con i muri
    private bool isTouchingWall = false;
    private Vector3 currentWallNormal = Vector3.zero;

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

        // Rimuovi la proiezione della direzione del movimento sul piano del muro
        // Questo permette al player di muoversi liberamente lontano dal muro
        /*
        if (isTouchingWall && currentWallNormal != Vector3.zero)
        {
            currentMovementDirection = Vector3.ProjectOnPlane(currentMovementDirection, currentWallNormal).normalized;
        }
        */

        if (inertiaTime > 0)
        {
            Vector3 desiredVelocity = currentMovementDirection * currentSpeed;
            Vector3 newVelocity = new Vector3(desiredVelocity.x, playerRigidbody.velocity.y, desiredVelocity.z);
            playerRigidbody.velocity = Vector3.Lerp(playerRigidbody.velocity, newVelocity, movementLerpSpeed * Time.fixedDeltaTime);
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

    void OnCollisionStay(Collision collision)
    {
        // Controlla se il player sta camminando contro un muro mentre è in aria
        if (!isGrounded && IsWall(collision))
        {
            currentWallNormal = GetWallNormal(collision);

            // Calcola la componente di velocità verso il muro
            float dot = Vector3.Dot(playerRigidbody.velocity, currentWallNormal);
            if (dot > 0)
            {
                // Rimuove la componente di velocità che spinge il player nel muro
                playerRigidbody.velocity -= dot * currentWallNormal;
            }

            // Applica una piccola forza verso il basso per simulare lo scivolamento
            Vector3 slideForce = new Vector3(0, -0.5f, 0);  // Ridotto per diminuire lo scivolamento
            playerRigidbody.AddForce(slideForce, ForceMode.Acceleration);
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (IsWall(collision))
        {
            currentWallNormal = Vector3.zero;
        }
    }

    // Metodo per controllare se l'oggetto in collisione è un muro
    bool IsWall(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
            // Se la normale della superficie ha un angolo quasi verticale (tra 75 e 90 gradi rispetto all'orizzontale)
            if (Mathf.Abs(Vector3.Dot(contact.normal, Vector3.up)) < 0.5f)
            {
                return true;  // È un muro
            }
        }
        return false;
    }

    // Ottieni la normale del muro con cui il player è in contatto
    Vector3 GetWallNormal(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
            // Trova la normale del muro
            if (Mathf.Abs(Vector3.Dot(contact.normal, Vector3.up)) < 0.5f)
            {
                return contact.normal;  // Restituisci la normale del muro
            }
        }
        return Vector3.zero;  // Restituisci un vettore nullo se non è un muro (non dovrebbe accadere qui)
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
