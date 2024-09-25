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
    public float movementMultiplier = 1.0f; // Moltiplicatore per la velocità di spostamento del player

    private Vector3 leftControllerPrevLocalPos;
    private Vector3 rightControllerPrevLocalPos;

    void Start()
    {
        if (playerRigidbody == null)
        {
            playerRigidbody = GetComponent<Rigidbody>();
        }

        // Inizializza le posizioni precedenti dei controller
        leftControllerPrevLocalPos = leftControllerTransform.localPosition;
        rightControllerPrevLocalPos = rightControllerTransform.localPosition;
    }

    void FixedUpdate()
    {
        HandleClimbingMovement();
    }

    void HandleClimbingMovement()
    {
        // Verifica se il player è in aria
        bool isPlayerInAir = !GetComponent<MovementController>().IsGrounded();

        // Verifica se uno dei controller sta toccando una superficie
        bool isLeftControllerTouching = leftControllerDetector.isTouchingSurface;
        bool isRightControllerTouching = rightControllerDetector.isTouchingSurface;

        // Se il player è in aria e almeno un controller sta toccando una superficie
        if (isPlayerInAir && (isLeftControllerTouching || isRightControllerTouching))
        {
            Vector3 totalMovement = Vector3.zero;

            if (isLeftControllerTouching)
            {
                Vector3 leftControllerDelta = leftControllerPrevLocalPos - leftControllerTransform.localPosition;
                totalMovement += leftControllerDelta;
            }

            if (isRightControllerTouching)
            {
                Vector3 rightControllerDelta = rightControllerPrevLocalPos - rightControllerTransform.localPosition;
                totalMovement += rightControllerDelta;
            }

            // Trasforma il movimento locale in movimento nel mondo
            Vector3 worldMovement = transform.TransformDirection(totalMovement) * movementMultiplier;

            // Ignora il movimento sull'asse Y (verticale)
            worldMovement.y = 0;

            // Sposta il player nella direzione calcolata
            playerRigidbody.MovePosition(playerRigidbody.position + worldMovement);

            // Nota: Non stiamo modificando l'asse Y poiché le interazioni verticali sono gestite dai colliders
        }

        // Aggiorna le posizioni precedenti dei controller
        leftControllerPrevLocalPos = leftControllerTransform.localPosition;
        rightControllerPrevLocalPos = rightControllerTransform.localPosition;
    }
}
