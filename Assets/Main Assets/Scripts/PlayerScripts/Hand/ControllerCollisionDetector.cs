using UnityEngine;

public class ControllerCollisionDetector : MonoBehaviour
{
    [Header("Settings")]
    public LayerMask climbableLayers; // LayerMask per specificare i layer scalabili
    public float maxSurfaceAngle = 15f; // Angolo massimo in gradi dalla verticale per attivare il climbing

    public bool isTouchingSurface = false;

    private Collider controllerCollider;

    private void Awake()
    {
        controllerCollider = GetComponent<Collider>();
    }

    private void OnTriggerStay(Collider other)
    {
        // Verifica se l'oggetto è nei layer scalabili
        if (IsLayerInLayerMask(other.gameObject.layer, climbableLayers))
        {
            // Ottieni il contatto normale usando Physics.ComputePenetration
            Vector3 direction;
            float distance;

            bool isOverlapping = Physics.ComputePenetration(
                controllerCollider, transform.position, transform.rotation,
                other, other.transform.position, other.transform.rotation,
                out direction, out distance);

            if (isOverlapping)
            {
                // La direzione punta dal controller al collider dell'oggetto
                Vector3 contactNormal = direction.normalized; // Corretto qui

                // Calcola l'angolo tra la normale della superficie e l'up vector
                float angle = Vector3.Angle(contactNormal, Vector3.up);

                // Aggiungi debug per monitorare l'angolo
                Debug.Log($"Controller {gameObject.name} sta toccando una superficie. Angolo: {angle}");

                // Se l'angolo è entro il range consentito, abilita il climbing
                if (angle <= maxSurfaceAngle)
                {
                    isTouchingSurface = true;
                    return;
                }
            }
        }

        isTouchingSurface = false;
    }

    private void OnTriggerExit(Collider other)
    {
        if (IsLayerInLayerMask(other.gameObject.layer, climbableLayers))
        {
            isTouchingSurface = false;
            Debug.Log($"Controller {gameObject.name} ha lasciato la superficie scalabile.");
        }
    }

    // Funzione helper per verificare se un layer è in un LayerMask
    private bool IsLayerInLayerMask(int layer, LayerMask layerMask)
    {
        return ((layerMask.value & (1 << layer)) != 0);
    }
}
