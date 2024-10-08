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
    public float bodyHeightMax = 2f;

    private void Start()
    {
        // Abilita l'interpolazione per i corpi rigidi delle mani per migliorare la fluidità
        Rigidbody leftHandRigidbody = leftHandJoint.GetComponent<Rigidbody>();
        Rigidbody rightHandRigidbody = rightHandJoint.GetComponent<Rigidbody>();
        if (leftHandRigidbody != null)
        {
            leftHandRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        }
        if (rightHandRigidbody != null)
        {
            rightHandRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        }
    }

    private void FixedUpdate()
    {
        // Aggiorna la posizione del collider del corpo in base alla posizione della testa
        bodyCollider.height = Mathf.Clamp(playerHead.localPosition.y, bodyHeightMin, bodyHeightMax);
        bodyCollider.center = new Vector3(playerHead.localPosition.x, bodyCollider.height / 2, playerHead.localPosition.z);

        // Aggiorna i giunti delle mani e della testa in base alla posizione dei controller
        UpdateHandJoint(leftHandJoint, OVRInput.Controller.LTouch);
        UpdateHandJoint(rightHandJoint, OVRInput.Controller.RTouch);

        // Posizione della testa
        headJoint.targetPosition = playerHead.localPosition;
    }

    private void UpdateHandJoint(ConfigurableJoint joint, OVRInput.Controller controller)
    {
        joint.targetPosition = OVRInput.GetLocalControllerPosition(controller);
        joint.targetRotation = OVRInput.GetLocalControllerRotation(controller);
    }
}
