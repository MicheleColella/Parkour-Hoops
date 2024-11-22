using UnityEngine;

public class EndGameTrigger : MonoBehaviour
{
    [Header("GameObjects")]
    [Tooltip("Il GameObject da teletrasportare.")]
    public GameObject objectToTeleport;

    [Tooltip("Il target dove il GameObject verrà teletrasportato.")]
    public Transform targetPosition;

    [Tooltip("Il tag del Player che attiverà il teletrasporto.")]
    public string playerTag = "Player";

    [Tooltip("Il GameObject che verrà rimosso dalla gerarchia.")]
    public GameObject endGamePlatform;

    private void OnTriggerEnter(Collider other)
    {
        // Controlla se l'oggetto che entra nel trigger ha il tag specificato
        if (other.CompareTag(playerTag))
        {
            Debug.Log("PlayerEnd");

            if (objectToTeleport != null && targetPosition != null)
            {
                // Rimuove la piattaforma dalla gerarchia del Player
                if (endGamePlatform != null && endGamePlatform.transform.parent != null)
                {
                    endGamePlatform.transform.parent = null;
                    Debug.Log($"{endGamePlatform.name} è stato scollegato dal suo parent.");
                }
                else
                {
                    Debug.LogWarning("EndGamePlatform non è assegnato o non ha un parent.");
                }

                // Teletrasporta l'oggetto alla posizione del target
                objectToTeleport.transform.position = targetPosition.position;

                // Controlla se l'oggetto ha un Rigidbody
                Rigidbody rb = objectToTeleport.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    // Azzerare la velocità del Rigidbody
                    rb.velocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero; // Azzerare anche la velocità angolare
                    Debug.Log("La velocità del Rigidbody è stata azzerata.");
                }

                Debug.Log($"{objectToTeleport.name} è stato teletrasportato a {targetPosition.position}");
            }
            else
            {
                Debug.LogWarning("Oggetto o Target non assegnato nell'Inspector!");
            }
        }
    }
}
