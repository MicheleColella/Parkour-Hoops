using UnityEngine;
using System.Collections.Generic;

public class GrabbableObject : MonoBehaviour
{
    [Tooltip("Indicates if the object is currently being grabbed.")]
    public bool isGrabbed
    {
        get { return grabbedBy.Count > 0; }
    }

    [Tooltip("References to the hands that are grabbing this object.")]
    public List<GameObject> grabbedBy = new List<GameObject>();

    [Header("Pull Settings")]
    [Tooltip("Indicates if the object can be pulled by the PullObjectTrigger.")]
    public bool canBePulled = true;

    private Rigidbody rb;
    private TrailRenderer trailRenderer;
    private Outline outline;

    void Awake()
    {
        // Cerca il Rigidbody nel GameObject corrente o nei suoi genitori
        rb = GetComponentInParent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogWarning("GrabbableObject requires a Rigidbody component in parent or self.");
        }

        // Cerca il TrailRenderer nel GameObject corrente
        trailRenderer = GetComponent<TrailRenderer>();

        // Cerca il componente Outline nel GameObject corrente
        outline = GetComponent<Outline>();
    }

    /// <summary>
    /// Call this method when the object is grabbed.
    /// </summary>
    /// <param name="hand">The hand GameObject that grabs this object.</param>
    public void OnGrabbed(GameObject hand)
    {
        if (!grabbedBy.Contains(hand))
        {
            grabbedBy.Add(hand);

            // Disattiva il TrailRenderer se è presente
            if (trailRenderer != null)
            {
                trailRenderer.enabled = false;
            }

            // Disattiva il componente Outline se è presente
            if (outline != null)
            {
                outline.enabled = false;
            }

            // If the object is being pulled, stop pulling
            PullObjectTrigger[] pullTriggers = FindObjectsOfType<PullObjectTrigger>();
            foreach (var pullTrigger in pullTriggers)
            {
                if (pullTrigger.isAttracting && pullTrigger.attractedObject == rb)
                {
                    pullTrigger.StopAttracting();
                }
            }
        }
    }

    /// <summary>
    /// Call this method when the object is released.
    /// </summary>
    /// <param name="hand">The hand GameObject that releases this object.</param>
    public void OnReleased(GameObject hand)
    {
        if (grabbedBy.Contains(hand))
        {
            grabbedBy.Remove(hand);

            // Riattiva il TrailRenderer se è presente e se non è più afferrato
            if (grabbedBy.Count == 0 && trailRenderer != null)
            {
                trailRenderer.enabled = true;
            }

            // Riattiva il componente Outline se è presente e se non è più afferrato
            if (grabbedBy.Count == 0 && outline != null)
            {
                outline.enabled = true;
            }
        }
    }
}
