using UnityEngine;
using UnityEngine.InputSystem;

public class AnimateHandOnInput : MonoBehaviour
{
    [Header("Input Actions")]
    public InputActionProperty pinchAnimationAction;
    public InputActionProperty gripAnimationAction;
    public InputActionProperty primaryButtonPresenceAnimationAction;
    public InputActionProperty secondaryButtonPresenceAnimationAction;
    public InputActionProperty stickPresenceAnimationAction;

    [Header("Animator")]
    public Animator handAnimator;

    [Header("Animation Speeds")]
    public float triggerSpeed = 5f;
    public float gripSpeed = 5f;

    private float currentTriggerValue = 0f;
    private float currentGripValue = 0f;

    void Update()
    {
        // Get target values for Trigger and Grip
        float targetTriggerValue = pinchAnimationAction.action.ReadValue<float>();
        float targetGripValue = gripAnimationAction.action.ReadValue<float>();

        // Smoothly interpolate towards the target values using Mathf.Lerp
        currentTriggerValue = Mathf.Lerp(currentTriggerValue, targetTriggerValue, triggerSpeed * Time.deltaTime);
        currentGripValue = Mathf.Lerp(currentGripValue, targetGripValue, gripSpeed * Time.deltaTime);

        // Update animation parameters
        handAnimator.SetFloat("Trigger", currentTriggerValue);
        handAnimator.SetFloat("Grip", currentGripValue);

        // Read button presence values
        float primaryButtonPresence = primaryButtonPresenceAnimationAction.action.ReadValue<float>();
        float secondaryButtonPresence = secondaryButtonPresenceAnimationAction.action.ReadValue<float>();
        float stickPresence = stickPresenceAnimationAction.action.ReadValue<float>();

        // Determine thumb button presence
        float thumbButtonPresence = (primaryButtonPresence == 1f || secondaryButtonPresence == 1f || stickPresence == 1f) ? 1f : 0f;

        // Update ThumbButtonPresence animation parameter
        handAnimator.SetFloat("ThumbButtonPresence", thumbButtonPresence);
    }
}
