using System.Collections;
using UnityEngine;

public class JumpController : MonoBehaviour
{
    [Header("Jump Settings")]
    public float minJumpForce = 5.0f;
    public float maxJumpForce = 15.0f;
    public float maxChargeTime = 3.0f;
    public float groundedCheckDelay = 0.2f;

    [Header("Rigidbody Settings")]
    public Rigidbody targetRigidbody;

    [SerializeField]
    private bool isGrounded = true;

    private bool isCharging = false;
    private float chargeStartTime;

    private XRControllerInputManager inputManager;

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

        if (targetRigidbody == null)
        {
            Debug.LogError("No Rigidbody assigned for jumping!");
        }
    }

    void Update()
    {
        HandleJumpInput();
    }

    private void HandleJumpInput()
    {
        bool isRightPrimaryButtonPressed = inputManager.GetRightPrimaryButton();

        if (isGrounded && !isCharging && isRightPrimaryButtonPressed)
        {
            StartCharging();
        }
        else if (isCharging && !isRightPrimaryButtonPressed)
        {
            PerformJump();
        }
    }

    private void StartCharging()
    {
        isCharging = true;
        chargeStartTime = Time.time;
    }

    private void PerformJump()
    {
        if (targetRigidbody == null) return;

        isCharging = false;

        float chargeTime = Mathf.Clamp(Time.time - chargeStartTime, 0f, maxChargeTime);
        float jumpForce = Mathf.Lerp(minJumpForce, maxJumpForce, chargeTime / maxChargeTime);

        targetRigidbody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

        isGrounded = false;
        StartCoroutine(GroundedCheckDelayCoroutine());
    }

    private IEnumerator GroundedCheckDelayCoroutine()
    {
        yield return new WaitForSeconds(groundedCheckDelay);

        if (!isGrounded)
        {
            isGrounded = false;
        }
    }

    /// <summary>
    /// Sets the grounded state, typically called by collision handlers.
    /// </summary>
    /// <param name="grounded">Whether the object is grounded.</param>
    public void SetGrounded(bool grounded)
    {
        isGrounded = grounded;

        if (grounded)
        {
            StopAllCoroutines();
        }
        else
        {
            StartCoroutine(GroundedCheckDelayCoroutine());
        }
    }
}
