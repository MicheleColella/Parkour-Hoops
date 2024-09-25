using UnityEngine;

public class ClimbingColliderAdjuster : MonoBehaviour
{
    [Header("References")]
    public CapsuleCollider playerCollider;             // Riferimento al CapsuleCollider del giocatore
    public ControllerCollisionDetector leftController;  // Riferimento allo script del controller sinistro
    public ControllerCollisionDetector rightController; // Riferimento allo script del controller destro
    public Transform playerCamera;                      // Riferimento alla testa del giocatore (CenterEyeAnchor)

    [Header("Collider Settings")]
    public float adjustedHeight = 1.0f;      // Altezza del collider durante la scalata e il salto
    // Rimosso jumpAdjustedHeight
    public float adjustSpeed = 2.0f;         // Velocità di regolazione dell'altezza

    public float groundCheckRadius = 0.1f;  // Raggio della sfera di controllo del terreno
    public Vector3 groundPosOffset = new Vector3(0, 0.05f, 0); // Offset per evitare sovrapposizioni col capsule collider

    [Header("Ground Check")]
    public LayerMask groundLayers;          // LayerMask per specificare i layer del terreno
    public bool isTouchingGround = false;   // Indica se il giocatore tocca il pavimento

    private float originalHeight;
    private Vector3 originalCenter;

    private float targetHeight;
    private Vector3 targetCenter;

    private float restoreDelayTimer = 0f;           // Timer per il ritardo nel ripristino dell'altezza
    public float restoreDelayDuration = 1.0f;       // Durata del ritardo (in secondi)

    void Start()
    {
        if (playerCollider == null)
        {
            playerCollider = GetComponent<CapsuleCollider>();
        }

        // Memorizza l'altezza e il centro originali del CapsuleCollider
        originalHeight = playerCollider.height;
        originalCenter = playerCollider.center;

        // Inizializza i valori target
        targetHeight = originalHeight;
        targetCenter = originalCenter;
    }

    void Update()
    {
        // Verifica se il giocatore è a terra e aggiorna il booleano isTouchingGround
        isTouchingGround = IsGrounded();

        // Verifica se uno dei controller sta toccando una superficie
        bool isControllerTouchingSurface = leftController.isTouchingSurface || rightController.isTouchingSurface;

        // Determina se il giocatore è in aria e non sta arrampicando
        bool isInAir = !isTouchingGround && !isControllerTouchingSurface;

        if (isTouchingGround)
        {
            // Reset del timer di ritardo
            restoreDelayTimer = 0f;

            // Ripristina l'altezza del collider
            RestoreColliderHeight();
        }
        else if (isControllerTouchingSurface || isInAir)
        {
            // Reset del timer di ritardo
            restoreDelayTimer = 0f;

            // Regola l'altezza del collider per l'arrampicata e il salto
            AdjustColliderHeight(adjustedHeight);
        }
        else
        {
            // Incrementa il timer di ritardo
            restoreDelayTimer += Time.deltaTime;

            if (restoreDelayTimer >= restoreDelayDuration)
            {
                // Ripristina l'altezza del collider dopo il ritardo
                RestoreColliderHeight();
            }
        }

        // Interpola gradualmente l'altezza e il centro del collider verso i valori target
        playerCollider.height = Mathf.Lerp(playerCollider.height, targetHeight, Time.deltaTime * adjustSpeed);
        playerCollider.center = Vector3.Lerp(playerCollider.center, targetCenter, Time.deltaTime * adjustSpeed);

        // Aggiorna il centro del collider per seguire la posizione della testa negli assi X e Z
        Vector3 localHeadPos = transform.InverseTransformPoint(playerCamera.position);
        targetCenter.x = localHeadPos.x;
        targetCenter.z = localHeadPos.z;

        playerCollider.center = new Vector3(targetCenter.x, playerCollider.center.y, targetCenter.z);
    }

    public bool IsGrounded()
    {
        // Calcola la posizione della base del CapsuleCollider e aggiungi un piccolo offset verso il basso per evitare sovrapposizione con il capsule collider
        Vector3 bottom = transform.position + playerCollider.center - Vector3.up * (playerCollider.height / 2f);
        Vector3 groundCheckPosition = bottom + groundPosOffset; // Aggiungi l'offset per evitare sovrapposizioni

        // Verifica se l'OverlapSphere tocca il terreno, usando il LayerMask configurabile dall'Inspector
        return Physics.CheckSphere(groundCheckPosition, groundCheckRadius, groundLayers, QueryTriggerInteraction.Ignore);
    }

    private void AdjustColliderHeight(float newHeight)
    {
        // Imposta l'altezza target
        targetHeight = newHeight;

        // Calcola l'adattamento del centro per accorciare dal basso
        float heightDifference = originalHeight - newHeight;
        targetCenter.y = originalCenter.y + (heightDifference / 2f);
    }

    private void RestoreColliderHeight()
    {
        // Imposta l'altezza e il centro target ai valori originali
        targetHeight = originalHeight;
        targetCenter.y = originalCenter.y;
    }

    private void OnDrawGizmos()
    {
        if (playerCollider != null)
        {
            Gizmos.color = Color.green;

            // Calcola le posizioni top e bottom del CapsuleCollider
            Vector3 colliderWorldCenter = transform.TransformPoint(playerCollider.center);
            float height = playerCollider.height;
            float radius = playerCollider.radius;
            float halfHeight = Mathf.Max(0, (height / 2f) - radius);

            Vector3 top = colliderWorldCenter + transform.up * halfHeight;
            Vector3 bottom = colliderWorldCenter - transform.up * halfHeight;

            // Disegna il CapsuleCollider
            Gizmos.DrawWireSphere(top, radius);
            Gizmos.DrawWireSphere(bottom, radius);

            // Disegna linee che collegano le sfere
            Gizmos.DrawLine(top + transform.right * radius, bottom + transform.right * radius);
            Gizmos.DrawLine(top - transform.right * radius, bottom - transform.right * radius);
            Gizmos.DrawLine(top + transform.forward * radius, bottom + transform.forward * radius);
            Gizmos.DrawLine(top - transform.forward * radius, bottom - transform.forward * radius);

            // Disegna l'OverlapSphere per il controllo del terreno
            Gizmos.color = Color.red;
            Vector3 groundCheckPosition = transform.position + playerCollider.center - Vector3.up * (playerCollider.height / 2f) + groundPosOffset;
            Gizmos.DrawWireSphere(groundCheckPosition, groundCheckRadius);
        }
    }
}
