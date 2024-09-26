using UnityEngine;

public class ControllerCollisionDetector : MonoBehaviour
{
    [Header("Settings")]
    public LayerMask climbableLayers; // LayerMask per le superfici arrampicabili
    public float maxSurfaceAngle = 60f; // Angolo massimo dalla verticale per considerare la superficie arrampicabile

    [HideInInspector]
    public bool isTouchingSurface = false;

    private void OnTriggerStay(Collider other)
    {
        if (IsClimbable(other))
        {
            isTouchingSurface = true;
            return;
        }

        isTouchingSurface = false;
    }

    private void OnTriggerExit(Collider other)
    {
        if (IsClimbable(other))
        {
            isTouchingSurface = false;
        }
    }

    private bool IsClimbable(Collider collider)
    {
        if (IsLayerInLayerMask(collider.gameObject.layer, climbableLayers))
        {
            // Ottieni la normale della superficie
            RaycastHit hit;
            Vector3 direction = collider.ClosestPoint(transform.position) - transform.position;

            if (Physics.Raycast(transform.position, direction, out hit, 0.2f, climbableLayers))
            {
                float angle = Vector3.Angle(hit.normal, Vector3.up);

                if (angle <= maxSurfaceAngle)
                {
                    return true;
                }
            }
        }
        return false;
    }

    // Funzione helper per verificare se un layer è in un LayerMask
    private bool IsLayerInLayerMask(int layer, LayerMask layerMask)
    {
        return ((layerMask.value & (1 << layer)) != 0);
    }
}
