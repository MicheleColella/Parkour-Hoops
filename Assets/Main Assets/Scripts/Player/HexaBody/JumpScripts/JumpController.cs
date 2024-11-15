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
    public Renderer targetRenderer;
    public string sizePropertyName = "_Size";
    public float shaderMinSize = 1.0f;
    public float shaderMaxSize = 2.0f;
    public float shaderChangeDuration = 1.0f;
    public float shaderResetDuration = 1.0f;

    private bool isCharging = false;
    private float chargeStartTime;
    private bool reachedMaxCharge = false;

    private XRControllerInputManager inputManager;

    private Coroutine shaderIncreaseCoroutine;
    private Coroutine shaderDecreaseCoroutine;

    void Start()
    {
        inputManager = XRControllerInputManager.Instance;

        if (targetRigidbody == null)
        {
            HexaBodyController hexaBodyController = GetComponent<HexaBodyController>();
            if (hexaBodyController != null && hexaBodyController.monoball != null)
            {
                targetRigidbody = hexaBodyController.monoball.GetComponent<Rigidbody>();
            }
        }

        if (targetRenderer == null)
        {
            targetRenderer = GetComponent<Renderer>();
            if (targetRenderer != null)
            {
                targetRenderer.material.SetFloat(sizePropertyName, shaderMinSize);
            }
        }
    }

    void Update()
    {
        HandleJumpInput();
        HandleCharging();
    }

    private void HandleJumpInput()
    {
        bool isRightPrimaryButtonPressed = inputManager.GetRightPrimaryButton();

        if (monoballCollisionHandler.isGrounded && !isCharging && isRightPrimaryButtonPressed)
        {
            StartCharging();
        }
        else if (isCharging && monoballCollisionHandler.isGrounded && !isRightPrimaryButtonPressed)
        {
            PerformJump(); // Salta quando il pulsante viene rilasciato
        }
        else if (!monoballCollisionHandler.isGrounded)
        {
            CancelCharging(); // Cancella la carica se il giocatore è in aria
        }
    }

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

    private void StartCharging()
    {
        isCharging = true;
        reachedMaxCharge = false;
        chargeStartTime = Time.time;
    }

    private void CancelCharging()
    {
        isCharging = false;

        if (shaderIncreaseCoroutine != null)
        {
            StopCoroutine(shaderIncreaseCoroutine);
            shaderIncreaseCoroutine = null;
        }

        if (shaderDecreaseCoroutine == null)
        {
            TriggerShaderDecrease();
        }
    }

    private void PerformJump()
    {
        if (targetRigidbody == null || !monoballCollisionHandler.isGrounded) return;

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

    public void SetGrounded(bool grounded)
    {
        monoballCollisionHandler.isGrounded = grounded;
    }

    public bool IsGrounded => monoballCollisionHandler.isGrounded;

    private void TriggerShaderIncrease()
    {
        if (targetRenderer == null) return;

        if (shaderDecreaseCoroutine != null)
        {
            StopCoroutine(shaderDecreaseCoroutine);
            shaderDecreaseCoroutine = null;
        }

        shaderIncreaseCoroutine = StartCoroutine(ChangeShaderSize(shaderMinSize, shaderMaxSize, shaderChangeDuration));
    }

    private void TriggerShaderDecrease()
    {
        if (targetRenderer == null) return;

        if (shaderIncreaseCoroutine != null)
        {
            StopCoroutine(shaderIncreaseCoroutine);
            shaderIncreaseCoroutine = null;
        }

        shaderDecreaseCoroutine = StartCoroutine(ChangeShaderSize(shaderMaxSize, shaderMinSize, shaderResetDuration));
    }

    private IEnumerator ChangeShaderSize(float startSize, float endSize, float duration)
    {
        float elapsed = 0f;

        Material mat = targetRenderer.material;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float newSize = Mathf.Lerp(startSize, endSize, elapsed / duration);
            mat.SetFloat(sizePropertyName, newSize);
            yield return null;
        }

        mat.SetFloat(sizePropertyName, endSize);
    }
}
