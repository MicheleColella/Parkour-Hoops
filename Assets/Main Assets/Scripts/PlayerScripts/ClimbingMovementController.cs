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

    [Header("Settings")]
    public float movementMultiplier = 1.0f; // Moltiplicatore di velocità di movimento
    public float smoothingFactor = 0.05f;   // Fattore di smoothing per il movimento
    public float maxClimbSpeed = 5.0f;      // Velocità massima di arrampicata

    private Vector3 leftHandGrabPoint;
    private Vector3 rightHandGrabPoint;
    private bool isLeftHandGrabbing = false;
    private bool isRightHandGrabbing = false;

    private Vector3 currentVelocity = Vector3.zero; // Per SmoothDamp

    // Memorizza la posizione precedente del giocatore
    private Vector3 previousPlayerPosition;

    // Aggiunto bool per indicare lo stato di arrampicata
    public bool isClimbing = false;

    void Start()
    {
        if (playerRigidbody == null)
        {
            playerRigidbody = GetComponent<Rigidbody>();
        }

        // Inizializza la posizione precedente del giocatore
        previousPlayerPosition = transform.position;
    }

    void FixedUpdate()
    {
        HandleClimbingMovement();

        // Aggiorna la posizione precedente del giocatore
        previousPlayerPosition = transform.position;
    }

    void HandleClimbingMovement()
    {
        // Verifica se il giocatore è in aria
        bool isPlayerInAir = !GetComponent<ClimbingColliderAdjuster>().IsGrounded();

        // Verifica se uno dei controller sta toccando una superficie scalabile
        bool isLeftControllerTouching = leftControllerDetector.isTouchingSurface;
        bool isRightControllerTouching = rightControllerDetector.isTouchingSurface;

        // Aggiorna lo stato di arrampicata
        isClimbing = isPlayerInAir && (isLeftControllerTouching || isRightControllerTouching);

        // Aggiungi debug per monitorare le condizioni
        Debug.Log($"isPlayerInAir: {isPlayerInAir}, isLeftControllerTouching: {isLeftControllerTouching}, isRightControllerTouching: {isRightControllerTouching}, isClimbing: {isClimbing}");

        // Gestisci lo stato di grabbing della mano sinistra
        if (isLeftControllerTouching && !isLeftHandGrabbing)
        {
            // La mano sinistra inizia a afferrare
            isLeftHandGrabbing = true;
            leftHandGrabPoint = leftControllerTransform.position;

            Debug.Log("La mano sinistra ha iniziato ad afferrare.");
        }
        else if (!isLeftControllerTouching && isLeftHandGrabbing)
        {
            // La mano sinistra smette di afferrare
            isLeftHandGrabbing = false;

            Debug.Log("La mano sinistra ha smesso di afferrare.");
        }

        // Gestisci lo stato di grabbing della mano destra
        if (isRightControllerTouching && !isRightHandGrabbing)
        {
            // La mano destra inizia a afferrare
            isRightHandGrabbing = true;
            rightHandGrabPoint = rightControllerTransform.position;

            Debug.Log("La mano destra ha iniziato ad afferrare.");
        }
        else if (!isRightControllerTouching && isRightHandGrabbing)
        {
            // La mano destra smette di afferrare
            isRightHandGrabbing = false;

            Debug.Log("La mano destra ha smesso di afferrare.");
        }

        // Se il giocatore sta arrampicando
        if (isClimbing)
        {
            Vector3 totalMovement = Vector3.zero;

            // Calcola il movimento del giocatore basato sulla mano sinistra
            if (isLeftHandGrabbing)
            {
                Vector3 leftHandDelta = (leftHandGrabPoint - leftControllerTransform.position) - (transform.position - previousPlayerPosition);
                totalMovement += leftHandDelta;
            }

            // Calcola il movimento del giocatore basato sulla mano destra
            if (isRightHandGrabbing)
            {
                Vector3 rightHandDelta = (rightHandGrabPoint - rightControllerTransform.position) - (transform.position - previousPlayerPosition);
                totalMovement += rightHandDelta;
            }

            // Applica il moltiplicatore di movimento
            totalMovement *= movementMultiplier;

            // Applica smoothing al movimento
            Vector3 smoothedMovement = Vector3.SmoothDamp(Vector3.zero, totalMovement, ref currentVelocity, smoothingFactor);

            // Limita la velocità massima di arrampicata
            smoothedMovement = Vector3.ClampMagnitude(smoothedMovement, maxClimbSpeed * Time.fixedDeltaTime);

            // Muovi il giocatore
            playerRigidbody.MovePosition(playerRigidbody.position + smoothedMovement);

            Debug.Log($"Movimento di arrampicata applicato: {smoothedMovement}");
        }
        else
        {
            // Resetta la velocità corrente per evitare movimenti indesiderati
            currentVelocity = Vector3.zero;
        }
    }
}
