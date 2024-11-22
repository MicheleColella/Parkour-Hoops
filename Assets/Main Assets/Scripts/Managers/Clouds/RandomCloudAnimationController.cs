using UnityEngine;

public class RandomCloudAnimationController : MonoBehaviour
{
    [Header("Velocità di Rotazione")]
    [Tooltip("Intervallo casuale per la velocità di rotazione.")]
    public Vector2 rotationSpeedRange = new Vector2(0.5f, 2f);

    [Header("Nomi delle Animazioni")]
    [Tooltip("Nome dell'animazione per la rotazione a destra.")]
    public string rotateRightAnimation = "RotateRight";
    [Tooltip("Nome dell'animazione per la rotazione a sinistra.")]
    public string rotateLeftAnimation = "RotateLeft";

    private Animator animator;
    private float currentRotationSpeed;

    void Awake()
    {
        // Ottieni il componente Animator all'inizio
        animator = GetComponent<Animator>();

        if (animator == null)
        {
            Debug.LogError("Nessun Animator trovato su questo oggetto. Aggiungi un Animator per utilizzare le animazioni.");
        }
    }

    void OnEnable()
    {
        // Gestisce il caso in cui l'oggetto viene attivato
        PlayRandomRotation();
    }

    void Start()
    {
        // Gestisce il caso in cui l'oggetto viene istanziato o è già presente nella scena
        PlayRandomRotation();
    }

    /// <summary>
    /// Sceglie casualmente e avvia un'animazione di rotazione (destra o sinistra) con una velocità casuale.
    /// </summary>
    public void PlayRandomRotation()
    {
        if (animator == null) return;

        // Genera una velocità casuale basata sul range
        currentRotationSpeed = Random.Range(rotationSpeedRange.x, rotationSpeedRange.y);

        // Genera un valore casuale per scegliere l'animazione
        int randomChoice = Random.Range(0, 2); // 0 o 1

        // Avvia l'animazione corrispondente
        if (randomChoice == 0)
        {
            animator.Play(rotateRightAnimation);
            //Debug.Log($"Animazione: Rotazione a Destra con velocità {currentRotationSpeed}");
        }
        else
        {
            animator.Play(rotateLeftAnimation);
            //Debug.Log($"Animazione: Rotazione a Sinistra con velocità {currentRotationSpeed}");
        }

        // Imposta la velocità dell'animazione
        animator.SetFloat("Speed", currentRotationSpeed);
    }

    /// <summary>
    /// Cambia dinamicamente l'intervallo di velocità.
    /// </summary>
    public void SetRotationSpeedRange(Vector2 newRange)
    {
        rotationSpeedRange = newRange;
    }

    /// <summary>
    /// Ritorna l'attuale velocità di rotazione usata.
    /// </summary>
    public float GetCurrentRotationSpeed()
    {
        return currentRotationSpeed;
    }
}
