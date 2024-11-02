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

    // Add any other useful parameters here
    [Header("Additional Settings")]
    [Tooltip("Enable or disable gravity when the object is grabbed.")]
    public bool disableGravityOnGrab = true;

    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogWarning("GrabbableObject requires a Rigidbody component.");
        }
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

            // If this is the first hand grabbing the object, disable gravity
            if (grabbedBy.Count == 1 && disableGravityOnGrab && rb != null)
            {
                rb.useGravity = false;
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

            // If no hands are grabbing the object anymore, re-enable gravity
            if (grabbedBy.Count == 0 && disableGravityOnGrab && rb != null)
            {
                rb.useGravity = true;
            }
        }
    }
}
