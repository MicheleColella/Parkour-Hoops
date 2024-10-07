using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContinuousMovementPhysics : MonoBehaviour
{
    public float speed = 1;
    public float turnSpeed = 60;
    private float jumpVelocity = 7;
    public float jumpHeight = 1.5f;

    public bool onlyMoveWhenGrounded = false;

    public Rigidbody rb;
    public LayerMask groundLayer;

    public Transform directionSource;
    public Transform turnSource;

    public CapsuleCollider bodyCollider;

    private Vector2 inputMoveAxis;
    private float inputTurnAxis;

    private bool isGrounded;

    // Update is called once per frame
    void Update()
    {
        inputMoveAxis = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);
        inputTurnAxis = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick).x;

        bool jumpInput = OVRInput.GetDown(OVRInput.Button.One);

        if (jumpInput && isGrounded)
        {
            jumpVelocity = Mathf.Sqrt(2 * -Physics.gravity.y * jumpHeight);
            rb.velocity = Vector3.up * jumpVelocity;
        }
    }

    private void FixedUpdate()
    {
        isGrounded = CheckIfGrounded();

        if (!onlyMoveWhenGrounded || (onlyMoveWhenGrounded && isGrounded))
        {
            Quaternion yaw = Quaternion.Euler(0, directionSource.eulerAngles.y, 0);
            Vector3 direction = yaw * new Vector3(inputMoveAxis.x, 0, inputMoveAxis.y);

            Vector3 targetMovePosition = rb.position + direction * Time.fixedDeltaTime * speed;

            Vector3 axis = Vector3.up;
            float angle = turnSpeed * Time.fixedDeltaTime * inputTurnAxis;

            Quaternion q = Quaternion.AngleAxis(angle, axis);

            rb.MoveRotation(rb.rotation * q);

            Vector3 newPosition = q * (targetMovePosition - turnSource.position) + turnSource.position;

            rb.MovePosition(newPosition);
        }
    }

    public bool CheckIfGrounded()
    {
        Vector3 start = bodyCollider.transform.TransformPoint(bodyCollider.center);
        float rayLength = bodyCollider.height / 2 - bodyCollider.radius + 0.05f;

        bool hasHit = Physics.SphereCast(start, bodyCollider.radius, Vector3.down, out RaycastHit hitInfo, rayLength, groundLayer);

        return hasHit;
    }
}
