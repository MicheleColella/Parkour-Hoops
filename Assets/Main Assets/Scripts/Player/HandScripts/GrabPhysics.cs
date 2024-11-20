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

    [Header("Animate Hand On Input Reference")]
    public AnimateHandOnInput animateHandOnInput; // Added reference

    private FixedJoint fixedJoint;
    public bool isGrabbing = false;
    private Collider grabbedObjectCollider;
    private GrabbableObject grabbedObjectScript;

    private List<FixedJoint> fixedJoints = new List<FixedJoint>();

    private bool pinkyTouched = false, ringTouched = false, middleTouched = false, pointerTouched = false, thumbTouched = false;
    private float pinkyGrab = 0f, ringGrab = 0f, middleGrab = 0f, pointerGrab = 0f, thumbGrab = 0f;

    private Collider currentCandidateObject = null;

    // Coroutines for each finger
    private Coroutine pinkyCoroutine, ringCoroutine, middleCoroutine, pointerCoroutine, thumbCoroutine;

    private void FixedUpdate()
    {
        bool isGrabButtonPressed = grabInputSource.action.ReadValue<float>() > 0.1f;

        if (!isGrabbing)
        {
            UpdateCandidateObject();

            if (isGrabButtonPressed && !isGrabbing)
            {
                // Check if grabbing is disabled
                if (animateHandOnInput != null && animateHandOnInput.isGrabbingDisabled)
                {
                    //Debug.Log("[GrabPhysics] Grabbing is disabled due to input values.");
                    return;
                }

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

        GrabbableObject grabbable = currentCandidateObject.GetComponentInParent<GrabbableObject>();
        if (grabbable == null) return;

        Rigidbody targetRigidbody = currentCandidateObject.attachedRigidbody;
        if (targetRigidbody == null) return;

        grabbedObjectCollider = currentCandidateObject;
        grabbedObjectScript = grabbable;
        grabbedObjectScript.OnGrabbed(gameObject);

        // Ignore collisions between this hand's colliders and the grabbed object
        foreach (Collider handCollider in handColliders)
        {
            Physics.IgnoreCollision(handCollider, grabbedObjectCollider, true);
        }

        fixedJoint = gameObject.AddComponent<FixedJoint>();
        fixedJoint.autoConfigureConnectedAnchor = true;
        fixedJoint.connectedBody = targetRigidbody;

        fixedJoints.Add(fixedJoint);

        isGrabbing = true;

        pinkyCoroutine = StartCoroutine(IncreaseFingerGrabValue("Pinky", pinkyTipColliders));
        ringCoroutine = StartCoroutine(IncreaseFingerGrabValue("Ring", ringTipColliders));
        middleCoroutine = StartCoroutine(IncreaseFingerGrabValue("Middle", middleTipColliders));
        pointerCoroutine = StartCoroutine(IncreaseFingerGrabValue("Pointer", pointerTipColliders));
        thumbCoroutine = StartCoroutine(IncreaseFingerGrabValue("Thumb", thumbTipColliders));

        currentCandidateObject = null;
    }

    private void ReleaseGrab()
    {
        isGrabbing = false;

        if (grabbedObjectCollider != null)
        {
            // Re-enable collisions between this hand's colliders and the grabbed object
            foreach (Collider handCollider in handColliders)
            {
                Physics.IgnoreCollision(handCollider, grabbedObjectCollider, false);
            }

            if (grabbedObjectScript != null)
            {
                grabbedObjectScript.OnReleased(gameObject);
                grabbedObjectScript = null;
            }

            grabbedObjectCollider = null;
        }

        if (fixedJoint != null)
        {
            fixedJoints.Remove(fixedJoint);
            Destroy(fixedJoint);
            fixedJoint = null;
        }

        if (pinkyCoroutine != null) StopCoroutine(pinkyCoroutine);
        if (ringCoroutine != null) StopCoroutine(ringCoroutine);
        if (middleCoroutine != null) StopCoroutine(middleCoroutine);
        if (pointerCoroutine != null) StopCoroutine(pointerCoroutine);
        if (thumbCoroutine != null) StopCoroutine(thumbCoroutine);

        pinkyCoroutine = ringCoroutine = middleCoroutine = pointerCoroutine = thumbCoroutine = null;

        ResetFingerGrabValues();
    }

    private IEnumerator IncreaseFingerGrabValue(string fingerName, List<Collider> fingerTipColliders)
    {
        while (GetFingerGrabValue(fingerName) < 1f && !GetFingerTouched(fingerName))
        {
            float grabValue = GetFingerGrabValue(fingerName) + Time.deltaTime * grabValueSpeed;
            SetFingerGrabValue(fingerName, grabValue);
            handAnimator.SetFloat($"{fingerName}Grab", grabValue);

            foreach (Collider fingerTipCollider in fingerTipColliders)
            {
                if (grabbedObjectCollider != null)
                {
                    // Only check for intersection with the grabbed object's colliders
                    if (fingerTipCollider.bounds.Intersects(grabbedObjectCollider.bounds))
                    {
                        SetFingerTouched(fingerName, true);
                        break;
                    }
                }
            }

            yield return null;
        }
    }

    private float GetFingerGrabValue(string fingerName)
    {
        switch (fingerName)
        {
            case "Pinky": return pinkyGrab;
            case "Ring": return ringGrab;
            case "Middle": return middleGrab;
            case "Pointer": return pointerGrab;
            case "Thumb": return thumbGrab;
            default: return 0f;
        }
    }

    private void SetFingerGrabValue(string fingerName, float value)
    {
        switch (fingerName)
        {
            case "Pinky": pinkyGrab = value; break;
            case "Ring": ringGrab = value; break;
            case "Middle": middleGrab = value; break;
            case "Pointer": pointerGrab = value; break;
            case "Thumb": thumbGrab = value; break;
        }
    }

    private bool GetFingerTouched(string fingerName)
    {
        switch (fingerName)
        {
            case "Pinky": return pinkyTouched;
            case "Ring": return ringTouched;
            case "Middle": return middleTouched;
            case "Pointer": return pointerTouched;
            case "Thumb": return thumbTouched;
            default: return false;
        }
    }

    private void SetFingerTouched(string fingerName, bool value)
    {
        switch (fingerName)
        {
            case "Pinky": pinkyTouched = value; break;
            case "Ring": ringTouched = value; break;
            case "Middle": middleTouched = value; break;
            case "Pointer": pointerTouched = value; break;
            case "Thumb": thumbTouched = value; break;
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
