using System.Collections;
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
    public GrabPhysics grabPhysics;

    [Header("Activation Sound")]
    public AudioSource activationSound;
    public float fadeOutSpeed = 1f;

    [Header("Debug Variables")]
    [ReadOnly]
    public bool isGrabbingDisabled;
    [ReadOnly]
    public float targetTriggerValue;
    [ReadOnly]
    public float targetGripValue;
    [ReadOnly]
    public float targetThumbValue;
    [ReadOnly]
    public float GrabtargetThumbValue;

    private float currentTriggerValue = 0f;
    private float currentGripValue = 0f;
    private float currentThumbValue = 0f;
    private bool isSoundPlaying = false;
    private Coroutine fadeOutCoroutine;

    void Update()
    {
        // Get target values for Trigger and Grip
        targetTriggerValue = pinchAnimationAction.action.ReadValue<float>();
        targetGripValue = gripAnimationAction.action.ReadValue<float>();

        // Read button presence values
        float primaryButtonValue = primaryButtonPresenceAnimationAction.action.ReadValue<float>();
        float secondaryButtonValue = secondaryButtonPresenceAnimationAction.action.ReadValue<float>();
        float stickButtonValue = stickPresenceAnimationAction.action.ReadValue<float>();

        // Determine thumb button presence
        bool thumbButtonPressed = primaryButtonValue > 0.5f || secondaryButtonValue > 0.5f || stickButtonValue > 0.5f;
        targetThumbValue = thumbButtonPressed ? 1f : 0f;
        
        bool GrabthumbButtonPressed = primaryButtonValue > 0.5f;
        GrabtargetThumbValue = GrabthumbButtonPressed ? 1f : 0f;

        // Check if there is an object in the pull trigger
        bool objectInPullTrigger = pullTrigger != null && pullTrigger.HasObjectsInTrigger();

        // Determine if grabbing is disabled
        isGrabbingDisabled = (targetTriggerValue >= 0.99f) && (targetGripValue >= 0.99f) && (GrabtargetThumbValue >= 0.99f) && (grabPhysics != null && !grabPhysics.isGrabbing);

        // Debug logs
        //Debug.Log($"[AnimateHandOnInput] isGrabbingDisabled: {isGrabbingDisabled}");
        //Debug.Log($"[AnimateHandOnInput] primaryButtonValue: {primaryButtonValue}, secondaryButtonValue: {secondaryButtonValue}, stickButtonValue: {stickButtonValue}, targetThumbValue: {targetThumbValue}");

        if (grabPhysics != null && grabPhysics.isGrabbing)
        {
            // If grabbing, do not update finger animations
            currentThumbValue = 0f;
            handAnimator.SetFloat("ThumbButtonPresence", currentThumbValue);
        }
        else if (isGrabbingDisabled)
        {
            // Smoothly interpolate towards the target values
            currentTriggerValue = Mathf.Lerp(currentTriggerValue, targetTriggerValue, triggerSpeed * Time.deltaTime);
            currentGripValue = Mathf.Lerp(currentGripValue, targetGripValue, gripSpeed * Time.deltaTime);
            currentThumbValue = targetThumbValue;
        }
        else if (objectInPullTrigger)
        {
            // Smoothly interpolate towards 0 if in pull trigger
            currentTriggerValue = Mathf.Lerp(currentTriggerValue, 0f, triggerSpeed * Time.deltaTime);
            currentGripValue = Mathf.Lerp(currentGripValue, 0f, gripSpeed * Time.deltaTime);
            currentThumbValue = Mathf.Lerp(currentThumbValue, 0f, triggerSpeed * Time.deltaTime);
        }
        else
        {
            // Smoothly interpolate towards the target values
            currentTriggerValue = Mathf.Lerp(currentTriggerValue, targetTriggerValue, triggerSpeed * Time.deltaTime);
            currentGripValue = Mathf.Lerp(currentGripValue, targetGripValue, gripSpeed * Time.deltaTime);
            currentThumbValue = targetThumbValue;
        }

        // Update animation parameters
        handAnimator.SetFloat("Trigger", currentTriggerValue);
        handAnimator.SetFloat("Grip", currentGripValue);
        handAnimator.SetFloat("ThumbButtonPresence", currentThumbValue);

        // Check if all values are at maximum (>= 0.99)
        if (currentTriggerValue >= 0.99f && currentGripValue >= 0.99f && currentThumbValue >= 0.99f)
        {
            if (!isSoundPlaying)
            {
                // Start the sound if all values are high and it's not already playing
                if (fadeOutCoroutine != null)
                {
                    StopCoroutine(fadeOutCoroutine); // Stop any fade-out in progress
                    activationSound.volume = 1f;     // Ensure volume is reset
                }
                activationSound.Play();
                isSoundPlaying = true;
            }
        }
        else
        {
            // Start fade out if the values are no longer all high
            if (isSoundPlaying)
            {
                fadeOutCoroutine = StartCoroutine(FadeOutSound());
                isSoundPlaying = false;
            }
        }
    }

    private IEnumerator FadeOutSound()
    {
        while (activationSound.volume > 0)
        {
            activationSound.volume -= fadeOutSpeed * Time.deltaTime;
            yield return null;

            // Stop fade-out immediately if sound needs to restart
            if (isSoundPlaying)
            {
                activationSound.volume = 1f; // Reset volume
                yield break;                 // Exit the coroutine
            }
        }

        activationSound.Stop();
        activationSound.volume = 1f; // Reset volume for next play
    }
}
