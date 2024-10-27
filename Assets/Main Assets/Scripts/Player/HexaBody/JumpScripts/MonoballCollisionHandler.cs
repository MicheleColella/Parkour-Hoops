using UnityEngine;
using System.Collections;

public class MonoballCollisionHandler : MonoBehaviour
{
    public JumpController jumpController;  // Riferimento al JumpController
    public bool isGrounded;
    public float groundDelay = 0.4f;  // Tempo di ritardo prima di impostare isGrounded a false
    public float maxSurfaceAngle = 45f; // Angolo massimo della superficie per non rotolare
    public float highAngularDrag = 150f; // Valore di drag angolare più alto per superfici meno inclinate
    public float normalAngularDrag = 5f; // Drag angolare standard per movimento regolare
    public float objectAngle;

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
        // Calcola l'angolo della superficie usando la normale della collisione
        Vector3 averageNormal = Vector3.zero;
        foreach (ContactPoint contact in collision.contacts)
        {
            averageNormal += contact.normal;
        }
        averageNormal /= collision.contactCount;

        float surfaceAngle = Vector3.Angle(averageNormal, Vector3.up);

        if (surfaceAngle <= maxSurfaceAngle)
        {
            // Se l'angolo della superficie è inferiore al massimo, aumenta il drag angolare per limitare la rotazione
            isGrounded = true;
            monoballRb.angularDrag = highAngularDrag;

            // Se è presente una coroutine in esecuzione per impostare `isGrounded` a false, la fermiamo
            if (groundCheckCoroutine != null)
            {
                StopCoroutine(groundCheckCoroutine);
                groundCheckCoroutine = null;
            }
        }
        else
        {
            // Se la superficie è troppo inclinata, usa il drag angolare normale e permetti la rotazione
            isGrounded = true;
            monoballRb.angularDrag = normalAngularDrag;

            // Se è presente una coroutine in esecuzione per impostare `isGrounded` a false, la fermiamo
            if (groundCheckCoroutine != null)
            {
                StopCoroutine(groundCheckCoroutine);
                groundCheckCoroutine = null;
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        // Avviamo una coroutine che aspetta il delay prima di impostare `isGrounded` a false
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
