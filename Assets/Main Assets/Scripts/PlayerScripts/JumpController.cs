using UnityEngine;

public class JumpController : MonoBehaviour
{
    public VRLocomotionManager locomotionManager;
    public Rigidbody playerRigidbody;
    public CapsuleCollider playerCollider;

    private ClimbingColliderAdjuster climbingColliderAdjuster;

    private enum JumpState { NotCharging, Charging, InAir }
    private JumpState jumpState = JumpState.NotCharging;
    private bool isJumping = false;
    private float jumpChargeTime = 0f;

    void Start()
    {
        climbingColliderAdjuster = GetComponent<ClimbingColliderAdjuster>();
    }

    void FixedUpdate()
    {
        HandleJumpState();
    }

    void HandleJumpState()
    {
        switch (jumpState)
        {
            case JumpState.NotCharging:
                if (OVRInput.Get(OVRInput.Button.One) && climbingColliderAdjuster.IsGrounded())
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
                    if (climbingColliderAdjuster.IsGrounded())
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
                else if (!climbingColliderAdjuster.IsGrounded())
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
                if (climbingColliderAdjuster.IsGrounded())
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
}
