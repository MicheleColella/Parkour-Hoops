using UnityEngine;

public class ClimbingColliderAdjuster : MonoBehaviour
{
    [Header("References")]
    public CapsuleCollider playerCollider;             // Riferimento al CapsuleCollider del player
    public ControllerCollisionDetector leftController;  // Riferimento allo script del controller sinistro
    public ControllerCollisionDetector rightController; // Riferimento allo script del controller destro

    [Header("Collider Settings")]
    public float adjustedHeight = 1.0f; // Altezza del collider quando si arrampica (impostabile dall'inspector)

    private float originalHeight;
    private Vector3 originalCenter;

    void Start()
    {
        if (playerCollider == null)
        {
            playerCollider = GetComponent<CapsuleCollider>();
        }

        // Memorizza l'altezza e il centro originale del capsule collider
        originalHeight = playerCollider.height;
        originalCenter = playerCollider.center;
    }

    void Update()
    {
        // Verifica se uno o entrambi i controller stanno toccando una superficie
        bool isControllerTouchingSurface = leftController.isTouchingSurface || rightController.isTouchingSurface;

        // Se il player è in aria e almeno un controller tocca una superficie, riduci il collider
        if (!IsGrounded() && isControllerTouchingSurface)
        {
            AdjustColliderHeight();
        }
        else
        {
            RestoreColliderHeight();
        }
    }

    private void AdjustColliderHeight()
    {
        // Riduci l'altezza del collider al valore impostato dall'inspector
        playerCollider.height = adjustedHeight;

        // Modifica il centro per mantenere il top all'altezza originale (accorcia il collider dal basso verso l'alto)
        float heightDifference = originalHeight - adjustedHeight;
        playerCollider.center = originalCenter + new Vector3(0, heightDifference / 2f, 0);
    }

    private void RestoreColliderHeight()
    {
        // Ripristina l'altezza e il centro originali
        playerCollider.height = originalHeight;
        playerCollider.center = originalCenter;
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
            Vector3 colliderWorldCenter = playerCollider.transform.TransformPoint(playerCollider.center);
            float height = playerCollider.height;
            float radius = playerCollider.radius;
            float halfHeight = Mathf.Max(0, (height / 2f) - radius);

            Vector3 top = colliderWorldCenter + playerCollider.transform.up * halfHeight;
            Vector3 bottom = colliderWorldCenter - playerCollider.transform.up * halfHeight;

            // Disegna il CapsuleCollider
            Gizmos.DrawWireSphere(top, radius);
            Gizmos.DrawWireSphere(bottom, radius);

            // Disegna le linee che collegano le sfere
            Gizmos.DrawLine(top + playerCollider.transform.right * radius, bottom + playerCollider.transform.right * radius);
            Gizmos.DrawLine(top - playerCollider.transform.right * radius, bottom - playerCollider.transform.right * radius);
            Gizmos.DrawLine(top + playerCollider.transform.forward * radius, bottom + playerCollider.transform.forward * radius);
            Gizmos.DrawLine(top - playerCollider.transform.forward * radius, bottom - playerCollider.transform.forward * radius);
        }
    }
}
