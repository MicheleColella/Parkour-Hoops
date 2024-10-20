using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class AnimateHandOnInput : MonoBehaviour
{
    public InputActionProperty pinchAnimationAction;
    public InputActionProperty gripAnimationAction;
    public InputActionProperty primatyButtonPresenceAnimationAction;
    public InputActionProperty secondaryButtonPresenceAnimationAction;
    public InputActionProperty stickPresenceAnimationAction;
    public Animator handAnimator;

    // Update is called once per frame
    void Update()
    {
        float triggerValue = pinchAnimationAction.action.ReadValue<float>();
        handAnimator.SetFloat("Trigger", triggerValue);

        float gripValue = gripAnimationAction.action.ReadValue<float>();
        handAnimator.SetFloat("Grip", gripValue);

        float primaryButtonPresence = primatyButtonPresenceAnimationAction.action.ReadValue<float>();
        handAnimator.SetFloat("PrimaryButtonPresence", primaryButtonPresence);

        float secondaryButtonPresence = secondaryButtonPresenceAnimationAction.action.ReadValue<float>();
        handAnimator.SetFloat("SecondaryButtonPresence", secondaryButtonPresence);

        float stickPresence = stickPresenceAnimationAction.action.ReadValue<float>();
        handAnimator.SetFloat("StickPresence", stickPresence);

    }
}
