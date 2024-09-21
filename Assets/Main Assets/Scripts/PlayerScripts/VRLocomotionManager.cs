using UnityEngine;

public class VRLocomotionManager : MonoBehaviour
{
    [Header("Movement Settings")]
    public float baseWalkSpeed = 2f;
    public float maxWalkSpeed = 15f;
    public float movementSensitivity = 0.02f;
    public float movementLerpSpeed = 10f;
    public float inertiaDuration = 0.5f;
    public float decelerationRate = 2f;
    public float movementMultiplier = 5f;

    [Header("Jump Settings")]
    public float minJumpForce = 5f;
    public float maxJumpForce = 10f;
    public float maxJumpChargeTime = 3f;
    public float reducedGravity = 0.5f;

    [Header("Vibration Settings")]
    public float vibrationStartIntensity = 0.2f;
    public float vibrationMaxIntensity = 1.0f;

    [Header("Snap Turn Settings")]
    public float snapTurnAngle = 45f;
    public float snapTurnCooldown = 0.1f;

    [Header("Ground Check Settings")]
    public LayerMask groundLayers;
    public float groundCheckRadius = 0.12f;
}
