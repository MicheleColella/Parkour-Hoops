using System.Collections.Generic;
using UnityEngine;

public class TeleportOnTrigger : MonoBehaviour
{
    [System.Serializable]
    public struct TeleportLocation
    {
        public string tag;            // Il tag associato a questa posizione di teletrasporto
        public Transform location;    // La posizione di teletrasporto
    }

    [SerializeField] private List<TeleportLocation> teleportLocations; // Lista delle posizioni di teletrasporto
    [SerializeField] private Transform defaultTeleport;               // Posizione di teletrasporto di default

    private void OnTriggerEnter(Collider other)
    {
        // Controlla se l'oggetto ha un Rigidbody
        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb != null)
        {
            // Trova la posizione di teletrasporto associata al tag dell'oggetto
            Transform teleportTarget = defaultTeleport;

            foreach (var location in teleportLocations)
            {
                if (other.CompareTag(location.tag))
                {
                    teleportTarget = location.location;
                    break;
                }
            }

            // Teletrasporta l'oggetto alla posizione selezionata
            rb.velocity = Vector3.zero; // Resetta la velocità
            rb.angularVelocity = Vector3.zero; // Resetta anche la velocità angolare
            other.transform.position = teleportTarget.position; // Posiziona l'oggetto
        }
    }
}
