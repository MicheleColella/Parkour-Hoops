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

    private float thumbButtonPresence;
    private float primaryButtonPresence;
    private float secondaryButtonPresence;
    private float stickPresence;
    private float triggerValue;
    private float gripValue;

    // Update is called once per frame
    void Update()
    {
        triggerValue = pinchAnimationAction.action.ReadValue<float>();
        handAnimator.SetFloat("Trigger", triggerValue);

        gripValue = gripAnimationAction.action.ReadValue<float>();
        handAnimator.SetFloat("Grip", gripValue);

        primaryButtonPresence = primatyButtonPresenceAnimationAction.action.ReadValue<float>();

        secondaryButtonPresence = secondaryButtonPresenceAnimationAction.action.ReadValue<float>();

        stickPresence = stickPresenceAnimationAction.action.ReadValue<float>();

        if (primaryButtonPresence == 1 || secondaryButtonPresence == 1 || stickPresence == 1)
        {
            thumbButtonPresence = 1;
        }
        else
        {
            thumbButtonPresence = 0;
        }

        handAnimator.SetFloat("ThumbButtonPresence", thumbButtonPresence);
    }
}
