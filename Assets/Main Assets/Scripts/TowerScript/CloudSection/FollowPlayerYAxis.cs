using UnityEngine;

public class FollowPlayerYAxis : MonoBehaviour
{
    public Transform player; // Il riferimento al giocatore
    public float followSpeed = 5f; // Velocità con cui l'oggetto segue il giocatore
    private float initialYOffset; // Offset iniziale sull'asse Y

    private void Start()
    {
        if (player == null)
        {
            Debug.LogError("Il riferimento al giocatore non è stato assegnato.");
            return;
        }

        // Calcola la distanza iniziale sull'asse Y tra l'oggetto e il giocatore
        initialYOffset = transform.position.y - player.position.y;
    }

    private void Update()
    {
        if (player == null) return;

        // Calcola la posizione target mantenendo l'offset iniziale
        float targetY = player.position.y + initialYOffset;

        // Interpola la posizione attuale verso il target in modo fluido
        float newY = Mathf.Lerp(transform.position.y, targetY, followSpeed * Time.deltaTime);

        // Aggiorna la posizione dell'oggetto mantenendo X e Z invariati
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
}
