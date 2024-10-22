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
    public Animator handAnimator; // Riferimento all'animator della mano
    public List<Collider> fingerTipColliders; // Lista dei colliders delle punte delle dita
    public float grabValueSpeed = 1.0f; // Velocità di incremento di GrabValue

    private FixedJoint fixedJoint;
    private bool isGrabbing = false;
    private Collider grabbedObjectCollider; // Il collider dell'oggetto afferrato
    private float grabValue = 0f; // Valore corrente di "GrabValue"
    private bool objectTouchedByFingers = false; // Per monitorare se l'oggetto è toccato dai colliders delle dita

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
                fixedJoint.autoConfigureConnectedAnchor = true; // Lascia auto-configurare l'anchor per mantenere l'oggetto nella sua posizione originale

                if (nearbyRigidbody)
                {
                    fixedJoint.connectedBody = nearbyRigidbody;
                }

                isGrabbing = true;
                StartCoroutine(IncreaseGrabValue()); // Avvia l'incremento graduale di GrabValue
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

            StopAllCoroutines(); // Ferma l'incremento di GrabValue
            ResetGrabValue(); // Resetta il valore di GrabValue
        }
    }

    // Coroutine per aumentare gradualmente GrabValue con velocità regolabile
    private IEnumerator IncreaseGrabValue()
    {
        while (grabValue < 1f && !objectTouchedByFingers)
        {
            grabValue += Time.deltaTime * grabValueSpeed; // Aumenta gradualmente GrabValue in base alla velocità impostata
            handAnimator.SetFloat("GrabValue", grabValue); // Aggiorna il parametro "GrabValue" nell'animator

            // Controlla se uno dei collider delle dita tocca l'oggetto afferrato
            foreach (Collider fingerTipCollider in fingerTipColliders)
            {
                if (fingerTipCollider.bounds.Intersects(grabbedObjectCollider.bounds))
                {
                    objectTouchedByFingers = true;
                    break;
                }
            }

            yield return null;
        }
    }

    // Funzione per resettare GrabValue
    private void ResetGrabValue()
    {
        grabValue = 0f;
        objectTouchedByFingers = false;
        handAnimator.SetFloat("GrabValue", grabValue);
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
