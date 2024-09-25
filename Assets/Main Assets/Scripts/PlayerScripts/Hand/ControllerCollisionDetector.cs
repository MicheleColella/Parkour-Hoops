using UnityEngine;

public class ControllerCollisionDetector : MonoBehaviour
{
    public bool isTouchingSurface = false;

    void OnTriggerEnter(Collider other)
    {
        // Assicurati che il controller non stia rilevando collisioni con i colliders del player stesso
        if (other.gameObject.layer != LayerMask.NameToLayer("Player"))
        {
            isTouchingSurface = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer("Player"))
        {
            isTouchingSurface = false;
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer("Player"))
        {
            isTouchingSurface = true;
        }
    }
}
