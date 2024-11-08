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
    public GrabPhysics grabPhysics;

    [Header("Pull Restrictions")]
    public float maxPullableMass = 10f;

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
        if (grabPhysics != null && grabPhysics.isGrabbing)
        {
            if (isAttracting)
            {
                StopAttracting();
            }
            DestroyInstantiatedPrefab();
            return;
        }

        bool pullButtonHeld = pullInputAction.action.ReadValue<float>() > 0.1f;

        UpdateCurrentCandidateObject();

        if (pullButtonHeld)
        {
            if (currentCandidateObject != null && !isAttracting)
            {
                attractedObject = currentCandidateObject;
                isAttracting = true;
                attractedObject.useGravity = false;

                ImpactSoundAndEffect impactScript = attractedObject.GetComponent<ImpactSoundAndEffect>();
                if (impactScript != null)
                {
                    impactScript.isAttracting = true;  // Imposta isAttracting a true per disabilitare l'impatto
                }

                DestroyInstantiatedPrefab();

                Debug.Log("Iniziato ad attirare l'oggetto: " + attractedObject.name);
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
                Debug.Log("Oggetto raggiunto l'origine della mano: " + attractedObject.name);
                StopAttracting();
            }
        }
        else if (attractedObject != null)
        {
            attractedObject.velocity = Vector3.zero;
        }
    }

    private void UpdateCurrentCandidateObject()
    {
        Rigidbody closestObject = GetClosestCandidateObject();

        if (closestObject != currentCandidateObject)
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
            attractedObject.velocity = Vector3.zero;

            ImpactSoundAndEffect impactScript = attractedObject.GetComponent<ImpactSoundAndEffect>();
            if (impactScript != null)
            {
                impactScript.isAttracting = false;  // Reimposta isAttracting a false
            }

            attractedObject = null;
            Debug.Log("Smetti di attirare l'oggetto.");
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
