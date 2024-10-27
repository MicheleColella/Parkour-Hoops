using UnityEngine;

public class JumpController : MonoBehaviour
{
    public MonoballCollisionHandler monoballCollisionHandler;

    [Header("Jump Settings")]
    public float minJumpForce = 5.0f;
    public float maxJumpForce = 15.0f;
    public float maxChargeTime = 3.0f;

    [Header("Rigidbody Settings")]
    public Rigidbody targetRigidbody;


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

        if (monoballCollisionHandler.isGrounded && !isCharging && isRightPrimaryButtonPressed)
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

        monoballCollisionHandler.isGrounded = false;
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
}
