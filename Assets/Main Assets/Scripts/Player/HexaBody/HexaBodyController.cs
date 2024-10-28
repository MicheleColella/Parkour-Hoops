using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Unity.XR.CoreUtils;

public class HexaBodyController : MonoBehaviour
{
    [Header("XR Toolkit Components")]
    public XROrigin xrOrigin;
    public GameObject xrCamera;

    [Header("Action-Based Controllers")]
    public ActionBasedController headController;
    public ActionBasedController rightHandController;
    public ActionBasedController leftHandController;

    [Header("HexaBody Parts")]
    public GameObject head;
    public GameObject chest;
    public GameObject fender;
    public GameObject monoball;

    [Header("Joints")]
    public ConfigurableJoint rightHandJoint;
    public ConfigurableJoint leftHandJoint;
    public ConfigurableJoint spineJoint;

    [Header("Movement Parameters")]
    [Tooltip("Forza applicata per il movimento della monoball")]
    public float walkForce = 700f;

    [Tooltip("Resistenza alla rotazione durante il movimento")]
    public float angularDragOnMove = 5f;

    [Tooltip("Resistenza alla rotazione quando la monoball è ferma")]
    public float angularBreakDrag = 10f;

    [Tooltip("Moltiplicatore di accelerazione per avviare il movimento più rapidamente")]
    public float accelerationMultiplier = 2.0f;

    [Tooltip("Velocità massima consentita per la monoball")]
    public float maxVelocityMagnitude = 5.0f;

    [Tooltip("Forza applicata per fermare la monoball")]
    public float stoppingForce = 2f;

    [Tooltip("Forza applicata per cambiare rapidamente direzione")]
    public float directionChangeForce = 8.0f;

    [Header("Crouch Settings")]
    public float crouchSpeed = 5.0f;
    public float standUpSpeed = 5.0f;
    public float minCrouchHeight = 1.0f;
    public float maxCrouchHeight = 2.0f;

    [Header("Scale Settings")]
    public Vector3 defaultMonoballScale = new Vector3(1, 1, 1);
    public Vector3 airMonoballScale = new Vector3(1.2f, 1.2f, 1.2f);

    public Vector3 defaultFenderScale = new Vector3(1, 1, 1);
    public Vector3 airFenderScale = new Vector3(1.1f, 1.1f, 1.1f);

    public Vector3 defaultChestScale = new Vector3(1, 1, 1);
    public Vector3 airChestScale = new Vector3(1.15f, 1.15f, 1.15f);

    [Header("Scale Adjustment Settings")]
    [Tooltip("Velocità di interpolazione per il cambio di scala.")]
    public float scaleChangeSpeed = 5f;

    [Header("References")]
    public JumpController jumpController; // Riferimento al JumpController

    [Tooltip("Velocità attuale della monoball")]
    public float monoballVelocity;

    private Rigidbody monoballRb;
    private Vector3 lastMoveDirection = Vector3.zero;
    private float additionalHeight;
    private float currentHeight;

    private Vector2 leftThumbstickInput;
    private Quaternion headYaw;
    private Vector3 moveDirection;
    private Vector3 monoballTorque;

    private Quaternion rightHandRotation;
    private Quaternion leftHandRotation;

    private XRControllerInputManager inputManager;

    void Start()
    {
        inputManager = XRControllerInputManager.Instance;

        if (monoball == null)
        {
            Debug.LogError("Monoball GameObject is not assigned.");
            return;
        }

        monoballRb = monoball.GetComponent<Rigidbody>();
        if (monoballRb == null)
        {
            Debug.LogError("Monoball does not have a Rigidbody component.");
            return;
        }

        additionalHeight = (0.5f * monoball.transform.lossyScale.y) +
                           (0.5f * fender.transform.lossyScale.y) +
                           (head.transform.position.y - chest.transform.position.y);

        currentHeight = maxCrouchHeight - additionalHeight;

        // Imposta le scale di default
        if (monoball != null)
            monoball.transform.localScale = defaultMonoballScale;

        if (fender != null)
            fender.transform.localScale = defaultFenderScale;

        if (chest != null)
            chest.transform.localScale = defaultChestScale;

        // Verifica il riferimento a JumpController
        if (jumpController == null)
        {
            jumpController = GetComponent<JumpController>();
            if (jumpController == null)
            {
                Debug.LogError("JumpController non assegnato e non trovato sullo stesso GameObject.");
            }
        }
    }

    void Update()
    {
        SyncCameraToPlayer();
        SyncXROriginToPlayer();
        ReadControllerInput();
        AdjustScaleBasedOnGrounded();

        // Aggiorna la velocità della monoball per il monitoraggio
        monoballVelocity = monoballRb.velocity.magnitude;
    }

    void FixedUpdate()
    {
        HandleMovement();
        RotatePlayerToHeadDirection();
        MoveAndRotateHands();
        AdjustSpineHeight();
    }

    private void ReadControllerInput()
    {
        // Leggi posizioni dei controller
        Vector3 rightHandPosition = rightHandController.positionAction.action.ReadValue<Vector3>();
        Vector3 leftHandPosition = leftHandController.positionAction.action.ReadValue<Vector3>();

        // Aggiorna le posizioni target dei joint
        rightHandJoint.targetPosition = rightHandPosition;
        leftHandJoint.targetPosition = leftHandPosition;

        // Leggi rotazioni dei controller
        rightHandRotation = rightHandController.rotationAction.action.ReadValue<Quaternion>();
        leftHandRotation = leftHandController.rotationAction.action.ReadValue<Quaternion>();

        // Leggi input del thumbstick
        leftThumbstickInput = inputManager.GetLeftThumbstickValue();

        // Calcola yaw della testa
        headYaw = Quaternion.Euler(0, xrOrigin.Camera.transform.eulerAngles.y, 0);

        // Determina direzione di movimento
        moveDirection = headYaw * new Vector3(leftThumbstickInput.x, 0, leftThumbstickInput.y);
        monoballTorque = new Vector3(moveDirection.z, 0, -moveDirection.x);
    }

    private void SyncCameraToPlayer()
    {
        if (head != null && xrCamera != null)
        {
            xrCamera.transform.position = head.transform.position;
        }
    }

    private void SyncXROriginToPlayer()
    {
        if (fender != null && monoball != null && xrOrigin != null)
        {
            Vector3 newOriginPosition = new Vector3(
                fender.transform.position.x,
                fender.transform.position.y - (0.5f * fender.transform.localScale.y + 0.5f * monoball.transform.localScale.y),
                fender.transform.position.z
            );
            xrOrigin.transform.position = newOriginPosition;
        }
    }

    private void RotatePlayerToHeadDirection()
    {
        if (chest != null)
        {
            chest.transform.rotation = headYaw;
        }
    }

    private void HandleMovement()
    {
        if (leftThumbstickInput == Vector2.zero)
        {
            StopMonoball();
        }
        else
        {
            MoveMonoball(walkForce);
        }
    }

    private void MoveMonoball(float force)
    {
        if (monoballRb == null) return;

        monoballRb.freezeRotation = false;
        monoballRb.angularDrag = angularDragOnMove;

        float directionChange = Vector3.Angle(moveDirection, lastMoveDirection);
        float adjustedForce = force;

        if (directionChange > 30f && lastMoveDirection != Vector3.zero)
        {
            adjustedForce *= (1 + directionChangeForce * (directionChange / 180f));
            Vector3 counterForce = -monoballRb.velocity * directionChangeForce;
            monoballRb.AddForce(counterForce, ForceMode.Acceleration);
        }

        if (new Vector2(monoballRb.velocity.x, monoballRb.velocity.z).magnitude < 1f)
        {
            adjustedForce *= accelerationMultiplier;
        }

        Vector3 torqueForce = monoballTorque.normalized * adjustedForce;
        monoballRb.AddTorque(torqueForce, ForceMode.Acceleration);

        // Limita la velocità solo sugli assi x e z
        Vector3 horizontalVelocity = new Vector3(monoballRb.velocity.x, 0, monoballRb.velocity.z);
        if (horizontalVelocity.magnitude > maxVelocityMagnitude)
        {
            horizontalVelocity = horizontalVelocity.normalized * maxVelocityMagnitude;
            monoballRb.velocity = new Vector3(horizontalVelocity.x, monoballRb.velocity.y, horizontalVelocity.z);
        }

        lastMoveDirection = moveDirection;
    }


    private void StopMonoball()
    {
        if (monoballRb == null) return;

        Vector3 currentVelocity = monoballRb.velocity;
        if (currentVelocity.magnitude > 0.1f)
        {
            Vector3 stopForce = -currentVelocity.normalized * stoppingForce;
            monoballRb.AddForce(stopForce, ForceMode.Acceleration);
        }
        else
        {
            monoballRb.velocity = Vector3.zero;
            monoballRb.angularVelocity = Vector3.zero;
            monoballRb.freezeRotation = true;
        }

        monoballRb.angularDrag = angularBreakDrag;
        lastMoveDirection = Vector3.zero;
    }

    private void AdjustSpineHeight()
    {
        if (headController == null) return;

        float headHeight = headController.positionAction.action.ReadValue<Vector3>().y - additionalHeight;
        float desiredHeight;

        if (inputManager.GetRightSecondaryButton())
        {
            desiredHeight = minCrouchHeight;
        }
        else
        {
            desiredHeight = Mathf.Clamp(
                headHeight,
                minCrouchHeight,
                maxCrouchHeight - additionalHeight
            );
        }

        currentHeight = Mathf.Lerp(currentHeight, desiredHeight, Time.fixedDeltaTime * standUpSpeed);
        spineJoint.targetPosition = new Vector3(0, currentHeight, 0);
    }

    private void MoveAndRotateHands()
    {
        if (rightHandJoint != null && leftHandJoint != null && headController != null)
        {
            Vector3 headPosition = headController.positionAction.action.ReadValue<Vector3>();

            rightHandJoint.targetPosition = rightHandController.positionAction.action.ReadValue<Vector3>() - headPosition;
            leftHandJoint.targetPosition = leftHandController.positionAction.action.ReadValue<Vector3>() - headPosition;

            rightHandJoint.targetRotation = rightHandRotation;
            leftHandJoint.targetRotation = leftHandRotation;
        }
    }

    private void AdjustScaleBasedOnGrounded()
    {
        if (jumpController == null)
            return;

        if (jumpController.IsGrounded)
        {
            // Imposta le scale di default
            if (monoball != null)
                monoball.transform.localScale = Vector3.Lerp(monoball.transform.localScale, defaultMonoballScale, Time.deltaTime * scaleChangeSpeed);

            if (fender != null)
                fender.transform.localScale = Vector3.Lerp(fender.transform.localScale, defaultFenderScale, Time.deltaTime * scaleChangeSpeed);

            if (chest != null)
                chest.transform.localScale = Vector3.Lerp(chest.transform.localScale, defaultChestScale, Time.deltaTime * scaleChangeSpeed);
        }
        else
        {
            // Imposta le scale in aria
            if (monoball != null)
                monoball.transform.localScale = Vector3.Lerp(monoball.transform.localScale, airMonoballScale, Time.deltaTime * scaleChangeSpeed);

            if (fender != null)
                fender.transform.localScale = Vector3.Lerp(fender.transform.localScale, airFenderScale, Time.deltaTime * scaleChangeSpeed);

            if (chest != null)
                chest.transform.localScale = Vector3.Lerp(chest.transform.localScale, airChestScale, Time.deltaTime * scaleChangeSpeed);
        }
    }
}
