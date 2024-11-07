using UnityEngine;
using UnityEngine.Events;

public class PhysicButtonPression : MonoBehaviour
{
    [Tooltip("Oggetto che attiverà l'evento quando entrerà in contatto con il trigger.")]
    public GameObject targetObject;

    [Tooltip("Azione da eseguire quando il trigger tocca l'oggetto specificato.")]
    public UnityEvent onPressAction;

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
