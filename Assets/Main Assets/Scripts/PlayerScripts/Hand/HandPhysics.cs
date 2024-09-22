using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class HandPhysics : MonoBehaviour
{
    [Header("Controller Settings")]
    public Transform controllerTransform; // Posizione del controller
    public Vector3 positionOffset = Vector3.zero; // Offset di posizione
    public Vector3 rotationOffset = Vector3.zero; // Offset di rotazione

    [Header("Movement Settings")]
    public float maxVelocity = 10f; // Velocità massima
    public float maxAngularVelocity = 10f; // Velocità angolare massima

    [Header("Teleport Settings")]
    public float teleportThreshold = 0.2f; // Soglia di distanza per il teletrasporto

    private Rigidbody rb;
    private bool isColliding = false;
    private Vector3 collisionNormal;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.constraints = RigidbodyConstraints.None;
        rb.mass = 0.1f;
        rb.drag = 0f;
        rb.angularDrag = 0f;
    }

    void FixedUpdate()
    {
        // Calcola la posizione e rotazione target con offset
        Vector3 targetPosition = controllerTransform.position + positionOffset;
        Quaternion targetRotation = Quaternion.Euler(controllerTransform.eulerAngles + rotationOffset);
        
        // Calcola la distanza tra la mano fisica e il controller
        float distance = Vector3.Distance(transform.position, targetPosition);

        // Se la distanza supera la soglia, teletrasporta la mano fisica
        if (distance > teleportThreshold)
        {
            rb.position = targetPosition;
            rb.rotation = targetRotation;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            return;
        }

        // Calcola la velocità desiderata per seguire il controller
        Vector3 desiredVelocity = (targetPosition - rb.position) / Time.fixedDeltaTime;
        desiredVelocity = Vector3.ClampMagnitude(desiredVelocity, maxVelocity);
        rb.velocity = desiredVelocity;

        // Calcola la velocità angolare desiderata per allineare la rotazione
        Quaternion rotationDelta = targetRotation * Quaternion.Inverse(rb.rotation);
        rotationDelta.ToAngleAxis(out float angle, out Vector3 axis);
        if (angle > 180f)
        {
            angle -= 360f;
        }
        Vector3 desiredAngularVelocity = axis.normalized * Mathf.Deg2Rad * angle / Time.fixedDeltaTime;
        desiredAngularVelocity = Vector3.ClampMagnitude(desiredAngularVelocity, maxAngularVelocity);
        rb.angularVelocity = desiredAngularVelocity;

        // Prevenzione della penetrazione negli oggetti
        if (isColliding)
        {
            Vector3 relativeVelocity = rb.velocity;
            float dot = Vector3.Dot(relativeVelocity, collisionNormal);
            if (dot < 0)
            {
                rb.velocity -= collisionNormal * dot; // Rimuove la componente di velocità verso il muro
            }
        }
    }

    // Rileva la collisione con qualsiasi oggetto fisico
    private void OnCollisionEnter(Collision collision)
    {
        isColliding = true;
        collisionNormal = collision.contacts[0].normal;
    }

    private void OnCollisionExit(Collision collision)
    {
        isColliding = false;
    }

    // Disegna i Gizmos per debug
    void OnDrawGizmos()
    {
        if (controllerTransform != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(controllerTransform.position, transform.position); // Linea tra controller e mano fisica
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, teleportThreshold); // Soglia di teletrasporto
        }
    }
}
