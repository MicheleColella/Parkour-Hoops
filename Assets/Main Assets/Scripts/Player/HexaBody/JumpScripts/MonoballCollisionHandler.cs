using UnityEngine;
using System.Collections;

public class MonoballCollisionHandler : MonoBehaviour
{
    public JumpController jumpController;
    public bool isGrounded;
    public float groundDelay = 0.4f;
    public float maxSurfaceAngle = 45f;
    public float highAngularDrag = 150f;
    public float normalAngularDrag = 5f;
    public float dynamicDragFactor = 2f; // Fattore per regolare il drag in base all'angolo
    public float angularStopThreshold = 0.1f;
    public float antiSlipForce = 10f; // Forza applicata per evitare scivolamenti su pendenze

    private Coroutine groundCheckCoroutine;
    private Rigidbody monoballRb;

    private void Start()
    {
        monoballRb = GetComponent<Rigidbody>();
        if (monoballRb == null)
        {
            Debug.LogError("Monoball does not have a Rigidbody component.");
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        Vector3 averageNormal = Vector3.zero;
        foreach (ContactPoint contact in collision.contacts)
        {
            averageNormal += contact.normal;
        }
        averageNormal /= collision.contactCount;

        float surfaceAngle = Vector3.Angle(averageNormal, Vector3.up);

        if (surfaceAngle <= maxSurfaceAngle)
        {
            isGrounded = true;

            // Calcola il drag dinamico in base all'angolo della superficie per maggiore controllo in salita
            float dynamicDrag = Mathf.Lerp(normalAngularDrag, highAngularDrag, surfaceAngle / maxSurfaceAngle);
            monoballRb.angularDrag = dynamicDrag;

            // Applica forza anti-scivolamento quando la velocità è inferiore alla soglia e su pendenze
            if (monoballRb.velocity.magnitude < angularStopThreshold && surfaceAngle > 0f)
            {
                Vector3 antiSlipDirection = Vector3.ProjectOnPlane(-averageNormal, Vector3.up).normalized;
                monoballRb.AddForce(antiSlipDirection * antiSlipForce, ForceMode.Acceleration);
            }

            // Ferma la coroutine se il player è su una superficie stabile
            if (groundCheckCoroutine != null)
            {
                StopCoroutine(groundCheckCoroutine);
                groundCheckCoroutine = null;
            }
        }
        else
        {
            // Usa il drag normale quando la superficie è troppo inclinata
            isGrounded = false;
            monoballRb.angularDrag = normalAngularDrag;

            // Avvia la coroutine per ritardare lo stato di "non-grounded"
            if (groundCheckCoroutine == null)
            {
                groundCheckCoroutine = StartCoroutine(GroundCheckDelay());
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (groundCheckCoroutine == null)
        {
            groundCheckCoroutine = StartCoroutine(GroundCheckDelay());
        }
    }

    private IEnumerator GroundCheckDelay()
    {
        yield return new WaitForSeconds(groundDelay);
        isGrounded = false;
        groundCheckCoroutine = null;
    }
}
