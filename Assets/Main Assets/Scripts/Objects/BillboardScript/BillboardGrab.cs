using UnityEngine;

public class BillboardGrab : MonoBehaviour
{
    [Header("Target Settings")]
    [Tooltip("Layer mask for target GameObjects to billboard towards.")]
    public LayerMask targetLayer;

    [Tooltip("Radius within which to search for target GameObjects.")]
    public float searchRadius = 100f;

    void Update()
    {
        // Trova tutti i collider all'interno del raggio di ricerca e nel layer specificato
        Collider[] targetColliders = Physics.OverlapSphere(transform.position, searchRadius, targetLayer);

        if (targetColliders.Length > 0)
        {
            // Ottieni la posizione del primo target
            Vector3 targetPosition = targetColliders[0].transform.position;

            // Calcola la direzione dal billboard al target
            Vector3 direction = targetPosition - transform.position;

            // Ruota il billboard per guardare verso il target
            transform.rotation = Quaternion.LookRotation(-direction);
        }
    }

    // Visualizza il raggio di ricerca nell'Editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, searchRadius);
    }
}
