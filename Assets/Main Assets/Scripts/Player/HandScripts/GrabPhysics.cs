using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GrabPhysics : MonoBehaviour
{
    [Header("Input Settings")]
    public InputActionProperty grabInputSource;

    [Header("Grabbing Settings")]
    public LayerMask grabLayer;
    public List<Collider> handColliders;
    public Animator handAnimator;
    public List<Collider> fingerTipColliders;
    public float grabValueSpeed = 1.0f;

    [Header("Grab Range")]
    public Collider grabRangeCollider;

    [Header("Prefab Settings")]
    public GameObject prefabToInstantiate;
    public Vector3 prefabScale = Vector3.one;
    public float prefabFollowSpeed = 100f;

    [Header("Reference Point")]
    public Transform referencePoint;

    private FixedJoint fixedJoint;
    private bool isGrabbing = false;
    private Collider grabbedObjectCollider;
    private float grabValue = 0f;
    private bool objectTouchedByFingers = false;

    private Collider currentCandidateObject = null;
    private GameObject instantiatedPrefab = null;

    private void FixedUpdate()
    {
        bool isGrabButtonPressed = grabInputSource.action.ReadValue<float>() > 0.1f;

        if (!isGrabbing)
        {
            UpdateCandidateObject();

            if (isGrabButtonPressed && !isGrabbing)
            {
                TryGrabObject();
            }
        }
        else
        {
            DestroyInstantiatedPrefab();
        }

        if (!isGrabButtonPressed && isGrabbing)
        {
            ReleaseGrab();
        }
    }

    private void LateUpdate()
    {
        if (instantiatedPrefab != null && currentCandidateObject != null)
        {
            float step = prefabFollowSpeed * Time.deltaTime;
            instantiatedPrefab.transform.position = Vector3.MoveTowards(
                instantiatedPrefab.transform.position,
                currentCandidateObject.transform.position,
                step
            );
        }
    }

    private void UpdateCandidateObject()
    {
        if (grabRangeCollider == null)
        {
            Debug.LogWarning("Grab Range Collider is not assigned.");
            return;
        }

        Collider[] nearbyColliders = Physics.OverlapBox(
            grabRangeCollider.bounds.center,
            grabRangeCollider.bounds.extents,
            grabRangeCollider.transform.rotation,
            grabLayer,
            QueryTriggerInteraction.Ignore
        );

        Collider closestCollider = null;
        float closestDistance = Mathf.Infinity;

        foreach (Collider collider in nearbyColliders)
        {
            if (collider.gameObject == gameObject || handColliders.Contains(collider))
                continue;

            float distance = Vector3.Distance(referencePoint.position, collider.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestCollider = collider;
            }
        }

        if (closestCollider != currentCandidateObject)
        {
            currentCandidateObject = closestCollider;
            DestroyInstantiatedPrefab();

            if (currentCandidateObject != null)
            {
                Rigidbody rb = currentCandidateObject.attachedRigidbody;
                if (rb != null && !rb.isKinematic)
                {
                    if (prefabToInstantiate != null)
                    {
                        instantiatedPrefab = Instantiate(prefabToInstantiate, currentCandidateObject.transform.position, Quaternion.identity);
                        instantiatedPrefab.transform.localScale = prefabScale;
                    }
                    else
                    {
                        Debug.LogWarning("Prefab to instantiate is not assigned.");
                    }
                }
            }
        }

        if (currentCandidateObject == null && instantiatedPrefab != null)
        {
            DestroyInstantiatedPrefab();
        }
    }

    private void TryGrabObject()
    {
        if (currentCandidateObject == null) return;

        Rigidbody targetRigidbody = currentCandidateObject.attachedRigidbody;
        if (targetRigidbody == null) return;

        grabbedObjectCollider = currentCandidateObject;
        foreach (Collider handCollider in handColliders)
        {
            Physics.IgnoreCollision(handCollider, grabbedObjectCollider, true);
        }

        fixedJoint = gameObject.AddComponent<FixedJoint>();
        fixedJoint.autoConfigureConnectedAnchor = true;
        fixedJoint.connectedBody = targetRigidbody;

        isGrabbing = true;
        StartCoroutine(IncreaseGrabValue());

        DestroyInstantiatedPrefab();
        currentCandidateObject = null;
    }

    private void ReleaseGrab()
    {
        isGrabbing = false;

        if (grabbedObjectCollider != null)
        {
            foreach (Collider handCollider in handColliders)
            {
                Physics.IgnoreCollision(handCollider, grabbedObjectCollider, false);
            }
            grabbedObjectCollider = null;
        }

        if (fixedJoint != null)
        {
            Destroy(fixedJoint);
        }

        StopAllCoroutines();
        ResetGrabValue();
    }

    private void DestroyInstantiatedPrefab()
    {
        if (instantiatedPrefab != null)
        {
            Destroy(instantiatedPrefab);
            instantiatedPrefab = null;
        }
    }

    private IEnumerator IncreaseGrabValue()
    {
        while (grabValue < 1f && !objectTouchedByFingers)
        {
            grabValue += Time.deltaTime * grabValueSpeed;
            handAnimator.SetFloat("GrabValue", grabValue);

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

    private void ResetGrabValue()
    {
        grabValue = 0f;
        objectTouchedByFingers = false;
        handAnimator.SetFloat("GrabValue", grabValue);
    }

    private void OnDrawGizmosSelected()
    {
        if (grabRangeCollider == null)
            return;

        Gizmos.color = Color.yellow;
        Gizmos.matrix = grabRangeCollider.transform.localToWorldMatrix;

        if (grabRangeCollider is BoxCollider boxCollider)
        {
            Gizmos.DrawWireCube(boxCollider.center, boxCollider.size);
        }
        else if (grabRangeCollider is SphereCollider sphereCollider)
        {
            Gizmos.DrawWireSphere(sphereCollider.center, sphereCollider.radius);
        }
    }
}
