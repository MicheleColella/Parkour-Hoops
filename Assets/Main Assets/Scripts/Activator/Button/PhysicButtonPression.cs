using UnityEngine;
using UnityEngine.Events;

public class PhysicButtonPression : MonoBehaviour
{
    [Tooltip("Oggetto che attiverà l'evento quando entrerà in contatto con il trigger.")]
    public GameObject targetObject;

    [Tooltip("Azione da eseguire quando il trigger tocca l'oggetto specificato.")]
    public UnityEvent onPressAction;

    [Tooltip("Altezza locale minima che l'oggetto può raggiungere.")]
    public float minLocalHeight = 0f;

    [Tooltip("Altezza locale massima che l'oggetto può raggiungere.")]
    public float maxLocalHeight = 1f;

    [ReadOnly]
    [Tooltip("Altezza locale attuale dell'oggetto.")]
    public float currentLocalHeight;

    private float initialLocalHeight;
    private Rigidbody rb;

    private void Start()
    {
        // Assegna l'altezza iniziale dell'oggetto
        initialLocalHeight = transform.localPosition.y;

        // Assegna l'altezza massima all'altezza iniziale
        maxLocalHeight = initialLocalHeight;

        // Ottiene il componente Rigidbody dell'oggetto
        rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            // Blocca i movimenti dell'oggetto sugli assi X e Z
            rb.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;

            // Blocca la rotazione su tutti gli assi
            rb.constraints |= RigidbodyConstraints.FreezeRotation;
        }
    }

    private void Update()
    {
        // Limita l'altezza locale dell'oggetto tra minLocalHeight e maxLocalHeight
        Vector3 localPosition = transform.localPosition;
        localPosition.y = Mathf.Clamp(localPosition.y, minLocalHeight, maxLocalHeight);
        transform.localPosition = localPosition;

        // Aggiorna l'altezza locale attuale
        currentLocalHeight = transform.localPosition.y;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Controlla se l'oggetto che entra in contatto è quello assegnato
        if (other.gameObject == targetObject)
        {
            // Esegue l'azione assegnata nell'Inspector
            Debug.Log("Pulsante premuto");
            onPressAction?.Invoke();
        }
    }
}
