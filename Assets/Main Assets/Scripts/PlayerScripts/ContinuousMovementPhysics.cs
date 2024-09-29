using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContinuousMovementPhysics : MonoBehaviour
{
    public float speed = 1;
    public Rigidbody rb;
    public float turnSpeed = 60;
    private float jumpVelocity;
    public float jumpHeight = 1.5f;
    public Transform directionSource; // Used to determine the movement direction (e.g., player head)
    public CapsuleCollider bodyCollider;
    private Vector2 inputMoveAxis;
    private float inputTurnAxis;

    public bool onlyMoveWhenGrounded = false;

    public Transform turnSource; // Used to apply the turn rotation
    public LayerMask groundLayer;

    private bool isGrounded;

    void Update()
    {
        // Get the input from the Meta Quest controllers using OVRInput
        inputMoveAxis = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);  // Left thumbstick for movement
        inputTurnAxis = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick).x;  // Right thumbstick for turning

        // Check for the jump input using the "A" button on the right controller
        bool jumpInput = OVRInput.GetDown(OVRInput.Button.One);  // "A" button on the right Meta Quest controller

        // If jump button is pressed and the player is grounded, apply the jump force
        if (jumpInput && isGrounded)
        {
            jumpVelocity = Mathf.Sqrt(2 * -Physics.gravity.y * jumpHeight);
            rb.velocity = Vector3.up * jumpVelocity;
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
            Vector3 direction = yaw * new Vector3(inputMoveAxis.x, 0, inputMoveAxis.y);

            // Move the Rigidbody based on calculated direction
            Vector3 targetMovePosition = rb.position + direction * Time.fixedDeltaTime * speed;

            // Handle rotation using the right thumbstick
            if (Mathf.Abs(inputTurnAxis) > 0.1f)
            {
                float turnAngle = inputTurnAxis * turnSpeed * Time.fixedDeltaTime;
                Quaternion rotation = Quaternion.Euler(0, turnAngle, 0);

                rb.MoveRotation(rb.rotation * rotation);

                // Apply rotation adjustment to the movement to maintain relative positioning
                Vector3 newPosition = rotation * (targetMovePosition - turnSource.position) + turnSource.position;
                rb.MovePosition(newPosition);
            }
            else
            {
                rb.MovePosition(targetMovePosition);
            }
        }
    }

    // Function to check if the player is grounded using a sphere cast
    public bool CheckIfGrounded()
    {
        Vector3 start = bodyCollider.transform.TransformPoint(bodyCollider.center);
        float rayLength = bodyCollider.height / 2 - bodyCollider.radius + 0.05f;

        return Physics.SphereCast(start, bodyCollider.radius, Vector3.down, out RaycastHit hitInfo, rayLength, groundLayer);
    }
}
