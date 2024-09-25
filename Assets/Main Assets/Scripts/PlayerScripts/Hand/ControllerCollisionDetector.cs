using UnityEngine;

public class ControllerCollisionDetector : MonoBehaviour
{
    [Header("Settings")]
    public LayerMask climbableLayers; // LayerMask per specificare i layer scalabili

    [HideInInspector]
    public bool isTouchingSurface = false;

    private void OnTriggerEnter(Collider other)
    {
        // Verifica se l'oggetto è nei layer scalabili
        if (IsLayerInLayerMask(other.gameObject.layer, climbableLayers))
        {
            isTouchingSurface = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (IsLayerInLayerMask(other.gameObject.layer, climbableLayers))
        {
            isTouchingSurface = false;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (IsLayerInLayerMask(other.gameObject.layer, climbableLayers))
        {
            isTouchingSurface = true;
        }
    }

    // Funzione helper per verificare se un layer è in un LayerMask
    private bool IsLayerInLayerMask(int layer, LayerMask layerMask)
    {
        return ((layerMask.value & (1 << layer)) > 0);
    }
}
