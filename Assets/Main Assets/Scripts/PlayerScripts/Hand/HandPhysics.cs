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
    public float contactDamping = 0.8f; // Smorzamento durante il contatto
    public float stopThreshold = 0.05f; // Soglia per considerare la mano reale "ferma"
    public float rotationSmoothing = 0.1f; // Fattore di smoothing per la rotazione
    public float distanceTolerance = 0.01f; // Soglia di tolleranza per fermare le correzioni continue
    public float angleTolerance = 2f; // Soglia angolare per fermare le correzioni rotazionali
    public float collisionFriction = 0.95f; // Frizione aggiuntiva quando la mano è in contatto con una superficie
    public float rotationAngleRange = 20f; // Range di tolleranza angolare per evitare jittering

    [Header("Teleport Settings")]
    public float teleportThreshold = 0.2f; // Soglia di distanza per il teletrasporto

    [Header("Climbing Settings")]
    public LayerMask climbableLayers; // Layer arrampicabili
    public float minClimbAngle = 0f; // Angolo minimo (0 gradi è verso l'alto)
    public float maxClimbAngle = 120f; // Angolo massimo

    [HideInInspector]
    public bool isGrabbing = false;

    public Transform ControllerTransform
    {
        get { return controllerTransform; }
    }

    public Transform PhysicalHandTransform
    {
        get { return this.transform; }
    }

    private Rigidbody rb;
    private bool isColliding = false;
    private Vector3 collisionNormal; // La normale della superficie di collisione
    private Quaternion initialLocalRotation; // Memorizza la rotazione iniziale della mano fisica rispetto al controller

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

        // Memorizza la rotazione iniziale locale rispetto al controller
        initialLocalRotation = Quaternion.Inverse(controllerTransform.rotation) * transform.rotation;
    }

    void FixedUpdate()
    {
        // Calcola la posizione e rotazione target con offset
        Vector3 targetPosition = controllerTransform.position + positionOffset;
        Quaternion targetRotation = controllerTransform.rotation * initialLocalRotation * Quaternion.Euler(rotationOffset);
        
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

        // Muovi la mano fisica solo se la distanza è superiore alla tolleranza
        if (distance > distanceTolerance)
        {
            Vector3 desiredVelocity = (targetPosition - rb.position) / Time.fixedDeltaTime;
            desiredVelocity = Vector3.ClampMagnitude(desiredVelocity, maxVelocity);
            rb.velocity = desiredVelocity;
        }
        else
        {
            // Ferma la mano fisica se è sufficientemente vicina alla destinazione
            rb.velocity = Vector3.zero;
        }

        // Calcola l'errore angolare tra la mano fisica e la rotazione target
        Quaternion rotationDelta = targetRotation * Quaternion.Inverse(rb.rotation);
        rotationDelta.ToAngleAxis(out float angle, out Vector3 axis);

        // Applica la rotazione solo se l'angolo è superiore alla soglia di tolleranza
        if (Mathf.Abs(angle) > angleTolerance)
        {
            Vector3 desiredAngularVelocity = axis.normalized * Mathf.Deg2Rad * angle / Time.fixedDeltaTime;
            desiredAngularVelocity = Vector3.ClampMagnitude(desiredAngularVelocity, maxAngularVelocity);
            rb.angularVelocity = desiredAngularVelocity;
        }
        else
        {
            // Ferma la rotazione se è abbastanza allineata
            rb.angularVelocity = Vector3.zero;
        }

        // Se la mano è in contatto con una superficie, applica lo smorzamento e frizione
        if (isColliding)
        {
            rb.velocity *= contactDamping;
            rb.velocity *= collisionFriction;  // Applica una frizione aggiuntiva per ridurre lo scivolamento

            // Impedisci che la velocità diventi troppo bassa e causi jittering
            if (rb.velocity.magnitude < stopThreshold)
            {
                rb.velocity = Vector3.zero;
            }

            // Calcola l'angolo tra la rotazione corrente e la rotazione target
            float angleBetweenRotations = Quaternion.Angle(rb.rotation, targetRotation);

            // Se l'angolo è all'interno del range di tolleranza, evita correzioni brusche
            if (angleBetweenRotations < rotationAngleRange)
            {
                Quaternion smoothRotation = Quaternion.Slerp(rb.rotation, targetRotation, rotationSmoothing);
                rb.rotation = smoothRotation;
            }
        }
    }

    // Rileva la collisione con qualsiasi oggetto fisico
    private void OnCollisionEnter(Collision collision)
    {
        isColliding = true;
        collisionNormal = collision.contacts[0].normal; // Memorizza la normale della superficie
        rb.angularVelocity = Vector3.zero;  // Ferma la rotazione per evitare jittering immediato

        Debug.Log($"{gameObject.name} OnCollisionEnter with {collision.gameObject.name}");

        // Verifica se ha colliso con una superficie arrampicabile
        if (IsClimbable(collision))
        {
            isGrabbing = true;
            Debug.Log($"{gameObject.name} started grabbing {collision.gameObject.name}");
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        isColliding = false;
        collisionNormal = Vector3.zero; // Resetta la normale quando non c'è collisione

        Debug.Log($"{gameObject.name} OnCollisionExit with {collision.gameObject.name}");

        // Verifica se era in contatto con una superficie arrampicabile
        
            isGrabbing = false;
            Debug.Log($"{gameObject.name} stopped grabbing {collision.gameObject.name}");
        
    }

    bool IsClimbable(Collision collision)
    {
        // Verifica se l'oggetto di collisione è in un layer arrampicabile
        bool inLayer = ((1 << collision.gameObject.layer) & climbableLayers.value) != 0;

        if (inLayer)
        {
            // Controllo opzionale: verifica l'angolo tra la normale della superficie e il vettore up
            foreach (ContactPoint contact in collision.contacts)
            {
                float angle = Vector3.Angle(contact.normal, Vector3.up);
                if (angle >= minClimbAngle && angle <= maxClimbAngle)
                {
                    return true;
                }
            }
        }
        return false;
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

            if (isColliding)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawRay(transform.position, collisionNormal); // Visualizza la normale della collisione
            }
        }
    }
}
