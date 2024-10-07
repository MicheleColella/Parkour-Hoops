using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicRig : MonoBehaviour
{
    public Transform playerHead;
    public Transform leftController;
    public Transform rightController;

    public ConfigurableJoint headJoint;
    public ConfigurableJoint leftHandJoint;
    public ConfigurableJoint rightHandJoint;

    public CapsuleCollider bodyCollider;

    public float bodyHeightMin = 0.5f;
    public float bodyHeightMax = 2;

    private void Update()
    {
        // Aggiorna la posizione del collider del corpo in base alla posizione della testa
        bodyCollider.height = Mathf.Clamp(playerHead.localPosition.y, bodyHeightMin, bodyHeightMax);
        bodyCollider.center = new Vector3(playerHead.localPosition.x, bodyCollider.height / 2, playerHead.localPosition.z);

        // Aggiorna i giunti delle mani e della testa in base alla posizione dei controller
        leftHandJoint.targetPosition = OVRInput.GetLocalControllerPosition(OVRInput.Controller.LTouch);
        leftHandJoint.targetRotation = OVRInput.GetLocalControllerRotation(OVRInput.Controller.LTouch);

        rightHandJoint.targetPosition = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch);
        rightHandJoint.targetRotation = OVRInput.GetLocalControllerRotation(OVRInput.Controller.RTouch);

        headJoint.targetPosition = playerHead.localPosition;
    }
}
