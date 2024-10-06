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

    void FixedUpdate()
    {
        // Usa il pulsante corretto in base alla mano selezionata
        bool isGrabButtonPressed = handType == HandType.Left
            ? OVRInput.Get(OVRInput.Button.PrimaryHandTrigger)  // Mano sinistra
            : OVRInput.Get(OVRInput.Button.SecondaryHandTrigger);  // Mano destra

        if (isGrabButtonPressed && !isGrabbing)
        {
            // Usa l'origine assegnata per rilevare i collider nelle vicinanze
            Collider[] nearbyColliders = Physics.OverlapSphere(grabOrigin.transform.position, radius, grabLayer, QueryTriggerInteraction.Ignore);

            if (nearbyColliders.Length > 0)
            {
                Rigidbody nearbyRigidBody = nearbyColliders[0].attachedRigidbody;

                // Crea un FixedJoint per attaccare l'oggetto preso
                fixedJoint = gameObject.AddComponent<FixedJoint>();
                fixedJoint.autoConfigureConnectedAnchor = false;

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
            // Rilascia l'oggetto
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
