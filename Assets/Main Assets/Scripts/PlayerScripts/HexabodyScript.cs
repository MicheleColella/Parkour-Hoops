using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class HexaBodyScript : MonoBehaviour
{
    [Header("OVR Camera Rig Parts")]
    public GameObject OVRCamera;       // Telecamera principale collegata alla testa del giocatore
    public GameObject OVRRig;          // Rig del giocatore

    [Header("OVR Controllers")]
    public Transform RightHandController;  // RightHandAnchor
    public Transform LeftHandController;   // LeftHandAnchor

    [Header("Hexabody Parts")]
    public GameObject Head;    // Parte fisica della testa
    public GameObject Chest;   // Parte fisica del petto
    public GameObject Fender;  // Parte fisica del fender
    public GameObject Monoball;  // Corpo fisico del giocatore

    public ConfigurableJoint RightHandJoint;  // Giunto per la mano destra fisica
    public ConfigurableJoint LeftHandJoint;   // Giunto per la mano sinistra fisica
    public ConfigurableJoint Spine;           // Giunto per la colonna vertebrale fisica

    [Header("Hexabody Movement Speed")]
    public float moveForceCrouch;
    public float moveForceWalk;
    public float moveForceSprint;

    [Header("Hexabody Drag")]
    public float angularDragOnMove;
    public float angularBreakDrag;

    [Header("Hexabody Crouch & Jump")]
    private bool jumping = false;
    public float crouchSpeed;
    public float lowestCrouch;
    public float highestCrouch;
    private float additionalHeight;

    private Vector3 crouchTarget;

    // Input-related variables
    private Quaternion headYaw;
    private Vector3 moveDirection;
    private Vector3 monoballTorque;

    private Vector3 rightHandControllerPos;
    private Vector3 leftHandControllerPos;

    private Quaternion rightHandControllerRotation;
    private Quaternion leftHandControllerRotation;

    private Vector2 leftThumbstick;  // Usato per il movimento tramite lo stick sinistro

    void Start()
    {
        // Calcoliamo l'altezza aggiuntiva basata sulla scala della Monoball e del Fender
        additionalHeight = (0.5f * Monoball.transform.lossyScale.y) + (0.5f * Fender.transform.lossyScale.y) + (Head.transform.position.y - Chest.transform.position.y);
    }

    void Update()
    {
        TrackCameraPosition();
        GetControllerInputValues();
        SyncHexaBodyWithRig();
    }

    private void FixedUpdate()
    {
        MovePlayerViaController();
        HandleJumping();

        if (!jumping)
        {
            AdjustSpineBasedOnCrouch();
        }

        MoveAndRotateHands();
    }

    private void GetControllerInputValues()
    {
        // Posizione e rotazione del controller destro
        rightHandControllerPos = RightHandController.position;
        rightHandControllerRotation = RightHandController.rotation;

        // Posizione e rotazione del controller sinistro
        leftHandControllerPos = LeftHandController.position;
        leftHandControllerRotation = LeftHandController.rotation;

        // Thumbstick sinistro usato per il movimento
        leftThumbstick = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, OVRInput.Controller.LTouch);

        // Calcoliamo la direzione del movimento basata sull'orientamento della testa
        headYaw = Quaternion.Euler(0, OVRCamera.transform.eulerAngles.y, 0);
        moveDirection = headYaw * new Vector3(leftThumbstick.x, 0, leftThumbstick.y);
        monoballTorque = new Vector3(moveDirection.z, 0, -moveDirection.x);
    }

    //------Tracking della testa e sincronizzazione del corpo fisico---------------------------------------------------------------------------------------
    private void TrackCameraPosition()
    {
        // Manteniamo la telecamera della scena sincronizzata con la posizione della testa del giocatore
        OVRCamera.transform.position = Head.transform.position;
    }

    private void SyncHexaBodyWithRig()
    {
        // Il corpo fisico (HexaBody) deve seguire la posizione del Camera Rig
        // Sposta il Monoball (corpo fisico) alla posizione del Camera Rig
        Monoball.transform.position = OVRRig.transform.position;

        // Sincronizza la testa fisica con la telecamera (CenterEyeAnchor)
        Head.transform.position = OVRCamera.transform.position;
        Head.transform.rotation = OVRCamera.transform.rotation;

        // Sincronizza il petto fisico con la rotazione della testa
        Chest.transform.rotation = Quaternion.Euler(0, OVRCamera.transform.eulerAngles.y, 0);

        // Sincronizza le mani fisiche con i controller
        RightHandJoint.targetPosition = RightHandController.position - OVRRig.transform.position;
        RightHandJoint.targetRotation = RightHandController.rotation;

        LeftHandJoint.targetPosition = LeftHandController.position - OVRRig.transform.position;
        LeftHandJoint.targetRotation = LeftHandController.rotation;
    }

    //-----Movimento del corpo Hexa (Monoball)---------------------------------------------------------------------------------
    private void MovePlayerViaController()
    {
        if (!jumping)
        {
            // Se il thumbstick non viene toccato, fermiamo il movimento
            if (leftThumbstick == Vector2.zero)
            {
                StopMonoball();
            }
            else
            {
                // Se il thumbstick sinistro viene toccato, spostiamo la Monoball
                float force = OVRInput.Get(OVRInput.Button.PrimaryThumbstick, OVRInput.Controller.LTouch) ? moveForceSprint : moveForceWalk;
                MoveMonoball(force);
            }
        }
        else
        {
            // Mentre si è in salto, applichiamo la forza di movimento di crouch
            if (leftThumbstick == Vector2.zero)
            {
                StopMonoball();
            }
            else
            {
                MoveMonoball(moveForceCrouch);
            }
        }
    }

    private void MoveMonoball(float force)
    {
        // Applichiamo la rotazione alla Monoball usando la forza calcolata
        Rigidbody rb = Monoball.GetComponent<Rigidbody>();
        rb.freezeRotation = false;
        rb.angularDrag = angularDragOnMove;
        rb.AddTorque(monoballTorque.normalized * force, ForceMode.Force);
    }

    private void StopMonoball()
    {
        // Fermiamo la Monoball
        Rigidbody rb = Monoball.GetComponent<Rigidbody>();
        rb.angularDrag = angularBreakDrag;

        if (rb.velocity == Vector3.zero)
        {
            rb.freezeRotation = true;
        }
    }

    //------Gestione del salto------------------------------------------------------------------------------------------
    private void HandleJumping()
    {
        bool jumpButtonPressed = OVRInput.Get(OVRInput.Button.One, OVRInput.Controller.LTouch);

        if (jumpButtonPressed && leftThumbstick.y < 0)
        {
            jumping = true;
            JumpSitDown();
        }
        else if (!jumpButtonPressed && jumping)
        {
            jumping = false;
            JumpSitUp();
        }
    }

    private void JumpSitDown()
    {
        if (crouchTarget.y >= lowestCrouch)
        {
            crouchTarget.y -= crouchSpeed * Time.fixedDeltaTime;
            Spine.targetPosition = new Vector3(0, crouchTarget.y, 0);
        }
    }

    private void JumpSitUp()
    {
        crouchTarget = new Vector3(0, highestCrouch - additionalHeight, 0);
        Spine.targetPosition = crouchTarget;
    }

    //------Controllo delle articolazioni-----------------------------------------------------------------------------------
    private void AdjustSpineBasedOnCrouch()
    {
        // La posizione della spina dorsale viene regolata in base alla posizione della telecamera
        crouchTarget.y = Mathf.Clamp(OVRCamera.transform.position.y - additionalHeight, lowestCrouch, highestCrouch - additionalHeight);
        Spine.targetPosition = new Vector3(0, crouchTarget.y, 0);
    }

    private void MoveAndRotateHands()
    {
        // Muoviamo e ruotiamo le mani in base alla posizione dei controller
        RightHandJoint.targetPosition = rightHandControllerPos - OVRCamera.transform.position;
        LeftHandJoint.targetPosition = leftHandControllerPos - OVRCamera.transform.position;

        RightHandJoint.targetRotation = rightHandControllerRotation;
        LeftHandJoint.targetRotation = leftHandControllerRotation;
    }
}
