using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabPhysics : MonoBehaviour
{
    public enum HandType
    {
        Left,
        Right
    }

    public HandType handType;
    public GameObject grabOrigin;  // Assegnato come origine per l'OverlapSphere
    public float radius = 0.1f;
    public LayerMask grabLayer;

    private FixedJoint fixedJoint;
    private bool isGrabbing = false;

    private Collider[] nearbyColliders = new Collider[10]; // Pre-allocate an array

    void FixedUpdate() 
    {
        bool isGrabButtonPressed = handType == HandType.Left
            ? OVRInput.Get(OVRInput.Button.PrimaryHandTrigger)
            : OVRInput.Get(OVRInput.Button.SecondaryHandTrigger);

        if (isGrabButtonPressed && !isGrabbing)
        {
            int numColliders = Physics.OverlapSphereNonAlloc(grabOrigin.transform.position, radius, nearbyColliders, grabLayer, QueryTriggerInteraction.Ignore);

            if (numColliders > 0)
            {
                Rigidbody nearbyRigidBody = nearbyColliders[0].attachedRigidbody;

                if (fixedJoint == null)
                {
                    fixedJoint = gameObject.AddComponent<FixedJoint>();
                    fixedJoint.autoConfigureConnectedAnchor = false;
                }

                if (nearbyRigidBody)
                {
                    fixedJoint.connectedBody = nearbyRigidBody;
                    fixedJoint.connectedAnchor = nearbyRigidBody.transform.InverseTransformPoint(grabOrigin.transform.position);
                }
                else
                {
                    fixedJoint.connectedAnchor = grabOrigin.transform.position;
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


    // Visualizza l'OverlapSphere nel editor
    void OnDrawGizmos()
    {
        if (grabOrigin != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(grabOrigin.transform.position, radius);
        }
    }
}
