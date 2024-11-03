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
    public List<Collider> pinkyTipColliders;
    public List<Collider> ringTipColliders;
    public List<Collider> middleTipColliders;
    public List<Collider> pointerTipColliders;
    public List<Collider> thumbTipColliders;
    public float grabValueSpeed = 1.0f;

    [Header("Reference Point")]
    public Transform referencePoint;

    [Header("Grab Range Trigger")]
    public GrabRangeTrigger grabRangeTrigger;

    private FixedJoint fixedJoint;
    public bool isGrabbing = false;
    private Collider grabbedObjectCollider;
    private GrabbableObject grabbedObjectScript;

    private List<FixedJoint> fixedJoints = new List<FixedJoint>();

    private bool pinkyTouched = false, ringTouched = false, middleTouched = false, pointerTouched = false, thumbTouched = false;
    private float pinkyGrab = 0f, ringGrab = 0f, middleGrab = 0f, pointerGrab = 0f, thumbGrab = 0f;

    private Collider currentCandidateObject = null;

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

        if (!isGrabButtonPressed && isGrabbing)
        {
            ReleaseGrab();
        }
    }

    private void UpdateCandidateObject()
    {
        if (grabRangeTrigger == null)
        {
            Debug.LogWarning("Grab Range Trigger is not assigned.");
            return;
        }

        List<Collider> candidates = grabRangeTrigger.GetCandidateObjects();

        Collider closestCollider = null;
        float closestDistance = Mathf.Infinity;

        foreach (Collider collider in candidates)
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
        }
    }

    private void TryGrabObject()
    {
        if (currentCandidateObject == null) return;

        // Usa GetComponentInParent per trovare GrabbableObject
        GrabbableObject grabbable = currentCandidateObject.GetComponentInParent<GrabbableObject>();
        if (grabbable == null) return;

        Rigidbody targetRigidbody = currentCandidateObject.attachedRigidbody;
        if (targetRigidbody == null) return;

        grabbedObjectCollider = currentCandidateObject;
        grabbedObjectScript = grabbable;
        grabbedObjectScript.OnGrabbed(gameObject); // Pass the hand GameObject

        foreach (Collider handCollider in handColliders)
        {
            Physics.IgnoreCollision(handCollider, grabbedObjectCollider, true);
        }

        fixedJoint = gameObject.AddComponent<FixedJoint>();
        fixedJoint.autoConfigureConnectedAnchor = true;
        fixedJoint.connectedBody = targetRigidbody;

        fixedJoints.Add(fixedJoint);

        isGrabbing = true;

        StartCoroutine(IncreaseFingerGrabValue("Pinky", pinkyTipColliders, pinkyGrab, pinkyTouched));
        StartCoroutine(IncreaseFingerGrabValue("Ring", ringTipColliders, ringGrab, ringTouched));
        StartCoroutine(IncreaseFingerGrabValue("Middle", middleTipColliders, middleGrab, middleTouched));
        StartCoroutine(IncreaseFingerGrabValue("Pointer", pointerTipColliders, pointerGrab, pointerTouched));
        StartCoroutine(IncreaseFingerGrabValue("Thumb", thumbTipColliders, thumbGrab, thumbTouched));

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

            if (grabbedObjectScript != null)
            {
                grabbedObjectScript.OnReleased(gameObject); // Pass the hand GameObject
                grabbedObjectScript = null;
            }

            grabbedObjectCollider = null;
        }

        // Destroy the specific fixed joint associated with this hand
        if (fixedJoint != null)
        {
            fixedJoints.Remove(fixedJoint);
            Destroy(fixedJoint);
            fixedJoint = null;
        }

        StopAllCoroutines();
        ResetFingerGrabValues();
    }

    // Inside your IncreaseFingerGrabValue coroutine
    private IEnumerator IncreaseFingerGrabValue(string fingerName, List<Collider> fingerTipColliders, float grabValue, bool fingerTouched)
    {
        while (grabValue < 1f && !fingerTouched)
        {
            grabValue += Time.deltaTime * grabValueSpeed;
            handAnimator.SetFloat($"{fingerName}Grab", grabValue);

            foreach (Collider fingerTipCollider in fingerTipColliders)
            {
                if (grabbedObjectCollider != null && fingerTipCollider.bounds.Intersects(grabbedObjectCollider.bounds))
                {
                    fingerTouched = true;
                    break;
                }
            }

            // Update the global grab value for the specific finger
            UpdateGlobalGrabValue(fingerName, grabValue, fingerTouched);

            yield return null;
        }
    }


    // Funzione per aggiornare i valori globali di grab
    private void UpdateGlobalGrabValue(string fingerName, float grabValue, bool fingerTouched)
    {
        switch (fingerName)
        {
            case "Pinky":
                pinkyGrab = grabValue;
                pinkyTouched = fingerTouched;
                break;
            case "Ring":
                ringGrab = grabValue;
                ringTouched = fingerTouched;
                break;
            case "Middle":
                middleGrab = grabValue;
                middleTouched = fingerTouched;
                break;
            case "Pointer":
                pointerGrab = grabValue;
                pointerTouched = fingerTouched;
                break;
            case "Thumb":
                thumbGrab = grabValue;
                thumbTouched = fingerTouched;
                break;
        }
    }

    private void ResetFingerGrabValues()
    {
        pinkyGrab = ringGrab = middleGrab = pointerGrab = thumbGrab = 0f;
        pinkyTouched = ringTouched = middleTouched = pointerTouched = thumbTouched = false;

        handAnimator.SetFloat("PinkyGrab", pinkyGrab);
        handAnimator.SetFloat("RingGrab", ringGrab);
        handAnimator.SetFloat("MiddleGrab", middleGrab);
        handAnimator.SetFloat("PointerGrab", pointerGrab);
        handAnimator.SetFloat("ThumbGrab", thumbGrab);
    }

    private void OnDrawGizmos()
    {
        if (grabRangeTrigger == null)
            return;

        Collider grabCollider = grabRangeTrigger.GetComponent<Collider>();
        if (grabCollider == null)
            return;

        Gizmos.color = Color.yellow;
        Gizmos.matrix = grabCollider.transform.localToWorldMatrix;

        if (grabCollider is BoxCollider boxCollider)
        {
            Gizmos.DrawWireCube(boxCollider.center, boxCollider.size);
        }
        else if (grabCollider is SphereCollider sphereCollider)
        {
            Gizmos.DrawWireSphere(sphereCollider.center, sphereCollider.radius);
        }
    }
}
