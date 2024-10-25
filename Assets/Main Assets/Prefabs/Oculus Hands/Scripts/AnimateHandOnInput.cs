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

    void Update()
    {
        // Update Trigger animation parameter
        float triggerValue = pinchAnimationAction.action.ReadValue<float>();
        handAnimator.SetFloat("Trigger", triggerValue);

        // Update Grip animation parameter
        float gripValue = gripAnimationAction.action.ReadValue<float>();
        handAnimator.SetFloat("Grip", gripValue);

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
