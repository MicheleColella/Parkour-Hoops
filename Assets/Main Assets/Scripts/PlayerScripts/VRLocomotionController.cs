using UnityEngine;
using Oculus.Platform;
using Oculus.Platform.Models;
using Oculus;
using Meta.XR;

public class VRLocomotionController : MonoBehaviour
{
    [Header("References")]
    public Transform playerCamera; // Riferimento alla telecamera del giocatore
    public Transform leftController; // Riferimento al controller sinistro
    public Transform rightController; // Riferimento al controller destro
    public CharacterController characterController; // Per la gestione del movimento del player

    [Header("Movement Settings")]
    public float baseWalkSpeed = 2f; // Velocità di camminata base
    public float maxWalkSpeed = 15f; // Velocità massima di camminata aumentata
    public float movementSensitivity = 0.02f; // Sensibilità del movimento delle mani
    public float movementLerpSpeed = 10f; // Velocità del movimento fluido aumentata per reattività
    public float inertiaDuration = 0.5f; // Durata dello scivolamento prima di fermarsi
    public float decelerationRate = 2f; // Tasso di rallentamento quando il player smette di muoversi
    public float groundCheckDistance = 0.2f; // Distanza per il controllo se il player è a terra
    public float movementMultiplier = 2f; // Moltiplicatore per scalare la velocità basata sul movimento delle mani

    [Header("Jump Settings")]
    public float minJumpForce = 3f; // Forza minima del salto
    public float maxJumpForce = 15f; // Forza massima del salto
    public float maxJumpChargeTime = 5f; // Tempo massimo per caricare il salto
    private float jumpChargeTime = 0f; // Tempo di caricamento attuale
    private bool isChargingJump = false; // Stato di caricamento del salto
    private bool isJumping = false; // Stato del salto

    [Header("Vibration Settings")]
    public float vibrationStartIntensity = 0.2f; // Intensità di vibrazione iniziale
    public float vibrationMaxIntensity = 1.0f; // Intensità massima di vibrazione

    [Header("Snap Turn Settings")]
    public float snapTurnAngle = 45f; // Angolo di rotazione per lo snap turn
    public float snapTurnCooldown = 0.5f; // Tempo minimo tra uno snap e l'altro

    private bool isGrounded; // Controllo se il player è a terra
    private Vector3 playerVelocity; // Velocità del player
    private Vector3 leftPreviousPosition; // Posizione precedente del controller sinistro
    private Vector3 rightPreviousPosition; // Posizione precedente del controller destro
    private Vector3 currentMovementDirection; // Direzione attuale del movimento
    private float inertiaTime; // Timer per lo scivolamento
    private float currentSpeed; // Velocità corrente del player

    private float lastSnapTime; // Tempo dell'ultimo snap turn

    void Start()
    {
        // Salviamo la posizione iniziale dei controller
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
        // Calcoliamo il movimento relativo delle mani
        Vector3 leftMovement = leftController.localPosition - leftPreviousPosition;
        Vector3 rightMovement = rightController.localPosition - rightPreviousPosition;

        // Aggiorniamo la posizione precedente
        leftPreviousPosition = leftController.localPosition;
        rightPreviousPosition = rightController.localPosition;

        // Calcoliamo la velocità di movimento delle mani
        float leftSpeed = leftMovement.magnitude / Time.deltaTime;
        float rightSpeed = rightMovement.magnitude / Time.deltaTime;

        // Velocità media delle mani
        float averageHandSpeed = (leftSpeed + rightSpeed) / 2f;

        // Se entrambe le mani si muovono significativamente, calcoliamo la velocità e muoviamo il player
        if (Mathf.Abs(leftMovement.y) > movementSensitivity && Mathf.Abs(rightMovement.y) > movementSensitivity && isGrounded)
        {
            // Direzione in avanti della telecamera
            Vector3 forwardDirection = playerCamera.forward;
            forwardDirection.y = 0; // Manteniamo il movimento solo sull'asse orizzontale
            forwardDirection.Normalize();

            // Calcoliamo la velocità del player in base alla velocità delle mani
            // Utilizziamo il movementMultiplier per scalare meglio la velocità
            float speedFactor = Mathf.Clamp(averageHandSpeed * movementMultiplier, baseWalkSpeed, maxWalkSpeed);
            currentSpeed = Mathf.Lerp(currentSpeed, speedFactor, movementLerpSpeed * Time.deltaTime);

            // Impostiamo la direzione del movimento
            currentMovementDirection = Vector3.Lerp(currentMovementDirection, forwardDirection, movementLerpSpeed * Time.deltaTime);

            // Resettiamo il timer dell'inertia
            inertiaTime = inertiaDuration;
        }

        // Se il player non è a terra, manteniamo la direzione attuale
        if (!isGrounded)
        {
            characterController.Move(currentMovementDirection * currentSpeed * Time.deltaTime);
        }

        // Applica la gravità se non si sta muovendo o durante il salto
        if (!isGrounded)
        {
            playerVelocity.y += Physics.gravity.y * Time.deltaTime;
        }

        // Applica la velocità al character controller
        characterController.Move(playerVelocity * Time.deltaTime);
    }

    void ApplyInertia()
    {
        // Manteniamo il movimento orizzontale mentre siamo in aria
        if (!isGrounded && isJumping)
        {
            characterController.Move(currentMovementDirection * currentSpeed * Time.deltaTime);
        }

        // Quando il player tocca terra, applichiamo l'inerzia per rallentare
        if (inertiaTime > 0 && isGrounded)
        {
            // Deceleriamo gradualmente il movimento
            currentSpeed = Mathf.Lerp(currentSpeed, 0, decelerationRate * Time.deltaTime);
            characterController.Move(currentMovementDirection * currentSpeed * Time.deltaTime);
            inertiaTime -= Time.deltaTime;
        }
        else if (isGrounded)
        {
            currentMovementDirection = Vector3.zero; // Ferma completamente il movimento
            currentSpeed = baseWalkSpeed; // Reset della velocità alla velocità di base
        }
    }

    void ApplyGravity()
    {
        // Controlla se il player è a terra
        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance + 0.1f, LayerMask.GetMask("Default"));

        // Se il player è a terra, resetta la velocità verticale
        if (isGrounded && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
            isJumping = false; // Il player ha toccato terra
        }

        // Se il player non è a terra, applica la gravità
        if (!isGrounded)
        {
            playerVelocity.y += Physics.gravity.y * Time.deltaTime;
            characterController.Move(playerVelocity * Time.deltaTime);
        }
    }

    void HandleJumpCharging()
    {
        // Controlla se il tasto A del controller destro viene premuto per caricare il salto
        if (OVRInput.Get(OVRInput.Button.One) && isGrounded && !isJumping)
        {
            isChargingJump = true;
            jumpChargeTime += Time.deltaTime;
            jumpChargeTime = Mathf.Clamp(jumpChargeTime, 0, maxJumpChargeTime);

            // Gestione della vibrazione durante il caricamento
            float vibrationStrength = Mathf.Lerp(vibrationStartIntensity, vibrationMaxIntensity, jumpChargeTime / maxJumpChargeTime);
            OVRInput.SetControllerVibration(vibrationStrength, vibrationStrength, OVRInput.Controller.RTouch);
        }

        // Quando il tasto viene rilasciato, salta con la forza caricata
        if (OVRInput.GetUp(OVRInput.Button.One) && isGrounded && isChargingJump)
        {
            isChargingJump = false;
            float appliedJumpForce = Mathf.Lerp(minJumpForce, maxJumpForce, jumpChargeTime / maxJumpChargeTime);
            playerVelocity.y = appliedJumpForce;
            isJumping = true; // Il player sta saltando

            // Ferma la vibrazione
            OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.RTouch);

            // Resetta il tempo di caricamento del salto
            jumpChargeTime = 0f;
        }
    }

    void HandleSnapTurn()
    {
        // Rotazione a scatto con lo stick destro (con cooldown)
        Vector2 snapInput = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick);

        if (Time.time - lastSnapTime > snapTurnCooldown)
        {
            if (snapInput.x > 0.5f) // Snap a destra
            {
                transform.Rotate(0, snapTurnAngle, 0);
                lastSnapTime = Time.time;
            }
            else if (snapInput.x < -0.5f) // Snap a sinistra
            {
                transform.Rotate(0, -snapTurnAngle, 0);
                lastSnapTime = Time.time;
            }
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // Controllo se il player è a terra
        if (hit.normal.y > 0.5f)
        {
            isGrounded = true;
            playerVelocity.y = 0;
            isJumping = false; // Ora il giocatore può saltare di nuovo
        }
    }

    private void OnDisable()
    {
        // Assicurati di fermare la vibrazione quando lo script viene disabilitato
        OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.RTouch);
    }
}
