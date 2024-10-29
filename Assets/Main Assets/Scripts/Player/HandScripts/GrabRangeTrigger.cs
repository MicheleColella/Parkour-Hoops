using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabRangeTrigger : MonoBehaviour
{
    [Header("Grabbing Settings")]
    public LayerMask grabLayer;

    // Lista per tenere traccia degli oggetti nel raggio di presa
    private List<Collider> candidateObjects = new List<Collider>();

    // Evento chiamato quando un oggetto entra nel trigger
    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & grabLayer) != 0)
        {
            // Aggiungi alla lista dei candidati se non è già presente
            if (!candidateObjects.Contains(other))
            {
                candidateObjects.Add(other);
            }
        }
    }

    // Evento chiamato quando un oggetto esce dal trigger
    private void OnTriggerExit(Collider other)
    {
        if (((1 << other.gameObject.layer) & grabLayer) != 0)
        {
            // Rimuovi dalla lista dei candidati
            if (candidateObjects.Contains(other))
            {
                candidateObjects.Remove(other);
            }
        }
    }

    // Metodo pubblico per ottenere gli oggetti candidati
    public List<Collider> GetCandidateObjects()
    {
        return candidateObjects;
    }

    // Metodo per visualizzare il trigger nell'Editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Collider grabCollider = GetComponent<Collider>();
        if (grabCollider == null)
            return;

        Gizmos.matrix = grabCollider.transform.localToWorldMatrix;

        if (grabCollider is BoxCollider boxCollider)
        {
            Gizmos.DrawWireCube(boxCollider.center, boxCollider.size);
        }
        else if (grabCollider is SphereCollider sphereCollider)
        {
            Gizmos.DrawWireSphere(sphereCollider.center, sphereCollider.radius);
        }
        // Aggiungi altri tipi di collider se necessario
    }
}
