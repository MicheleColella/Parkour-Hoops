using UnityEngine;

public class FollowPlayerYAxis : MonoBehaviour
{
    public Transform player; // Il riferimento al giocatore
    public float followSpeed = 5f; // Velocità con cui l'oggetto segue il giocatore
    private float initialYOffset; // Offset iniziale sull'asse Y
    private float highestY; // Valore Y più alto raggiunto dall'oggetto
    public bool shouldFollow = true; // Indica se l'oggetto deve seguire il giocatore

    private void Start()
    {
        if (player == null)
        {
            Debug.LogError("Il riferimento al giocatore non è stato assegnato.");
            return;
        }

        // Calcola l'offset iniziale tra l'oggetto e il giocatore
        initialYOffset = transform.position.y - player.position.y;

        // Inizializza `highestY` con la posizione attuale dell'oggetto
        highestY = transform.position.y;
    }

    private void Update()
    {
        if (player == null || !shouldFollow) return;

        // Calcola la posizione target mantenendo l'offset iniziale
        float targetY = player.position.y + initialYOffset;

        // Se il target Y è maggiore dell'attuale posizione più alta
        if (targetY > highestY)
        {
            // Interpola verso la nuova posizione più alta
            float newY = Mathf.Lerp(transform.position.y, targetY, followSpeed * Time.deltaTime);

            // Aggiorna la posizione dell'oggetto mantenendo X e Z invariati
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);

            // Aggiorna il valore della posizione Y più alta
            highestY = transform.position.y;
        }
    }
}
