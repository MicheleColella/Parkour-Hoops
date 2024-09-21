using UnityEngine;

public class JumpController : MonoBehaviour
{
    public VRLocomotionManager locomotionManager;
    public Rigidbody playerRigidbody;
    public CapsuleCollider playerCollider;

    private enum JumpState { NotCharging, Charging, InAir }
    private JumpState jumpState = JumpState.NotCharging;
    private bool isJumping = false;
    private float jumpChargeTime = 0f;

    void FixedUpdate()
    {
        HandleJumpState();
    }

    void HandleJumpState()
    {
        switch (jumpState)
        {
            case JumpState.NotCharging:
                if (OVRInput.Get(OVRInput.Button.One) && IsGrounded())
                {
                    jumpState = JumpState.Charging;
                    jumpChargeTime = 0f;
                    VibrationController.Instance.StartVibration(0.5f, locomotionManager.vibrationStartIntensity);
                }
                else
                {
                    VibrationController.Instance.StopVibration();
                }
                break;

            case JumpState.Charging:
                if (!OVRInput.Get(OVRInput.Button.One))
                {
                    if (IsGrounded())
                    {
                        float appliedJumpForce = Mathf.Lerp(locomotionManager.minJumpForce, locomotionManager.maxJumpForce, jumpChargeTime / locomotionManager.maxJumpChargeTime);
                        appliedJumpForce = Mathf.Clamp(appliedJumpForce, locomotionManager.minJumpForce, locomotionManager.maxJumpForce);
                        Vector3 horizontalVelocity = playerRigidbody.velocity;
                        horizontalVelocity.y = appliedJumpForce;
                        playerRigidbody.velocity = horizontalVelocity;

                        jumpState = JumpState.InAir;
                        isJumping = true;
                    }
                    else
                    {
                        jumpState = JumpState.NotCharging;
                    }

                    jumpChargeTime = 0f;
                    VibrationController.Instance.StopVibration();
                }
                else if (!IsGrounded())
                {
                    jumpState = JumpState.NotCharging;
                    jumpChargeTime = 0f;
                    VibrationController.Instance.StopVibration();
                }
                else
                {
                    jumpChargeTime += Time.fixedDeltaTime;
                    jumpChargeTime = Mathf.Clamp(jumpChargeTime, 0, locomotionManager.maxJumpChargeTime);
                    float vibrationStrength = Mathf.Lerp(locomotionManager.vibrationStartIntensity, locomotionManager.vibrationMaxIntensity, jumpChargeTime / locomotionManager.maxJumpChargeTime);
                    VibrationController.Instance.UpdateVibration(0.5f, vibrationStrength);
                }
                break;

            case JumpState.InAir:
                if (IsGrounded())
                {
                    jumpState = JumpState.NotCharging;
                    isJumping = false;
                    jumpChargeTime = 0f;

                    // Interpola la velocità verticale verso zero per un atterraggio più fluido
                    Vector3 velocity = playerRigidbody.velocity;
                    velocity.y = Mathf.Lerp(velocity.y, 0, Time.fixedDeltaTime * 10f);
                    playerRigidbody.velocity = velocity;

                    VibrationController.Instance.StopVibration();
                }
                break;
        }
    }

    bool IsGrounded()
    {
        Vector3 groundCheckPos = playerCollider.bounds.center - new Vector3(0, playerCollider.bounds.extents.y, 0);
        return Physics.OverlapSphere(groundCheckPos, locomotionManager.groundCheckRadius, locomotionManager.groundLayers).Length > 0;
    }
}
