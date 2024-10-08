using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContinuousMovementPhysics : MonoBehaviour
{
    public float speed = 10f;  // Aumenta la velocità per compensare l'uso di AddForce
    public float maxSpeed = 5f; // Velocità massima del player
    public float jumpForce = 7f;
    public float jumpHeight = 1.5f;

    public bool onlyMoveWhenGrounded = false;

    public Rigidbody rb;
    public LayerMask groundLayer;

    public Transform directionSource;

    public CapsuleCollider bodyCollider;

    private Vector2 inputMoveAxis;

    private bool isGrounded;

    private void Start()
    {
        // Abilita l'interpolazione per migliorare la fluidità del movimento
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    void Update()
    {
        // Raccogli input solo in Update, perché Update è chiamato a ogni frame
        inputMoveAxis = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);

        bool jumpInput = OVRInput.GetDown(OVRInput.Button.One);

        if (jumpInput && isGrounded)
        {
            // Usa AddForce per il salto
            float jumpVelocity = Mathf.Sqrt(2 * -Physics.gravity.y * jumpHeight);
            rb.AddForce(Vector3.up * jumpVelocity, ForceMode.VelocityChange);
        }
    }

    private void FixedUpdate()
    {
        isGrounded = CheckIfGrounded();

        if (!onlyMoveWhenGrounded || (onlyMoveWhenGrounded && isGrounded))
        {
            // Movimento fluido utilizzando AddForce
            Quaternion yaw = Quaternion.Euler(0, directionSource.eulerAngles.y, 0);
            Vector3 direction = yaw * new Vector3(inputMoveAxis.x, 0, inputMoveAxis.y);

            if (direction.magnitude > 0.1f)  // Applica il movimento solo se l'input è significativo
            {
                // Controlla la velocità attuale del player
                Vector3 currentVelocity = rb.velocity;
                Vector3 horizontalVelocity = new Vector3(currentVelocity.x, 0, currentVelocity.z);

                // Se la velocità è inferiore alla velocità massima, applica la forza
                if (horizontalVelocity.magnitude < maxSpeed)
                {
                    // Applica la forza nella direzione del movimento
                    rb.AddForce(direction.normalized * speed, ForceMode.Force);
                }
            }
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
