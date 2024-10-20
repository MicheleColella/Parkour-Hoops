using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Unity.XR.CoreUtils;

public class HexaBodyController : MonoBehaviour
{
    [Header("XR Toolkit Components")]
    public XROrigin XROrigin;
    public GameObject XRCamera;

    [Header("Action-Based Controllers")]
    public ActionBasedController HeadController;
    public ActionBasedController RightHandController;
    public ActionBasedController LeftHandController;

    [Header("HexaBody Parts")]
    public GameObject Head;
    public GameObject Chest;
    public GameObject Fender;
    public GameObject Monoball;

    public ConfigurableJoint RightHandJoint;
    public ConfigurableJoint LeftHandJoint;
    public ConfigurableJoint SpineJoint;

    [Header("Movement Parameters")]
    public float walkForce;
    public float angularDragOnMove;
    public float angularBreakDrag;
    public float accelerationMultiplier = 2.0f;
    public float maxVelocityMagnitude = 5.0f;
    public float stoppingForce = 10.0f;
    public float directionChangeForce = 8.0f;

    [Header("Crouch Settings")]
    public float crouchSpeed = 1.0f;
    public float standUpSpeed = 1.0f;
    public float minCrouchHeight;
    public float maxCrouchHeight;

    private Rigidbody monoballRb;
    private Vector3 lastMoveDirection;
    private bool isCrouching = false;
    private bool isStandingUp = false;
    private float additionalHeight;
    private float currentHeight;
    private float targetHeight;

    private Vector2 leftThumbstickInput;
    private Quaternion headYaw;
    private Vector3 moveDirection;
    private Vector3 monoballTorque;

    // Variabili per la rotazione delle mani
    private Quaternion rightHandRotation;
    private Quaternion leftHandRotation;

    private XRControllerInputManager inputManager;

    void Start()
    {
        inputManager = XRControllerInputManager.Instance;
        additionalHeight = (0.5f * Monoball.transform.lossyScale.y) + (0.5f * Fender.transform.lossyScale.y) + (Head.transform.position.y - Chest.transform.position.y);
        monoballRb = Monoball.GetComponent<Rigidbody>();
        lastMoveDirection = Vector3.zero;

        currentHeight = maxCrouchHeight - additionalHeight;
        targetHeight = currentHeight;
    }

    void Update()
    {
        SyncCameraToPlayer();
        SyncXROriginToPlayer();
        ReadControllerInput();
    }

    void FixedUpdate()
    {
        MovePlayer();
        HandleCrouchInput();  // Aggiunto metodo per gestire il crouch con il pulsante destro
        UpdateCrouchHeight();

        if (!isCrouching && !isStandingUp)
        {
            AdjustSpineForCrouch();
        }

        RotatePlayerToHeadDirection();
        MoveAndRotateHands();
    }

    private void ReadControllerInput()
    {
        RightHandJoint.targetPosition = RightHandController.positionAction.action.ReadValue<Vector3>();
        LeftHandJoint.targetPosition = LeftHandController.positionAction.action.ReadValue<Vector3>();

        // Aggiorna le variabili con la rotazione dei controller
        rightHandRotation = RightHandController.rotationAction.action.ReadValue<Quaternion>();
        leftHandRotation = LeftHandController.rotationAction.action.ReadValue<Quaternion>();

        leftThumbstickInput = inputManager.GetLeftThumbstickValue();

        headYaw = Quaternion.Euler(0, XROrigin.Camera.transform.eulerAngles.y, 0);
        moveDirection = headYaw * new Vector3(leftThumbstickInput.x, 0, leftThumbstickInput.y);
        monoballTorque = new Vector3(moveDirection.z, 0, -moveDirection.x);
    }

    private void SyncCameraToPlayer()
    {
        XRCamera.transform.position = Head.transform.position;
    }

    private void SyncXROriginToPlayer()
    {
        XROrigin.transform.position = new Vector3(Fender.transform.position.x,
                                                  Fender.transform.position.y - (0.5f * Fender.transform.localScale.y + 0.5f * Monoball.transform.localScale.y),
                                                  Fender.transform.position.z);
    }

    private void RotatePlayerToHeadDirection()
    {
        Chest.transform.rotation = headYaw;
    }

    private void MovePlayer()
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

        if (monoballRb.velocity.magnitude < 1f)
        {
            adjustedForce *= accelerationMultiplier;
        }

        Vector3 torqueForce = monoballTorque.normalized * adjustedForce;
        monoballRb.AddTorque(torqueForce, ForceMode.Acceleration);

        if (monoballRb.velocity.magnitude > maxVelocityMagnitude)
        {
            monoballRb.velocity = monoballRb.velocity.normalized * maxVelocityMagnitude;
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

    private void HandleCrouchInput()
    {
        if (inputManager.GetRightPrimaryButton())
        {
            isCrouching = true;
            targetHeight = minCrouchHeight;  // Riduci l'altezza al valore del crouch
        }
        else if (!inputManager.GetRightPrimaryButton() && isCrouching)
        {
            isCrouching = false;
            isStandingUp = true;
            targetHeight = maxCrouchHeight - additionalHeight;  // Ritorna all'altezza normale
        }
    }

    private void UpdateCrouchHeight()
    {
        if (isCrouching || isStandingUp)
        {
            float previousHeight = currentHeight;

            if (currentHeight < targetHeight)
            {
                currentHeight += standUpSpeed * Time.fixedDeltaTime;
                if (currentHeight > targetHeight)
                {
                    currentHeight = targetHeight;
                    if (isStandingUp) isStandingUp = false;
                }
            }
            else if (currentHeight > targetHeight)
            {
                currentHeight -= crouchSpeed * Time.fixedDeltaTime;
                if (currentHeight < targetHeight)
                {
                    currentHeight = targetHeight;
                }
            }

            if (previousHeight != currentHeight)
            {
                SpineJoint.targetPosition = new Vector3(0, currentHeight, 0);
            }
        }
    }

    private void AdjustSpineForCrouch()
    {
        if (!isCrouching && !isStandingUp)
        {
            currentHeight = Mathf.Clamp(HeadController.positionAction.action.ReadValue<Vector3>().y - additionalHeight, minCrouchHeight, maxCrouchHeight - additionalHeight);
            SpineJoint.targetPosition = new Vector3(0, currentHeight, 0);
        }
    }

    private void MoveAndRotateHands()
    {
        RightHandJoint.targetPosition = RightHandController.positionAction.action.ReadValue<Vector3>() - HeadController.positionAction.action.ReadValue<Vector3>();
        LeftHandJoint.targetPosition = LeftHandController.positionAction.action.ReadValue<Vector3>() - HeadController.positionAction.action.ReadValue<Vector3>();

        RightHandJoint.targetRotation = rightHandRotation;
        LeftHandJoint.targetRotation = leftHandRotation;
    }
}
