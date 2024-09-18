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
    public float maxWalkSpeed = 5f; // Velocità massima di camminata
    public float movementSensitivity = 0.02f; // Sensibilità del movimento delle mani
    public float movementLerpSpeed = 5f; // Velocità del movimento fluido
    public float inertiaDuration = 0.5f; // Durata dello scivolamento prima di fermarsi
    public float decelerationRate = 2f; // Tasso di rallentamento quando il player smette di muoversi
    public float groundCheckDistance = 0.2f; // Distanza per il controllo se il player è a terra

    [Header("Jump Settings")]
    public float jumpForce = 8f; // Forza del salto aumentata per un salto più alto
    public float gravityDuringJump = -4.0f; // Gravità ridotta durante il salto per rallentare la discesa
    private float defaultGravity = -9.81f; // Gravità normale

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
    private bool isJumping; // Controllo se il player è in aria

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
        HandleJump();
        HandleDebugMovement();
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

            // Calcoliamo la velocità del player in base alla velocità delle mani
            float speedFactor = Mathf.Clamp(averageHandSpeed, 0, maxWalkSpeed);
            currentSpeed = Mathf.Lerp(currentSpeed, speedFactor, movementLerpSpeed * Time.deltaTime);

            // Interpoliamo la direzione del movimento
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
            playerVelocity.y += defaultGravity * Time.deltaTime;
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
        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance);

        // Se il player è a terra, resetta la velocità verticale
        if (isGrounded && playerVelocity.y < 0)
        {
            playerVelocity.y = 0f;
            isJumping = false; // Il player ha toccato terra
        }

        // Se il player non è a terra, applica la gravità
        if (!isGrounded)
        {
            // Gravità ridotta durante il salto per rallentare la discesa
            playerVelocity.y += gravityDuringJump * Time.deltaTime;
            characterController.Move(playerVelocity * Time.deltaTime);
        }
    }

    void HandleJump()
    {
        // Gestione del salto quando si preme il tasto A del controller destro
        if (OVRInput.GetDown(OVRInput.Button.One) && isGrounded)
        {
            playerVelocity.y = jumpForce;
            isGrounded = false;
            isJumping = true; // Il player è in aria
        }
    }

    void HandleDebugMovement()
    {
        // Movimento di debug usando lo stick sinistro
        Vector2 input = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);
        Vector3 moveDirection = new Vector3(input.x, 0, input.y);
        moveDirection = playerCamera.TransformDirection(moveDirection);
        moveDirection.y = 0;

        if (input.magnitude > 0 && isGrounded)
        {
            // Applichiamo la stessa logica di inertia al movimento con lo stick
            currentMovementDirection = Vector3.Lerp(currentMovementDirection, moveDirection, movementLerpSpeed * Time.deltaTime);
            inertiaTime = inertiaDuration;
        }

        characterController.Move(currentMovementDirection * currentSpeed * Time.deltaTime);
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
        }
    }
}
