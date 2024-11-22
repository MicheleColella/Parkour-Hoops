using UnityEngine;

public class ObjectFollower : MonoBehaviour
{
    public Transform player;             // Riferimento al Transform del player
    public float initialDistance = 5f;   // Distanza iniziale da mantenere dal player
    public bool moveUpwardAtSpeed = false; // Bool per attivare/disattivare il movimento verso l'alto
    public float upwardSpeed = 2f;       // Velocità a cui l'oggetto si muove verso l'alto

    private float offsetY;               // Offset verticale iniziale tra l'oggetto e il player

    void Start()
    {
        // Calcola l'offset iniziale in Y
        offsetY = transform.position.y - player.position.y;
    }

    void Update()
    {
        if (moveUpwardAtSpeed)
        {
            // Calcola la distanza attuale in Y tra l'oggetto e il player
            float currentDistance = Mathf.Abs(transform.position.y - player.position.y);

            if (currentDistance > initialDistance)
            {
                // Segue il player fino a quando la distanza non è minore o uguale a quella iniziale
                Vector3 targetPosition = transform.position;
                targetPosition.y = player.position.y + offsetY;
                transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * upwardSpeed);
            }
            else
            {
                // Si muove verso l'alto a una certa velocità
                transform.Translate(Vector3.up * upwardSpeed * Time.deltaTime);
            }
        }
        else
        {
            // Segue il player mantenendo la distanza iniziale
            Vector3 newPosition = transform.position;
            newPosition.y = player.position.y + offsetY;
            transform.position = newPosition;
        }
    }
}
