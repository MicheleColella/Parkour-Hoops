using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabPhysics : MonoBehaviour
{
    public OVRInput.Button grabButton = OVRInput.Button.PrimaryHandTrigger; // Usa il pulsante del grilletto della mano
    public float radius = 0.1f;
    public LayerMask grabLayer;

    private FixedJoint fixedJoint;
    private bool isGrabbing = false;

    void FixedUpdate()
    {
        bool isGrabButtonPressed = OVRInput.Get(grabButton);

        if (isGrabButtonPressed && !isGrabbing)
        {
            Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, radius, grabLayer, QueryTriggerInteraction.Ignore);

            if (nearbyColliders.Length > 0)
            {
                Rigidbody nearbyRigidBody = nearbyColliders[0].attachedRigidbody;

                fixedJoint = gameObject.AddComponent<FixedJoint>();
                fixedJoint.autoConfigureConnectedAnchor = false;

                if (nearbyRigidBody)
                {
                    fixedJoint.connectedBody = nearbyRigidBody;
                    fixedJoint.connectedAnchor = nearbyRigidBody.transform.InverseTransformPoint(transform.position);
                }
                else
                {
                    fixedJoint.connectedAnchor = transform.position;
                }

                isGrabbing = true;
            }
        }
        else if (!isGrabButtonPressed && isGrabbing)
        {
            isGrabbing = false;
            if (fixedJoint)
            {
                Destroy(fixedJoint);
            }
        }
    }
}
