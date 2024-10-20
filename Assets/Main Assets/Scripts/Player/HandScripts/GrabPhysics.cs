using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GrabPhysics : MonoBehaviour
{
    public InputActionProperty grabInputSource;
    public float radius = 0.1f;
    public LayerMask grabLayer;
    public GameObject sphereCenterObject; // Riferimento al GameObject che determina il centro della sfera
    public List<Collider> handColliders; // Lista di colliders della mano, da assegnare nell'Inspector

    private FixedJoint fixedJoint;
    private bool isGrabbing = false;
    private Collider grabbedObjectCollider; // Il collider dell'oggetto afferrato

    private void FixedUpdate()
    {
        bool isGrabButtonPressed = grabInputSource.action.ReadValue<float>() > 0.1f;

        if (isGrabButtonPressed && !isGrabbing)
        {
            // Usa la posizione del GameObject specificato come centro della sfera
            Vector3 spherePosition = sphereCenterObject != null ? sphereCenterObject.transform.position : transform.position;
            Collider[] nearbyColliders = Physics.OverlapSphere(spherePosition, radius, grabLayer, QueryTriggerInteraction.Ignore);

            if (nearbyColliders.Length > 0)
            {
                Rigidbody nearbyRigidbody = nearbyColliders[0].attachedRigidbody;

                // Ignora le collisioni tra ciascun collider della mano e l'oggetto preso
                grabbedObjectCollider = nearbyColliders[0];
                if (handColliders != null && grabbedObjectCollider != null)
                {
                    foreach (Collider handCollider in handColliders)
                    {
                        Physics.IgnoreCollision(handCollider, grabbedObjectCollider, true);
                    }
                }

                fixedJoint = gameObject.AddComponent<FixedJoint>();
                fixedJoint.autoConfigureConnectedAnchor = false;

                if (nearbyRigidbody)
                {
                    fixedJoint.connectedBody = nearbyRigidbody;
                    fixedJoint.connectedAnchor = nearbyRigidbody.transform.InverseTransformPoint(spherePosition);
                }
                else
                {
                    fixedJoint.connectedAnchor = spherePosition;
                }

                isGrabbing = true;
            }
        }
        else if (!isGrabButtonPressed && isGrabbing)
        {
            isGrabbing = false;

            // Ripristina le collisioni tra la mano e l'oggetto rilasciato
            if (handColliders != null && grabbedObjectCollider != null)
            {
                foreach (Collider handCollider in handColliders)
                {
                    Physics.IgnoreCollision(handCollider, grabbedObjectCollider, false);
                }
                grabbedObjectCollider = null; // Reset del riferimento all'oggetto afferrato
            }

            if (fixedJoint)
            {
                Destroy(fixedJoint);
            }
        }
    }

    // Funzione per disegnare la sfera nel Scene view
    private void OnDrawGizmos()
    {
        if (sphereCenterObject == null)
            return;

        // Imposta il colore del Gizmo
        Gizmos.color = Color.yellow;

        // Usa la posizione del GameObject specificato come centro della sfera
        Vector3 spherePosition = sphereCenterObject.transform.position;

        // Disegna la sfera
        Gizmos.DrawWireSphere(spherePosition, radius);
    }
}
