using UnityEngine;
using System.Collections;

public class ClimbingColliderAdjuster : MonoBehaviour
{
    [Header("References")]
    public CapsuleCollider playerCollider;             // Riferimento al CapsuleCollider del giocatore
    public ControllerCollisionDetector leftController;  // Riferimento allo script del controller sinistro
    public ControllerCollisionDetector rightController; // Riferimento allo script del controller destro

    [Header("Collider Settings")]
    public float adjustedHeight = 1.0f;      // Altezza del collider durante la scalata
    public float adjustSpeed = 2.0f;         // Velocità di regolazione dell'altezza del collider

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
        // Verifica se il giocatore è a terra
        if (IsGrounded())
        {
            // Reset del timer di ritardo
            restoreDelayTimer = 0f;

            // Ripristina immediatamente l'altezza del collider
            RestoreColliderHeight();
        }
        else
        {
            // Verifica se uno dei controller sta toccando una superficie
            bool isControllerTouchingSurface = leftController.isTouchingSurface || rightController.isTouchingSurface;

            if (isControllerTouchingSurface)
            {
                // Reset del timer di ritardo
                restoreDelayTimer = 0f;

                // Regola l'altezza del collider
                AdjustColliderHeight();
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
                else
                {
                    // Mantieni l'altezza corrente del collider
                    // Non cambiare targetHeight o targetCenter
                }
            }
        }

        // Interpola gradualmente l'altezza e il centro del collider verso i valori target
        playerCollider.height = Mathf.Lerp(playerCollider.height, targetHeight, Time.deltaTime * adjustSpeed);
        playerCollider.center = Vector3.Lerp(playerCollider.center, targetCenter, Time.deltaTime * adjustSpeed);
    }

    private void AdjustColliderHeight()
    {
        // Imposta l'altezza target all'altezza regolata
        targetHeight = adjustedHeight;

        // Calcola l'adattamento del centro per accorciare dal basso
        float heightDifference = originalHeight - adjustedHeight;
        targetCenter = originalCenter + new Vector3(0, heightDifference / 2f, 0);
    }

    private void RestoreColliderHeight()
    {
        // Imposta l'altezza e il centro target ai valori originali
        targetHeight = originalHeight;
        targetCenter = originalCenter;
    }

    private bool IsGrounded()
    {
        return GetComponent<MovementController>().IsGrounded();
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
        }
    }
}
