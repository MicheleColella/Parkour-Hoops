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

    [Header("Pull Trigger Reference")]
    public PullObjectTrigger pullTrigger;

    [Header("Grab Physics Reference")]
    public GrabPhysics grabPhysics; // Reference to the GrabPhysics script

    private float currentTriggerValue = 0f;
    private float currentGripValue = 0f;
    private float currentThumbValue = 0f;

    void Update()
    {
        // Get target values for Trigger and Grip
        float targetTriggerValue = pinchAnimationAction.action.ReadValue<float>();
        float targetGripValue = gripAnimationAction.action.ReadValue<float>();

        // Read button presence values
        float primaryButtonValue = primaryButtonPresenceAnimationAction.action.ReadValue<float>();
        float secondaryButtonValue = secondaryButtonPresenceAnimationAction.action.ReadValue<float>();
        float stickButtonValue = stickPresenceAnimationAction.action.ReadValue<float>();

        // Determine thumb button presence
        bool thumbButtonPressed = primaryButtonValue > 0.5f || secondaryButtonValue > 0.5f || stickButtonValue > 0.5f;
        float targetThumbValue = thumbButtonPressed ? 1f : 0f;

        // Check if there is an object in the pull trigger
        bool objectInPullTrigger = pullTrigger != null && pullTrigger.HasObjectsInTrigger();

        if (grabPhysics != null && grabPhysics.isGrabbing)
        {
            // Hand is grabbing, do not update finger animations
            // Set ThumbButtonPresence to 0 to prevent interference
            currentThumbValue = 0f;
            handAnimator.SetFloat("ThumbButtonPresence", currentThumbValue);
        }
        else
        {
            if (objectInPullTrigger)
            {
                // Smoothly interpolate towards 0
                currentTriggerValue = Mathf.Lerp(currentTriggerValue, 0f, triggerSpeed * Time.deltaTime);
                currentGripValue = Mathf.Lerp(currentGripValue, 0f, gripSpeed * Time.deltaTime);
                currentThumbValue = Mathf.Lerp(currentThumbValue, 0f, triggerSpeed * Time.deltaTime);
            }
            else
            {
                // Smoothly interpolate towards the target values
                currentTriggerValue = Mathf.Lerp(currentTriggerValue, targetTriggerValue, triggerSpeed * Time.deltaTime);
                currentGripValue = Mathf.Lerp(currentGripValue, targetGripValue, gripSpeed * Time.deltaTime);

                // Update thumb value without Lerp to prevent fluctuation
                currentThumbValue = targetThumbValue;
            }

            // Update animation parameters
            handAnimator.SetFloat("Trigger", currentTriggerValue);
            handAnimator.SetFloat("Grip", currentGripValue);
            handAnimator.SetFloat("ThumbButtonPresence", currentThumbValue);
        }
    }
}
