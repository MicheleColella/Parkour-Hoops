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
    public InputActionProperty pullInputAction1;
    public InputActionProperty pullInputAction2;

    [Header("Hand Grabbing Reference")]
    public GrabPhysics grabPhysics;

    [Header("Animate Hand On Input Reference")]
    public AnimateHandOnInput animateHandOnInput; // Added reference

    [Header("Pull Restrictions")]
    public float maxPullableMass = 10f;

    [Header("Attraction Time")]
    public float maxAttractionTime = 2f; // Maximum time for attraction, configurable in Inspector

    [Header("Prefab Settings")]
    public GameObject prefabToInstantiate;
    public Vector3 prefabScale = Vector3.one;

    [Header("Prefab Movement")]
    public float prefabSmoothTime = 0.05f;

    [Header("Debug")]
    [ReadOnly]
    public bool isAttracting = false;

    [HideInInspector]
    public Rigidbody attractedObject;
    private List<Rigidbody> objectsInTrigger = new List<Rigidbody>();

    private Rigidbody currentCandidateObject = null;
    private GameObject instantiatedPrefab = null;

    private Vector3 prefabVelocity = Vector3.zero;

    private float attractionDuration = 0f;
    private bool hasAttractionTimedOut = false;

    void OnEnable()
    {
        pullInputAction1.action.Enable();
        pullInputAction2.action.Enable();
    }

    void OnDisable()
    {
        pullInputAction1.action.Disable();
        pullInputAction2.action.Disable();
    }

    void Update()
    {
        if (grabPhysics != null && grabPhysics.isGrabbing)
        {
            if (isAttracting)
            {
                StopAttracting();
            }
            DestroyInstantiatedPrefab();
            return;
        }

        bool pullButtonHeld1 = pullInputAction1.action.ReadValue<float>() > 0.1f;
        bool pullButtonHeld2 = pullInputAction2.action.ReadValue<float>() > 0.1f;

        // Reset the timeout flag if input is released
        if (!pullButtonHeld1 || !pullButtonHeld2)
        {
            hasAttractionTimedOut = false;
            if (isAttracting)
            {
                StopAttracting();
            }
        }

        UpdateCurrentCandidateObject();

        if (pullButtonHeld1 && pullButtonHeld2 && !hasAttractionTimedOut)
        {
            // Check if pulling is disabled
            if (animateHandOnInput != null && animateHandOnInput.isGrabbingDisabled)
            {
                //Debug.Log("[PullObjectTrigger] Pulling is disabled due to input values.");
                return;
            }

            if (currentCandidateObject != null && !isAttracting)
            {
                attractedObject = currentCandidateObject;
                isAttracting = true;
                attractionDuration = 0f; // Reset attraction timer
                attractedObject.useGravity = false;

                ImpactSoundAndEffect impactScript = attractedObject.GetComponent<ImpactSoundAndEffect>();
                if (impactScript != null)
                {
                    impactScript.isAttracting = true;  // Disable impact
                }

                DestroyInstantiatedPrefab();
            }
        }
        else
        {
            if (isAttracting)
            {
                StopAttracting();
            }
        }
    }

    void LateUpdate()
    {
        if (instantiatedPrefab != null && currentCandidateObject != null)
        {
            instantiatedPrefab.transform.position = Vector3.SmoothDamp(
                instantiatedPrefab.transform.position,
                currentCandidateObject.position,
                ref prefabVelocity,
                prefabSmoothTime
            );
        }
    }

    void FixedUpdate()
    {
        if (isAttracting && attractedObject != null)
        {
            attractionDuration += Time.fixedDeltaTime; // Increment the attraction duration

            GrabbableObject grabbable = attractedObject.GetComponentInParent<GrabbableObject>();
            if (grabbable != null && grabbable.isGrabbed)
            {
                StopAttracting();
                return;
            }

            Vector3 direction = (handOrigin.position - attractedObject.position);
            attractedObject.velocity = direction.normalized * attractionSpeed;

            if (direction.magnitude < 0.1f)
            {
                StopAttracting();
                return;
            }

            // Stop attracting if duration exceeds the configured maxAttractionTime
            if (attractionDuration > maxAttractionTime)
            {
                StopAttracting();
                hasAttractionTimedOut = true; // Prevent re-attraction until input is released
                return;
            }
        }
    }

    private void UpdateCurrentCandidateObject()
    {
        Rigidbody closestObject = GetClosestCandidateObject();

        if (closestObject != currentCandidateObject && (animateHandOnInput == null || !animateHandOnInput.isGrabbingDisabled))
        {
            DestroyInstantiatedPrefab();
            currentCandidateObject = closestObject;

            if (currentCandidateObject != null)
            {
                InstantiatePrefabAtCandidateObject();
            }
        }
        else if (currentCandidateObject != null)
        {
            GrabbableObject grabbable = currentCandidateObject.GetComponentInParent<GrabbableObject>();
            if (grabbable == null || !grabbable.canBePulled || grabbable.isGrabbed || currentCandidateObject.mass > maxPullableMass)
            {
                DestroyInstantiatedPrefab();
                currentCandidateObject = null;
            }
        }
    }

    private Rigidbody GetClosestCandidateObject()
    {
        Rigidbody closest = null;
        float minDistance = Mathf.Infinity;

        foreach (Rigidbody obj in objectsInTrigger)
        {
            GrabbableObject grabbable = obj.GetComponentInParent<GrabbableObject>();
            if (grabbable == null || !grabbable.canBePulled || grabbable.isGrabbed || obj.mass > maxPullableMass)
            {
                continue;
            }

            if (isAttracting && obj == attractedObject)
            {
                continue;
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

    private void InstantiatePrefabAtCandidateObject()
    {
        if (prefabToInstantiate != null && currentCandidateObject != null)
        {
            instantiatedPrefab = Instantiate(prefabToInstantiate, currentCandidateObject.position, Quaternion.identity);
            instantiatedPrefab.transform.localScale = prefabScale;
        }
    }

    private void DestroyInstantiatedPrefab()
    {
        if (instantiatedPrefab != null)
        {
            Destroy(instantiatedPrefab);
            instantiatedPrefab = null;
            prefabVelocity = Vector3.zero;
        }
    }

    public void StopAttracting()
    {
        if (attractedObject != null)
        {
            isAttracting = false;
            attractedObject.useGravity = true;
            attractedObject.velocity = Vector3.zero; // Ensure velocity is zero

            ImpactSoundAndEffect impactScript = attractedObject.GetComponent<ImpactSoundAndEffect>();
            if (impactScript != null)
            {
                impactScript.isAttracting = false;  // Reset isAttracting
            }

            attractedObject = null;
            attractionDuration = 0f; // Reset the timer
        }
    }

    public bool HasObjectsInTrigger()
    {
        foreach (Rigidbody obj in objectsInTrigger)
        {
            GrabbableObject grabbable = obj.GetComponentInParent<GrabbableObject>();
            if (grabbable != null && grabbable.canBePulled && !grabbable.isGrabbed && obj.mass <= maxPullableMass)
            {
                return true;
            }
        }
        return false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & targetLayer) != 0)
        {
            Rigidbody rb = other.attachedRigidbody;
            if (rb != null && !objectsInTrigger.Contains(rb))
            {
                objectsInTrigger.Add(rb);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        Rigidbody rb = other.attachedRigidbody;
        if (rb != null && objectsInTrigger.Contains(rb))
        {
            if (rb == attractedObject)
            {
                StopAttracting();
            }

            if (rb == currentCandidateObject)
            {
                DestroyInstantiatedPrefab();
                currentCandidateObject = null;
            }

            objectsInTrigger.Remove(rb);
        }
    }
}
