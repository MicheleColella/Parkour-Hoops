using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContinuousMovementPhysics : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 8f;
    public float acceleration = 30f;
    public float deceleration = 40f;
    public float minJumpHeight = 1.5f;
    public float maxJumpHeight = 4.5f;
    public float maxJumpHoldTime = 3f;
    public bool onlyMoveWhenGrounded = true;

    [Header("Turn Settings")]
    public float snapTurnAngle = 45f;
    public float snapTurnCooldown = 0.5f;

    [Header("References")]
    public Rigidbody rb;
    public Transform directionSource;
    public CapsuleCollider bodyCollider;
    public Transform turnSource;
    public LayerMask groundLayer;

    private Vector2 inputMoveAxis;
    private float inputTurnAxis;
    private bool isGrounded;
    private float lastSnapTurnTime;
    private Vector3 currentVelocity;
    private float jumpHoldTime;
    private bool isJumping;

    void Start()
    {
        lastSnapTurnTime = -snapTurnCooldown;
        jumpHoldTime = 0f;
        isJumping = false;
    }

    void Update() 
    {
        // Recuperiamo input solo se cambiano
        Vector2 controllerMove = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);
        float controllerTurn = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick).x;

        float keyboardHorizontal = Input.GetAxis("Horizontal");
        float keyboardVertical = Input.GetAxis("Vertical");
        float keyboardTurn = Input.GetAxis("Mouse X");

        Vector2 newInputMoveAxis = controllerMove + new Vector2(keyboardHorizontal, keyboardVertical);
        if (newInputMoveAxis.sqrMagnitude > 1)
        {
            newInputMoveAxis.Normalize();
        }

        // Aggiorniamo solo se l'input è cambiato
        if (!Mathf.Approximately(newInputMoveAxis.x, inputMoveAxis.x) || !Mathf.Approximately(newInputMoveAxis.y, inputMoveAxis.y))
        {
            inputMoveAxis = newInputMoveAxis;
        }

        inputTurnAxis = controllerTurn + keyboardTurn;

        bool jumpInputHeld = OVRInput.Get(OVRInput.Button.One) || Input.GetKey(KeyCode.Space);
        bool jumpInputReleased = OVRInput.GetUp(OVRInput.Button.One) || Input.GetKeyUp(KeyCode.Space);

        if (jumpInputHeld && isGrounded)
        {
            jumpHoldTime += Time.deltaTime;
            jumpHoldTime = Mathf.Clamp(jumpHoldTime, 0, maxJumpHoldTime);
        }

        if (jumpInputReleased && isGrounded)
        {
            float jumpPower = Mathf.Lerp(minJumpHeight, maxJumpHeight, jumpHoldTime / maxJumpHoldTime);
            float jumpVelocity = Mathf.Sqrt(2 * jumpPower * Mathf.Abs(Physics.gravity.y));
            rb.velocity = new Vector3(rb.velocity.x, jumpVelocity, rb.velocity.z);
            jumpHoldTime = 0f;
        }

        if (Mathf.Abs(inputTurnAxis) > 0.5f && Time.time - lastSnapTurnTime > snapTurnCooldown)
        {
            float turnDirection = inputTurnAxis > 0 ? 1f : -1f;
            rb.MoveRotation(rb.rotation * Quaternion.Euler(0, snapTurnAngle * turnDirection, 0));
            lastSnapTurnTime = Time.time;
        }
    }

    void FixedUpdate()
    {
        isGrounded = CheckIfGrounded();

        if (!onlyMoveWhenGrounded || (onlyMoveWhenGrounded && isGrounded))
        {
            Quaternion yaw = Quaternion.Euler(0, directionSource.eulerAngles.y, 0);
            Vector3 targetDirection = yaw * new Vector3(inputMoveAxis.x, 0, inputMoveAxis.y).normalized;

            float movementSpeed = (targetDirection.sqrMagnitude > 0 ? acceleration : deceleration) * Time.fixedDeltaTime;
            currentVelocity = Vector3.MoveTowards(currentVelocity, targetDirection * speed, movementSpeed);

            rb.velocity = new Vector3(currentVelocity.x, rb.velocity.y, currentVelocity.z);
        }
    }


    public bool CheckIfGrounded()
    {
        Vector3 start = bodyCollider.transform.TransformPoint(bodyCollider.center);
        float rayLength = bodyCollider.height / 2 - bodyCollider.radius + 0.1f;

        return Physics.SphereCast(start, bodyCollider.radius * 0.9f, Vector3.down, out RaycastHit hitInfo, rayLength, groundLayer);
    } 

    void OnDrawGizmos()
    {
        if (bodyCollider == null)
            return;

        // Imposta il colore del gizmo per il ground check
        Gizmos.color = isGrounded ? Color.green : Color.red;

        // Calcola il punto iniziale del SphereCast
        Vector3 start = bodyCollider.transform.TransformPoint(bodyCollider.center);
        float rayLength = bodyCollider.height / 2 - bodyCollider.radius + 0.1f;

        // Disegna la sfera alla base del capsule collider
        Gizmos.DrawWireSphere(start - new Vector3(0, rayLength, 0), bodyCollider.radius * 0.9f);

        // Imposta il colore del gizmo per il capsule collider
        Gizmos.color = Color.blue;

        // Disegna il capsule collider come due semisfere collegate da un cilindro
        Vector3 topSphere = start + Vector3.up * (bodyCollider.height / 2 - bodyCollider.radius);
        Vector3 bottomSphere = start + Vector3.down * (bodyCollider.height / 2 - bodyCollider.radius);

        Gizmos.DrawWireSphere(topSphere, bodyCollider.radius);
        Gizmos.DrawWireSphere(bottomSphere, bodyCollider.radius);
        Gizmos.DrawLine(topSphere + Vector3.forward * bodyCollider.radius, bottomSphere + Vector3.forward * bodyCollider.radius);
        Gizmos.DrawLine(topSphere - Vector3.forward * bodyCollider.radius, bottomSphere - Vector3.forward * bodyCollider.radius);
        Gizmos.DrawLine(topSphere + Vector3.right * bodyCollider.radius, bottomSphere + Vector3.right * bodyCollider.radius);
        Gizmos.DrawLine(topSphere - Vector3.right * bodyCollider.radius, bottomSphere - Vector3.right * bodyCollider.radius);
    }

}