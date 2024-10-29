using UnityEngine;
using System.Collections;

public class JumpController : MonoBehaviour
{
    public MonoballCollisionHandler monoballCollisionHandler;

    [Header("Jump Settings")]
    public float minJumpForce = 5.0f;
    public float maxJumpForce = 15.0f;
    public float maxChargeTime = 3.0f;

    [Header("Rigidbody Settings")]
    public Rigidbody targetRigidbody;

    [Header("Shader Settings")]
    [Tooltip("Renderer del materiale che contiene la proprietà _Size dello shader.")]
    public Renderer targetRenderer; // Renderer per accedere al materiale

    [Tooltip("Nome della proprietà dello shader da modificare.")]
    public string sizePropertyName = "_Size"; // Nome della proprietà dello shader

    [Tooltip("Valore minimo della proprietà _Size.")]
    public float shaderMinSize = 1.0f;

    [Tooltip("Valore massimo della proprietà _Size.")]
    public float shaderMaxSize = 2.0f;

    [Tooltip("Durata del cambiamento della proprietà _Size quando si raggiunge la carica massima.")]
    public float shaderChangeDuration = 1.0f;

    [Tooltip("Durata del reset della proprietà _Size dopo il salto.")]
    public float shaderResetDuration = 1.0f;

    private bool isCharging = false;
    private float chargeStartTime;
    private bool reachedMaxCharge = false;

    private XRControllerInputManager inputManager;

    // Riferimenti alle coroutine per gestire le transizioni dello shader
    private Coroutine shaderIncreaseCoroutine;
    private Coroutine shaderDecreaseCoroutine;

    void Start()
    {
        inputManager = XRControllerInputManager.Instance;

        // Assegna il Rigidbody se non è già assegnato
        if (targetRigidbody == null)
        {
            HexaBodyController hexaBodyController = GetComponent<HexaBodyController>();
            if (hexaBodyController != null && hexaBodyController.monoball != null)
            {
                targetRigidbody = hexaBodyController.monoball.GetComponent<Rigidbody>();
            }
        }

        if (targetRigidbody == null)
        {
            Debug.LogError("Nessun Rigidbody assegnato per il salto!");
        }

        // Assegna il Renderer se non è già assegnato
        if (targetRenderer == null)
        {
            targetRenderer = GetComponent<Renderer>();
            if (targetRenderer == null)
            {
                Debug.LogError("Nessun Renderer assegnato per la manipolazione dello shader!");
            }
            else
            {
                // Inizializza la proprietà _Size dello shader al valore minimo
                targetRenderer.material.SetFloat(sizePropertyName, shaderMinSize);
            }
        }
    }

    void Update()
    {
        HandleJumpInput();
        HandleCharging();
    }

    /// <summary>
    /// Gestisce l'input per il salto.
    /// </summary>
    private void HandleJumpInput()
    {
        bool isRightPrimaryButtonPressed = inputManager.GetRightPrimaryButton();

        if (monoballCollisionHandler.isGrounded && !isCharging && isRightPrimaryButtonPressed)
        {
            StartCharging();
        }
        else if (isCharging && !isRightPrimaryButtonPressed)
        {
            PerformJump();
        }
    }

    /// <summary>
    /// Gestisce lo stato di caricamento del salto.
    /// </summary>
    private void HandleCharging()
    {
        if (isCharging)
        {
            float chargeTime = Time.time - chargeStartTime;

            if (chargeTime >= maxChargeTime && !reachedMaxCharge)
            {
                reachedMaxCharge = true;
                TriggerShaderIncrease();
            }
        }
    }

    /// <summary>
    /// Inizia il caricamento del salto.
    /// </summary>
    private void StartCharging()
    {
        isCharging = true;
        reachedMaxCharge = false;
        chargeStartTime = Time.time;
    }

    /// <summary>
    /// Esegue il salto applicando la forza calcolata.
    /// </summary>
    private void PerformJump()
    {
        if (targetRigidbody == null) return;

        isCharging = false;

        float chargeTime = Mathf.Clamp(Time.time - chargeStartTime, 0f, maxChargeTime);
        float jumpForce = Mathf.Lerp(minJumpForce, maxJumpForce, chargeTime / maxChargeTime);

        targetRigidbody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

        monoballCollisionHandler.isGrounded = false;

        if (reachedMaxCharge)
        {
            TriggerShaderDecrease();
        }
    }

    /// <summary>
    /// Imposta lo stato di grounded, chiamato da MonoballCollisionHandler.
    /// </summary>
    /// <param name="grounded">Se l'oggetto è a terra.</param>
    public void SetGrounded(bool grounded)
    {
        monoballCollisionHandler.isGrounded = grounded;
    }

    // Proprietà pubblica per accedere allo stato di grounded
    public bool IsGrounded
    {
        get { return monoballCollisionHandler.isGrounded; }
    }

    /// <summary>
    /// Inizia la transizione dell'shader per aumentare la proprietà _Size.
    /// </summary>
    private void TriggerShaderIncrease()
    {
        if (targetRenderer == null) return;

        // Ferma eventuali coroutine di diminuzione in corso
        if (shaderDecreaseCoroutine != null)
        {
            StopCoroutine(shaderDecreaseCoroutine);
            shaderDecreaseCoroutine = null;
        }

        // Avvia la coroutine per aumentare la dimensione dello shader
        shaderIncreaseCoroutine = StartCoroutine(ChangeShaderSize(shaderMinSize, shaderMaxSize, shaderChangeDuration));
    }

    /// <summary>
    /// Inizia la transizione dell'shader per diminuire la proprietà _Size.
    /// </summary>
    private void TriggerShaderDecrease()
    {
        if (targetRenderer == null) return;

        // Ferma eventuali coroutine di aumento in corso
        if (shaderIncreaseCoroutine != null)
        {
            StopCoroutine(shaderIncreaseCoroutine);
            shaderIncreaseCoroutine = null;
        }

        // Avvia la coroutine per diminuire la dimensione dello shader
        shaderDecreaseCoroutine = StartCoroutine(ChangeShaderSize(shaderMaxSize, shaderMinSize, shaderResetDuration));
    }

    /// <summary>
    /// Coroutine per modificare gradualmente la proprietà _Size dello shader.
    /// </summary>
    /// <param name="startSize">Valore iniziale della proprietà _Size.</param>
    /// <param name="endSize">Valore finale della proprietà _Size.</param>
    /// <param name="duration">Durata della transizione.</param>
    /// <returns>IEnumerator per la coroutine.</returns>
    private IEnumerator ChangeShaderSize(float startSize, float endSize, float duration)
    {
        float elapsed = 0f;

        // Assicurati di avere un'istanza del materiale
        Material mat = targetRenderer.material;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float newSize = Mathf.Lerp(startSize, endSize, elapsed / duration);
            mat.SetFloat(sizePropertyName, newSize);
            yield return null;
        }

        // Imposta il valore finale esattamente
        mat.SetFloat(sizePropertyName, endSize);
    }
}
