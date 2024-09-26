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
    public CapsuleCollider playerCollider; // Riferimento al collider del giocatore

    [Header("Settings")]
    public float movementMultiplier = 1.0f; // Moltiplicatore per il movimento di arrampicata
    public float maxClimbSpeed = 5.0f;      // Velocità massima di arrampicata
    public float smoothingFactor = 0.1f;    // Fattore di smoothing per il movimento

    private Vector3 leftHandGrabPoint;
    private Vector3 rightHandGrabPoint;
    private bool isLeftHandGrabbing = false;
    private bool isRightHandGrabbing = false;

    // Indica se il giocatore sta arrampicando
    public bool isClimbing = false;

    // Layer originali
    private int originalLayer;
    public int climbingLayer; // Layer da assegnare durante l'arrampicata

    void Start()
    {
        if (playerRigidbody == null)
        {
            playerRigidbody = GetComponent<Rigidbody>();
        }

        if (playerCollider == null)
        {
            playerCollider = GetComponent<CapsuleCollider>();
        }

        // Memorizza il layer originale del giocatore
        originalLayer = gameObject.layer;
    }

    void FixedUpdate()
    {
        HandleClimbingMovement();
    }

    void HandleClimbingMovement()
    {
        // Input per la presa (puoi cambiare i pulsanti secondo le tue preferenze)
        bool leftGrabInput = OVRInput.Get(OVRInput.Button.PrimaryHandTrigger);
        bool rightGrabInput = OVRInput.Get(OVRInput.Button.SecondaryHandTrigger);

        // Verifica se le mani stanno afferrando
        bool leftHandCanGrab = leftControllerDetector.isTouchingSurface && leftGrabInput;
        bool rightHandCanGrab = rightControllerDetector.isTouchingSurface && rightGrabInput;

        // Gestisci lo stato di grabbing della mano sinistra
        if (leftHandCanGrab && !isLeftHandGrabbing)
        {
            // Inizia a afferrare con la mano sinistra
            isLeftHandGrabbing = true;
            leftHandGrabPoint = leftControllerTransform.position;
        }
        else if (!leftHandCanGrab && isLeftHandGrabbing)
        {
            // Smette di afferrare con la mano sinistra
            isLeftHandGrabbing = false;
        }

        // Gestisci lo stato di grabbing della mano destra
        if (rightHandCanGrab && !isRightHandGrabbing)
        {
            // Inizia a afferrare con la mano destra
            isRightHandGrabbing = true;
            rightHandGrabPoint = rightControllerTransform.position;
        }
        else if (!rightHandCanGrab && isRightHandGrabbing)
        {
            // Smette di afferrare con la mano destra
            isRightHandGrabbing = false;
        }

        // Determina se il giocatore sta arrampicando
        bool wasClimbing = isClimbing;
        isClimbing = isLeftHandGrabbing || isRightHandGrabbing;

        // Cambia il layer del giocatore se lo stato di arrampicata è cambiato
        if (isClimbing && !wasClimbing)
        {
            // Inizia l'arrampicata: cambia il layer
            gameObject.layer = climbingLayer;
        }
        else if (!isClimbing && wasClimbing)
        {
            // Termina l'arrampicata: ripristina il layer originale
            gameObject.layer = originalLayer;
        }

        if (isClimbing)
        {
            Vector3 movement = Vector3.zero;

            if (isLeftHandGrabbing)
            {
                Vector3 leftHandDelta = leftHandGrabPoint - leftControllerTransform.position;
                movement += leftHandDelta;
            }

            if (isRightHandGrabbing)
            {
                Vector3 rightHandDelta = rightHandGrabPoint - rightControllerTransform.position;
                movement += rightHandDelta;
            }

            // Applica il moltiplicatore di movimento
            movement *= movementMultiplier;

            // Limita la velocità massima di arrampicata
            float maxMovementPerFrame = maxClimbSpeed * Time.fixedDeltaTime;
            if (movement.magnitude > maxMovementPerFrame)
            {
                movement = movement.normalized * maxMovementPerFrame;
            }

            // Applica smoothing al movimento
            Vector3 targetPosition = playerRigidbody.position + movement;
            Vector3 smoothedPosition = Vector3.Lerp(playerRigidbody.position, targetPosition, smoothingFactor);

            // Muovi il giocatore
            playerRigidbody.MovePosition(smoothedPosition);
        }
    }
}
