using UnityEngine;

public class ClimbingController : MonoBehaviour
{
    public Rigidbody playerRigidbody;
    public HandPhysics leftHandPhysics;
    public HandPhysics rightHandPhysics;
    public MovementController movementController;
    public JumpController jumpController;
    public float climbingSpeedMultiplier = 1f;

    private bool isClimbing = false;
    private Vector3 leftHandPrevControllerLocalPos;
    private Vector3 rightHandPrevControllerLocalPos;

    private void Start()
    {
        if (playerRigidbody == null)
        {
            playerRigidbody = GetComponent<Rigidbody>();
        }
    }

    void FixedUpdate()
    {
        bool leftGrabbing = leftHandPhysics.isGrabbing;
        bool rightGrabbing = rightHandPhysics.isGrabbing;

        if (leftGrabbing || rightGrabbing)
        {
            if (!isClimbing)
            {
                // Entra nello stato di arrampicata
                isClimbing = true;
                // Disabilita movimento e salto
                movementController.enabled = false;
                jumpController.enabled = false;
                // Disabilita la gravità
                playerRigidbody.useGravity = false;
                // Azzera la velocità del player
                playerRigidbody.velocity = Vector3.zero;

                // Inizializza le posizioni precedenti dei controller
                leftHandPrevControllerLocalPos = playerRigidbody.transform.InverseTransformPoint(leftHandPhysics.ControllerTransform.position);
                rightHandPrevControllerLocalPos = playerRigidbody.transform.InverseTransformPoint(rightHandPhysics.ControllerTransform.position);
            }

            Vector3 climbingMovement = Vector3.zero;

            if (leftGrabbing)
            {
                climbingMovement += CalculateClimbingMovement(leftHandPhysics, ref leftHandPrevControllerLocalPos);
            }
            if (rightGrabbing)
            {
                climbingMovement += CalculateClimbingMovement(rightHandPhysics, ref rightHandPrevControllerLocalPos);
            }

            // Applica il movimento al player
            playerRigidbody.MovePosition(playerRigidbody.position - climbingMovement * climbingSpeedMultiplier);
        }
        else
        {
            if (isClimbing)
            {
                // Esce dallo stato di arrampicata
                isClimbing = false;
                // Riabilita movimento e salto
                movementController.enabled = true;
                jumpController.enabled = true;
                // Riabilita la gravità
                playerRigidbody.useGravity = true;
                // Azzera la velocità residua
                playerRigidbody.velocity = Vector3.zero;
            }
        }

        Debug.Log("isClimbing: " + isClimbing);
        Debug.Log("useGravity: " + playerRigidbody.useGravity);
        Debug.Log("Player Velocity: " + playerRigidbody.velocity);
    }

    private Vector3 CalculateClimbingMovement(HandPhysics handPhysics, ref Vector3 handPrevControllerLocalPos)
    {
        // Ottiene la posizione locale corrente del controller rispetto al player
        Vector3 currentControllerLocalPos = playerRigidbody.transform.InverseTransformPoint(handPhysics.ControllerTransform.position);
        // Calcola il delta di movimento tra il frame precedente e quello corrente
        Vector3 handDelta = currentControllerLocalPos - handPrevControllerLocalPos;
        // Aggiorna la posizione precedente per il prossimo frame
        handPrevControllerLocalPos = currentControllerLocalPos;
        return handDelta;
    }
}
