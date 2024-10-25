using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GrabPhysics : MonoBehaviour
{
    public InputActionProperty grabInputSource;
    public LayerMask grabLayer;
    public List<Collider> handColliders; // List of hand colliders, assign in Inspector
    public Animator handAnimator; // Reference to the hand's animator
    public List<Collider> fingerTipColliders; // List of fingertip colliders
    public float grabValueSpeed = 1.0f; // Speed of GrabValue increment

    // Public field for the grab range collider
    public Collider grabRangeCollider; // Assign the trigger collider manually

    // Variables for the prefab instantiation
    public GameObject prefabToInstantiate; // The prefab to instantiate
    public Vector3 prefabScale = Vector3.one; // Scale of the prefab, set in Inspector

    // Reference point to find the closest object
    public Transform referencePoint; // Assign the reference point in the Inspector

    private FixedJoint fixedJoint;
    private bool isGrabbing = false;
    private Collider grabbedObjectCollider; // Collider of the grabbed object
    private float grabValue = 0f; // Current "GrabValue"
    private bool objectTouchedByFingers = false; // Monitor if the object is touched by fingertip colliders

    // Variables for managing the candidate object and instantiated prefab
    private Collider currentCandidateObject = null; // The current candidate object for grabbing
    private GameObject instantiatedPrefab = null; // Reference to the instantiated prefab

    private void FixedUpdate()
    {
        bool isGrabButtonPressed = grabInputSource.action.ReadValue<float>() > 0.1f;

        // Manage candidate object and prefab instantiation only when not grabbing
        if (!isGrabbing)
        {
            // Use the grabRangeCollider's bounds to find nearby colliders
            if (grabRangeCollider != null)
            {
                Collider[] nearbyColliders = Physics.OverlapBox(
                    grabRangeCollider.bounds.center,
                    grabRangeCollider.bounds.extents,
                    grabRangeCollider.transform.rotation,
                    grabLayer,
                    QueryTriggerInteraction.Ignore);

                // Exclude self and hand colliders, and find the closest object to the reference point
                Collider targetCollider = null;
                float closestDistance = Mathf.Infinity;

                foreach (Collider collider in nearbyColliders)
                {
                    if (collider.gameObject != gameObject && !handColliders.Contains(collider))
                    {
                        // Calculate distance to the reference point
                        float distance = Vector3.Distance(referencePoint.position, collider.transform.position);
                        if (distance < closestDistance)
                        {
                            closestDistance = distance;
                            targetCollider = collider;
                        }
                    }
                }

                // Check if the candidate object has changed
                if (targetCollider != currentCandidateObject)
                {
                    // Candidate has changed
                    // Destroy the previous instantiated prefab, if any
                    if (instantiatedPrefab != null)
                    {
                        Destroy(instantiatedPrefab);
                        instantiatedPrefab = null;
                    }

                    currentCandidateObject = targetCollider;

                    if (currentCandidateObject != null)
                    {
                        // Check if the candidate object's Rigidbody is not kinematic before instantiating the prefab
                        Rigidbody rb = currentCandidateObject.attachedRigidbody;
                        if (rb != null && !rb.isKinematic)
                        {
                            // Instantiate the prefab at the candidate object's position
                            if (prefabToInstantiate != null)
                            {
                                instantiatedPrefab = Instantiate(prefabToInstantiate, currentCandidateObject.transform.position, currentCandidateObject.transform.rotation);
                                instantiatedPrefab.transform.localScale = prefabScale;
                                // Do not parent the prefab to the candidate object
                            }
                            else
                            {
                                Debug.LogWarning("Prefab to instantiate is not assigned.");
                            }
                        }
                    }
                }
            }
            else
            {
                Debug.LogWarning("Grab Range Collider is not assigned.");
            }

            // If there is no candidate object and instantiatedPrefab is not null, destroy it
            if (currentCandidateObject == null && instantiatedPrefab != null)
            {
                Destroy(instantiatedPrefab);
                instantiatedPrefab = null;
            }
        }
        else
        {
            // When grabbing, ensure that the instantiatedPrefab is destroyed
            if (instantiatedPrefab != null)
            {
                Destroy(instantiatedPrefab);
                instantiatedPrefab = null;
            }
            currentCandidateObject = null;
        }

        // Update the position of the instantiated prefab to match the candidate object's position
        if (instantiatedPrefab != null && currentCandidateObject != null)
        {
            instantiatedPrefab.transform.position = currentCandidateObject.transform.position;
            instantiatedPrefab.transform.rotation = currentCandidateObject.transform.rotation;
        }

        // Grabbing logic
        if (isGrabButtonPressed && !isGrabbing)
        {
            if (currentCandidateObject != null)
            {
                Rigidbody targetRigidbody = currentCandidateObject.attachedRigidbody;

                // Ignore collisions between each hand collider and the grabbed object
                grabbedObjectCollider = currentCandidateObject;
                if (handColliders != null && grabbedObjectCollider != null)
                {
                    foreach (Collider handCollider in handColliders)
                    {
                        Physics.IgnoreCollision(handCollider, grabbedObjectCollider, true);
                    }
                }

                fixedJoint = gameObject.AddComponent<FixedJoint>();
                fixedJoint.autoConfigureConnectedAnchor = true; // Auto-configure the anchor to maintain the object's original position

                if (targetRigidbody)
                {
                    fixedJoint.connectedBody = targetRigidbody;
                }

                isGrabbing = true;
                StartCoroutine(IncreaseGrabValue()); // Start gradually increasing GrabValue

                // Destroy the instantiated prefab as we are grabbing the object
                if (instantiatedPrefab != null)
                {
                    Destroy(instantiatedPrefab);
                    instantiatedPrefab = null;
                }
                currentCandidateObject = null;
            }
        }
        else if (!isGrabButtonPressed && isGrabbing)
        {
            isGrabbing = false;

            // Restore collisions between the hand and the released object
            if (handColliders != null && grabbedObjectCollider != null)
            {
                foreach (Collider handCollider in handColliders)
                {
                    Physics.IgnoreCollision(handCollider, grabbedObjectCollider, false);
                }
                grabbedObjectCollider = null; // Reset the reference to the grabbed object
            }

            if (fixedJoint)
            {
                Destroy(fixedJoint);
            }

            StopAllCoroutines(); // Stop increasing GrabValue
            ResetGrabValue(); // Reset the value of GrabValue
        }
    }

    // Coroutine to gradually increase GrabValue at adjustable speed
    private IEnumerator IncreaseGrabValue()
    {
        while (grabValue < 1f && !objectTouchedByFingers)
        {
            grabValue += Time.deltaTime * grabValueSpeed; // Gradually increase GrabValue based on the set speed
            handAnimator.SetFloat("GrabValue", grabValue); // Update the "GrabValue" parameter in the animator

            // Check if any of the fingertip colliders are touching the grabbed object
            foreach (Collider fingerTipCollider in fingerTipColliders)
            {
                if (fingerTipCollider.bounds.Intersects(grabbedObjectCollider.bounds))
                {
                    objectTouchedByFingers = true;
                    break;
                }
            }

            yield return null;
        }
    }

    // Function to reset GrabValue
    private void ResetGrabValue()
    {
        grabValue = 0f;
        objectTouchedByFingers = false;
        handAnimator.SetFloat("GrabValue", grabValue);
    }

    // Optional: Function to draw the collider bounds in the Scene view
    private void OnDrawGizmos()
    {
        if (grabRangeCollider == null)
            return;

        Gizmos.color = Color.yellow;
        // Draw the collider bounds
        Gizmos.matrix = grabRangeCollider.transform.localToWorldMatrix;
        if (grabRangeCollider is BoxCollider)
        {
            BoxCollider boxCollider = grabRangeCollider as BoxCollider;
            Gizmos.DrawWireCube(boxCollider.center, boxCollider.size);
        }
        else if (grabRangeCollider is SphereCollider)
        {
            SphereCollider sphereCollider = grabRangeCollider as SphereCollider;
            Gizmos.DrawWireSphere(sphereCollider.center, sphereCollider.radius);
        }
        // Add other collider types as needed
    }
}
