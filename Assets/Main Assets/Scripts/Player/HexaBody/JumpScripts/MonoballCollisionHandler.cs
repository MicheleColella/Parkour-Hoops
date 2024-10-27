using UnityEngine;
using System.Collections;

public class MonoballCollisionHandler : MonoBehaviour
{
    public JumpController jumpController;  // Riferimento al JumpController
    public bool isGrounded;
    public float groundDelay = 0.4f;  // Tempo di ritardo prima di impostare isGrounded a false

    private Coroutine groundCheckCoroutine;

    private void OnCollisionStay(Collision collision)
    {
        isGrounded = true;
        // Se è presente una coroutine in esecuzione per impostare `isGrounded` a false, la fermiamo
        if (groundCheckCoroutine != null)
        {
            StopCoroutine(groundCheckCoroutine);
            groundCheckCoroutine = null;
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
