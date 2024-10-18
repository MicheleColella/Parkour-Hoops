using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEditor.XR.LegacyInputHelpers;
using UnityEngine.XR;
using UnityEngine.InputSystem;
using Unity.XR.CoreUtils;


public class HexaBodyScript : MonoBehaviour
{
    [Header("XR Toolkit Parts")]
    public XROrigin XROrigin;  // Sostituisci XRRig con XROrigin
    public GameObject XRCamera;  // Puoi mantenere la XRCamera come GameObject separato


    [Header("Actionbased Controller")]
    public ActionBasedController CameraController;
    public ActionBasedController RightHandController;
    public ActionBasedController LeftHandController;

    [Header("Hexabody Parts")]
    public GameObject Head;
    public GameObject Chest;
    public GameObject Fender;
    public GameObject Monoball;

    public ConfigurableJoint RightHandJoint;
    public ConfigurableJoint LeftHandJoint;
    public ConfigurableJoint Spine;

    [Header("Hexabody Movespeed")]
    public float moveForceCrouch;
    public float moveForceWalk;
    public float moveForceSprint;

    [Header("Hexabody Drag")]
    public float angularDragOnMove;
    public float angularBreakDrag;

    [Header("Movement Configuration")]
    public float accelerationMultiplier = 2.0f; // Moltiplicatore per l'accelerazione iniziale
    public float maxVelocityMagnitude = 5.0f;   // Velocità massima consentita
    public float stoppingForce = 10.0f;         // Forza di arresto quando non c'è input
    public float directionChangeForce = 8.0f;   // Forza extra applicata durante i cambi di direzione

    private Vector3 lastMoveDirection;
    private Rigidbody monoballRb;

    [Header("Hexabody Croch & Jump")]
    bool jumping = false;

    public float crouchSpeed;
    public float lowesCrouch;
    public float highestCrouch;
    private float additionalHight;

    Vector3 CrouchTarget;

    //---------Input Values---------------------------------------------------------------------------------------------------------------//

    private Quaternion headYaw;
    private Vector3 moveDirection;
    private Vector3 monoballTorque;

    private Vector3 CameraControllerPos;

    private Vector3 RightHandControllerPos;
    private Vector3 LeftHandControllerPos;

    private Quaternion RightHandControllerRotation;
    private Quaternion LeftHandControllerRotation;

    private Vector2 LeftTrackpad;


    private XRControllerInputManager inputManager;


    void Start()
    {
        inputManager = XRControllerInputManager.Instance;
        additionalHight = (0.5f * Monoball.transform.lossyScale.y) + (0.5f * Fender.transform.lossyScale.y) + (Head.transform.position.y - Chest.transform.position.y);
        monoballRb = Monoball.GetComponent<Rigidbody>();
        lastMoveDirection = Vector3.zero;
    }


    void Update()
    {
        CameraToPlayer();
        XROriginToPlayer();

        getContollerInputValues();
    }

    private void FixedUpdate() 
    {
        movePlayerViaController();
        jump();

        if (!jumping)
        {
            spineContractionOnRealWorldCrouch();
        }

        rotatePlayer();
        moveAndRotateHand();
    }

    private void getContollerInputValues()
    {
        // Right Controller: Position & Rotation
        RightHandControllerPos = RightHandController.positionAction.action.ReadValue<Vector3>();
        RightHandControllerRotation = RightHandController.rotationAction.action.ReadValue<Quaternion>();

        // Trackpad (Stick Sinistro per movimento)
        LeftTrackpad = inputManager.GetLeftThumbstickValue();  // Prendi i valori dello stick sinistro
        //Debug.Log("Left Thumbstick X: " + LeftTrackpad.x + ", Y: " + LeftTrackpad.y);  // Monitorare l'input del thumbstick sinistro

        // Left Controller: Position & Rotation
        LeftHandControllerPos = LeftHandController.positionAction.action.ReadValue<Vector3>();
        LeftHandControllerRotation = LeftHandController.rotationAction.action.ReadValue<Quaternion>();

        // Camera Inputs
        CameraControllerPos = CameraController.positionAction.action.ReadValue<Vector3>();

        // Calcolo della direzione del movimento basato sull'orientamento della testa
        headYaw = Quaternion.Euler(0, XROrigin.Camera.transform.eulerAngles.y, 0);

        // Movimento in tutte le direzioni: avanti/indietro (Y) e destra/sinistra (X)
        moveDirection = headYaw * new Vector3(LeftTrackpad.x, 0, LeftTrackpad.y);

        // Torque per il monoball (movimento)
        monoballTorque = new Vector3(moveDirection.z, 0, -moveDirection.x);
    }



    //------Transforms---------------------------------------------------------------------------------------
    private void CameraToPlayer()
    {
        XRCamera.transform.position = Head.transform.position;
    }
    private void XROriginToPlayer()  // Cambiato il nome del metodo da XRRigToPlayer
    {
        XROrigin.transform.position = new Vector3(Fender.transform.position.x,
                                                  Fender.transform.position.y - (0.5f * Fender.transform.localScale.y + 0.5f * Monoball.transform.localScale.y),
                                                  Fender.transform.position.z);
    }

    private void rotatePlayer()
    {
        Chest.transform.rotation = headYaw;
    }
    //-----HexaBody Movement---------------------------------------------------------------------------------
    private void movePlayerViaController()
    {
        if (!jumping)
        {
            if (LeftTrackpad == Vector2.zero)  // Se lo stick sinistro non viene mosso, ferma il movimento
            {
                stopMonoball();
            }
            else if (!inputManager.GetLeftPrimaryButton())  // Movimento normale (camminata)
            {
                moveMonoball(moveForceWalk);
            }
            else if (inputManager.GetLeftPrimaryButton())  // Corsa con primary button sinistro
            {
                moveMonoball(moveForceSprint);
            }
        }
        else if (jumping)
        {
            if (LeftTrackpad == Vector2.zero)
            {
                stopMonoball();
            }
            else
            {
                moveMonoball(moveForceCrouch);
            }
        }
    }


    private void moveMonoball(float force)
    {
        if (monoballRb == null) return;

        monoballRb.freezeRotation = false;
        monoballRb.angularDrag = angularDragOnMove;

        // Calcola l'angolo tra la direzione attuale e quella precedente
        float directionChange = Vector3.Angle(moveDirection, lastMoveDirection);

        // Applica una forza extra se c'è un cambio di direzione significativo
        float finalForce = force;
        if (directionChange > 30f && lastMoveDirection != Vector3.zero)
        {
            finalForce *= (1 + directionChangeForce * (directionChange / 180f));

            // Applica una forza contraria alla velocità corrente per facilitare il cambio di direzione
            Vector3 counterForce = -monoballRb.velocity * directionChangeForce;
            monoballRb.AddForce(counterForce, ForceMode.Acceleration);
        }

        // Applica un boost all'accelerazione iniziale
        if (monoballRb.velocity.magnitude < 1f)
        {
            finalForce *= accelerationMultiplier;
        }

        // Applica il torque con la forza calcolata
        Vector3 torqueForce = monoballTorque.normalized * finalForce;
        monoballRb.AddTorque(torqueForce, ForceMode.Acceleration);

        // Limita la velocità massima
        if (monoballRb.velocity.magnitude > maxVelocityMagnitude)
        {
            monoballRb.velocity = monoballRb.velocity.normalized * maxVelocityMagnitude;
        }

        lastMoveDirection = moveDirection;
    }

    private void stopMonoball()
    {
        if (monoballRb == null) return;

        // Applica una forza di arresto proporzionale alla velocità corrente
        Vector3 currentVel = monoballRb.velocity;
        if (currentVel.magnitude > 0.1f)
        {
            Vector3 stopForce = -currentVel.normalized * stoppingForce;
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

    //------Jumping------------------------------------------------------------------------------------------
    private void jump()
    {
        if (inputManager.GetRightPrimaryButton())  // Crouch con primary button destro
        {
            jumping = true;
            jumpSitDown();  // Mantiene la posizione accovacciata mentre il pulsante è premuto
        }
        else if (!inputManager.GetRightPrimaryButton() && jumping == true)
        {
            jumping = false;
            jumpSitUp();  // Torna alla posizione in piedi quando il pulsante viene rilasciato
        }
    }

    private void jumpSitDown()
    {
        if (CrouchTarget.y >= lowesCrouch)
        {
            CrouchTarget.y -= crouchSpeed * Time.fixedDeltaTime;
            Spine.targetPosition = new Vector3(0, CrouchTarget.y, 0);
        }
    }

    private void jumpSitUp()
    {
        CrouchTarget = new Vector3(0, highestCrouch - additionalHight, 0);
        Spine.targetPosition = CrouchTarget;
    }


    //------Joint Controll-----------------------------------------------------------------------------------
    private void spineContractionOnRealWorldCrouch()
    {
        CrouchTarget.y = Mathf.Clamp(CameraControllerPos.y - additionalHight, lowesCrouch, highestCrouch - additionalHight);
        Spine.targetPosition = new Vector3(0, CrouchTarget.y, 0);

    }
    private void moveAndRotateHand()
    {
        RightHandJoint.targetPosition = RightHandControllerPos - CameraControllerPos;
        LeftHandJoint.targetPosition = LeftHandControllerPos - CameraControllerPos;

        RightHandJoint.targetRotation = RightHandControllerRotation;
        LeftHandJoint.targetRotation = LeftHandControllerRotation;
    }
}
