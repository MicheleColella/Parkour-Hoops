using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpController : MonoBehaviour
{
    [Header("Jump Settings")]
    public float minJumpForce = 5.0f;  // Forza minima per il salto
    public float maxJumpForce = 15.0f;  // Forza massima per il salto
    public float maxChargeTime = 3.0f;  // Tempo massimo di carica del salto
    public float groundedCheckDelay = 0.2f;  // Delay prima di impostare isGrounded a false

    [SerializeField]
    public bool isGrounded = true;  // Stato che indica se la Monoball è a terra, visibile nell'Inspector
    private bool isCharging = false;  // Controlla se il salto è in fase di carica
    private float chargeStartTime;  // Tempo di inizio della carica
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
        // Verifica se il player è a terra e può iniziare a caricare il salto
        if (isGrounded && canJump && !isCharging && inputManager.GetRightPrimaryButton())
        {
            StartCharging();
        }
        // Se il pulsante viene rilasciato, esegui il salto
        else if (isCharging && !inputManager.GetRightPrimaryButton())
        {
            PerformJump();
        }
    }

    private void StartCharging()
    {
        isCharging = true;
        chargeStartTime = Time.time;  // Memorizza il tempo di inizio della carica
    }

    private void PerformJump()
    {
        if (targetRigidbody == null) return;

        // Calcola il tempo di carica
        float chargeTime = Time.time - chargeStartTime;
        // Limita il tempo di carica al valore massimo
        chargeTime = Mathf.Clamp(chargeTime, 0, maxChargeTime);

        // Calcola la forza del salto basata sul tempo di carica
        float jumpForce = Mathf.Lerp(minJumpForce, maxJumpForce, chargeTime / maxChargeTime);

        // Applica la forza verso l'alto al Rigidbody assegnato
        targetRigidbody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        isGrounded = false;  // Imposta isGrounded a false subito dopo il salto
        isCharging = false;  // Reset dello stato di carica

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
