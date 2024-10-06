using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContinuousMovementPhysics : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 8f; // Increased speed for faster movement
    public float acceleration = 30f; // Increased acceleration for quicker response
    public float deceleration = 40f; // Increased deceleration for quicker stopping
    public float jumpHeight = 1.5f;
    public bool onlyMoveWhenGrounded = true;

    [Header("Turn Settings")]
    public float snapTurnAngle = 45f; // Angle of each snap turn
    public float snapTurnCooldown = 0.5f; // Time in seconds between each snap turn

    [Header("References")]
    public Rigidbody rb;
    public Transform directionSource; // Used to determine the movement direction (e.g., player head)
    public CapsuleCollider bodyCollider;
    public Transform turnSource; // Used to apply the turn rotation
    public LayerMask groundLayer;

    private Vector2 inputMoveAxis;
    private float inputTurnAxis;
    private bool isGrounded;
    private float lastSnapTurnTime;
    private Vector3 currentVelocity;

    void Start()
    {
        // Initialize the last snap turn time to ensure immediate turn availability
        lastSnapTurnTime = -snapTurnCooldown;
    }

    void Update()
    {
        // Get the input from the Meta Quest controllers using OVRInput
        Vector2 controllerMove = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);  // Left thumbstick for movement
        float controllerTurn = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick).x;  // Right thumbstick for turning

        // Get input from keyboard
        float keyboardHorizontal = Input.GetAxis("Horizontal"); // A/D or Left/Right arrows
        float keyboardVertical = Input.GetAxis("Vertical"); // W/S or Up/Down arrows
        float keyboardTurn = Input.GetAxis("Mouse X"); // Mouse movement for turning

        // Combine controller and keyboard inputs
        inputMoveAxis = controllerMove + new Vector2(keyboardHorizontal, keyboardVertical);
        inputTurnAxis = controllerTurn + keyboardTurn;

        // Normalize to prevent faster diagonal movement
        if (inputMoveAxis.sqrMagnitude > 1)
        {
            inputMoveAxis.Normalize();
        }

        // Check for the jump input using the "A" button on the right controller or spacebar
        bool jumpInput = OVRInput.GetDown(OVRInput.Button.One) || Input.GetKeyDown(KeyCode.Space);

        // If jump button is pressed and the player is grounded, apply the jump force
        if (jumpInput && isGrounded)
        {
            float jumpVelocity = Mathf.Sqrt(2 * jumpHeight * Mathf.Abs(Physics.gravity.y));
            Vector3 velocity = rb.velocity;
            rb.velocity = new Vector3(velocity.x, jumpVelocity, velocity.z);
        }

        // Handle snap turn logic
        if (Mathf.Abs(inputTurnAxis) > 0.5f && Time.time - lastSnapTurnTime > snapTurnCooldown)
        {
            float turnDirection = inputTurnAxis > 0 ? 1f : -1f;
            rb.MoveRotation(rb.rotation * Quaternion.Euler(0, snapTurnAngle * turnDirection, 0));
            lastSnapTurnTime = Time.time;
        }
    }

    void FixedUpdate()
    {
        // Check if the player is grounded
        isGrounded = CheckIfGrounded();

        // Only allow movement when grounded if the flag is set, otherwise always move
        if (!onlyMoveWhenGrounded || (onlyMoveWhenGrounded && isGrounded))
        {
            // Calculate movement direction based on the player's head or body orientation
            Quaternion yaw = Quaternion.Euler(0, directionSource.eulerAngles.y, 0);
            Vector3 targetDirection = yaw * new Vector3(inputMoveAxis.x, 0, inputMoveAxis.y).normalized;

            // Adjust the current velocity towards the target direction for smoother acceleration and deceleration
            currentVelocity = Vector3.MoveTowards(currentVelocity, targetDirection * speed, (targetDirection.sqrMagnitude > 0 ? acceleration : deceleration) * Time.fixedDeltaTime);

            // Apply movement force
            rb.velocity = new Vector3(currentVelocity.x, rb.velocity.y, currentVelocity.z); // Directly set velocity for more immediate response
        }
    }

    // Function to check if the player is grounded using a sphere cast
    public bool CheckIfGrounded()
    {
        Vector3 start = bodyCollider.transform.TransformPoint(bodyCollider.center);
        float rayLength = bodyCollider.height / 2 - bodyCollider.radius + 0.1f;

        return Physics.SphereCast(start, bodyCollider.radius * 0.9f, Vector3.down, out RaycastHit hitInfo, rayLength, groundLayer);
    }
}