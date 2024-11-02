// PullObjectTrigger.cs
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class PullObjectTrigger : MonoBehaviour
{
    [Header("Pull Settings")]
    public float attractionSpeed = 5f;
    public LayerMask targetLayer;
    public Transform handOrigin;

    [Header("Input Action")]
    public InputActionProperty pullInputAction;

    [Header("Hand Grabbing Reference")]
    public GrabPhysics grabPhysics; // Reference to the hand's GrabPhysics script

    [Header("Debug")]
    public bool isAttracting = false;

    public Rigidbody attractedObject;
    private List<Rigidbody> objectsInTrigger = new List<Rigidbody>();

    void OnEnable()
    {
        pullInputAction.action.Enable();
    }

    void OnDisable()
    {
        pullInputAction.action.Disable();
    }

    void Update()
    {
        // Check if the hand is currently grabbing an object
        if (grabPhysics != null && grabPhysics.isGrabbing)
        {
            // The hand is grabbing an object, so do not attract other objects
            if (isAttracting)
            {
                StopAttracting();
            }
            return;
        }

        bool pullButtonHeld = pullInputAction.action.ReadValue<float>() > 0.1f;

        if (pullButtonHeld)
        {
            if (objectsInTrigger.Count > 0 && !isAttracting)
            {
                // Find the closest object to the handOrigin
                attractedObject = GetClosestObject();
                if (attractedObject != null)
                {
                    // Check if the object is not kinematic and not already grabbed
                    GrabbableObject grabbable = attractedObject.GetComponent<GrabbableObject>();
                    if (!attractedObject.isKinematic && (grabbable == null || !grabbable.isGrabbed))
                    {
                        isAttracting = true;
                        attractedObject.useGravity = false;

                        Debug.Log("Started attracting object: " + attractedObject.name);
                    }
                    else
                    {
                        Debug.Log("Cannot attract object: " + attractedObject.name);
                        attractedObject = null;
                    }
                }
            }
        }
        else
        {
            // If the pull button is not held, stop attracting
            if (isAttracting)
            {
                StopAttracting();
            }
        }

        // Debug visual
        if (isAttracting && attractedObject != null)
        {
            Debug.DrawLine(attractedObject.position, handOrigin.position, Color.red);
        }
    }

    void FixedUpdate()
    {
        if (isAttracting && attractedObject != null)
        {
            // Check if the object is grabbed during attraction
            GrabbableObject grabbable = attractedObject.GetComponent<GrabbableObject>();
            if (grabbable != null && grabbable.isGrabbed)
            {
                StopAttracting();
                return;
            }

            // Move the object towards the handOrigin
            Vector3 direction = (handOrigin.position - attractedObject.position);
            attractedObject.velocity = direction.normalized * attractionSpeed;

            // If the object is close enough, stop attracting
            if (direction.magnitude < 0.1f)
            {
                StopAttracting();
                Debug.Log("Object reached handOrigin: " + attractedObject.name);
            }
        }
        else if (attractedObject != null)
        {
            // Ensure the object stops moving if not attracting
            attractedObject.velocity = Vector3.zero;
        }
    }

    public void StopAttracting()
    {
        if (attractedObject != null)
        {
            isAttracting = false;
            attractedObject.useGravity = true;
            attractedObject.velocity = Vector3.zero;
            attractedObject = null;
            Debug.Log("Stopped attracting object.");
        }
    }

    private Rigidbody GetClosestObject()
    {
        Rigidbody closest = null;
        float minDistance = Mathf.Infinity;

        foreach (Rigidbody obj in objectsInTrigger)
        {
            // Check if the object is grabbed
            GrabbableObject grabbable = obj.GetComponent<GrabbableObject>();
            if (grabbable != null && grabbable.isGrabbed)
            {
                continue; // Skip objects that are already grabbed
            }

            float distance = Vector3.Distance(obj.position, handOrigin.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closest = obj;
            }
        }

        return closest;
    }

    public bool HasObjectsInTrigger()
    {
        // Return true if there is at least one object in the trigger that is not grabbed
        foreach (Rigidbody obj in objectsInTrigger)
        {
            GrabbableObject grabbable = obj.GetComponent<GrabbableObject>();
            if (grabbable == null || !grabbable.isGrabbed)
            {
                return true;
            }
        }
        return false;
    }

    void OnTriggerEnter(Collider other)
    {
        // Check if the object is in the target layer
        if (((1 << other.gameObject.layer) & targetLayer) != 0)
        {
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb != null)
            {
                objectsInTrigger.Add(rb);
                Debug.Log("Object entered trigger: " + other.name);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb != null && objectsInTrigger.Contains(rb))
        {
            // If the object we are attracting exits the trigger, stop attracting
            if (rb == attractedObject)
            {
                StopAttracting();
                Debug.Log("Attracted object exited trigger.");
            }

            objectsInTrigger.Remove(rb);
            Debug.Log("Object exited trigger: " + other.name);
        }
    }
}
