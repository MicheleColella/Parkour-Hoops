using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabPhysics : MonoBehaviour
{
    public float radius = 0.1f;
    public LayerMask grabLayer;

    private FixedJoint fixedJoint;
    private bool isGrabbing = false;

    // Specify whether this script is for the left or right hand
    public bool isLeftHand;

    void FixedUpdate()
    {
        // Use the correct grip button based on which hand this script is attached to
        bool isGrabButtonPressed = isLeftHand 
            ? OVRInput.Get(OVRInput.Button.PrimaryHandTrigger)  // Left hand grip
            : OVRInput.Get(OVRInput.Button.SecondaryHandTrigger);  // Right hand grip

        if (isGrabButtonPressed && !isGrabbing)
        {
            // Detect nearby colliders to grab
            Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, radius, grabLayer, QueryTriggerInteraction.Ignore);

            if (nearbyColliders.Length > 0)
            {
                Rigidbody nearbyRigidBody = nearbyColliders[0].attachedRigidbody;

                // Create a FixedJoint to attach the grabbed object
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
            // Release the object
            isGrabbing = false;

            if (fixedJoint)
            {
                Destroy(fixedJoint);
            }
        }
    }
}
