using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpController : MonoBehaviour
{
    [Header("Jump Settings")]
    public float jumpForce = 10.0f;  // Forza applicata per il salto
    public float groundedCheckDelay = 0.2f;  // Delay prima di impostare isGrounded a false

    [SerializeField]
    public bool isGrounded = true;  // Stato che indica se la Monoball è a terra, visibile nell'Inspector
    private bool canJump = true;  // Controlla se il salto è permesso

    [Header("Target Rigidbody")]
    public Rigidbody targetRigidbody;  // RigidBody a cui applicare la forza

    private XRControllerInputManager inputManager;

    void Start()
    {
        // Ottieni il riferimento al gestore input
        inputManager = XRControllerInputManager.Instance;

        // Se non è stato assegnato un RigidBody, usa quello della Monoball di default
        if (targetRigidbody == null)
        {
            HexaBodyController hexaBodyController = GetComponent<HexaBodyController>();
            if (hexaBodyController != null)
            {
                targetRigidbody = hexaBodyController.Monoball.GetComponent<Rigidbody>();
            }
        }

        if (targetRigidbody == null)
        {
            Debug.LogError("Nessun Rigidbody assegnato per il salto!");
        }
    }

    void Update()
    {
        HandleJumpInput();
    }

    private void HandleJumpInput()
    {
        // Verifica se il player è a terra e preme il pulsante di salto (ad esempio, il tasto A del controller)
        if (isGrounded && canJump && inputManager.GetRightPrimaryButton())
        {
            PerformJump();
        }
    }

    private void PerformJump()
    {
        if (targetRigidbody == null) return;

        //Debug.Log("Jump");
        // Applica la forza verso l'alto al Rigidbody assegnato
        targetRigidbody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        isGrounded = false;  // Imposta isGrounded a false subito dopo il salto

        // Inizia il ritardo per aggiornare isGrounded a false se non si è a terra
        StartCoroutine(GroundedCheckDelayCoroutine());
    }

    // Questo coroutine gestisce il ritardo prima di impostare isGrounded a false
    private IEnumerator GroundedCheckDelayCoroutine()
    {
        yield return new WaitForSeconds(groundedCheckDelay);

        // Se dopo il delay non si è ancora a terra, imposta isGrounded a false
        if (!isGrounded)
        {
            isGrounded = false;
        }
    }

    // Metodo chiamato dal MonoballCollisionHandler per aggiornare lo stato di isGrounded
    public void SetGrounded(bool grounded)
    {
        isGrounded = grounded;

        if (grounded)
        {
            StopAllCoroutines();  // Ferma qualsiasi coroutine in esecuzione che sta gestendo il delay
        }
        else
        {
            StartCoroutine(GroundedCheckDelayCoroutine());  // Avvia il delay prima di impostare isGrounded a false
        }
    }
}
