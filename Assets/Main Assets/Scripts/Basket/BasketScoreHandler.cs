using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasketScoreHandler : MonoBehaviour
{
    [Header("Trigger Settings")]
    [Tooltip("The tag of the object that can activate the trigger.")]
    public string targetTag = "Ball";

    [Tooltip("Cooldown time in seconds during which the trigger is inactive.")]
    public float triggerCooldown = 2f;

    private bool isTriggerOnCooldown = false; // Indicates if the trigger is on cooldown

    [Header("Visual Effects")]
    [Tooltip("Prefab to instantiate when the trigger is activated.")]
    public GameObject VFXToInstantiate;

    [Tooltip("Position and transformations of the VFX to instantiate.")]
    public Transform vfxPosition;

    [Tooltip("Time after which the VFX will be destroyed.")]
    public float destroyDelay = 2f;

    [Header("Audio Settings")]
    [Tooltip("AudioSource for the sound to play when the trigger is activated.")]
    public AudioSource audioSource;

    [Space]
    [Tooltip("AudioSource for the whistle played at the start.")]
    public AudioSource whistleAudioSource;

    [Tooltip("If true, plays a whistle when the object is activated.")]
    public bool playWhistleOnStart = false;

    [Header("Animation Settings")]
    [Tooltip("Animator component for handling animations.")]
    public Animator animator; // Reference to the Animator component

    [Tooltip("Name of the default animation to play.")]
    public string defaultAnimationName = "Float";

    [Tooltip("Name of the goal animation to play when scoring.")]
    public string goalAnimationName = "GoalAnim";

    [Header("References")]
    [Tooltip("Manages the score in the scene.")]
    private PointManager pointManager; // Reference to the PointManager that manages the score

    [Tooltip("Reference to the BasketSectionManager.")]
    private BasketSectionManager sectionManager; // Reference to the BasketSectionManager

    [Header("Scoring Settings")]
    [Tooltip("Score awarded if the basket is made within the first time interval.")]
    public int firstIntervalScore = 10;

    [Tooltip("Time limit for the first scoring interval (in seconds).")]
    public float firstIntervalTimeLimit = 5f;

    [Tooltip("Score awarded if the basket is made within the second time interval.")]
    public int secondIntervalScore = 5;

    [Tooltip("Time limit for the second scoring interval (in seconds).")]
    public float secondIntervalTimeLimit = 15f;

    [Tooltip("Score awarded if the basket is made within the third time interval.")]
    public int thirdIntervalScore = 3;

    [Tooltip("Time limit for the third scoring interval (in seconds).")]
    public float thirdIntervalTimeLimit = 30f;

    [ReadOnly]
    [Tooltip("Activation time of the basket prefab.")]
    public float activationTime; // Activation time of the prefab

    private void Awake()
    {
        // Automatically find the PointManager instance in the scene
        pointManager = FindObjectOfType<PointManager>();

        // Check if found
        if (pointManager == null)
        {
            Debug.LogError("PointManager not found in the scene! Make sure there is an object with the PointManager script.");
        }

        // Check if animator is assigned
        if (animator == null)
        {
            Debug.LogError("Animator component not assigned! Please assign it in the Inspector.");
        }
    }

    private void OnEnable()
    {
        // Reset the trigger cooldown
        isTriggerOnCooldown = false;

        // Reset the activation time
        activationTime = Time.time;

        // Enable colliders/triggers if they were disabled
        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = true;
        }

        // Play the whistle if required
        if (playWhistleOnStart && whistleAudioSource != null)
        {
            whistleAudioSource.Play();
        }

        // Reset animator state and play the default animation
        if (animator != null)
        {
            animator.Rebind(); // Resets the animator to its default state
            animator.Play(defaultAnimationName, 0, 0f); // Start default animation from the beginning
        }
    }


    private void OnDisable()
    {
        // Disable colliders/triggers to prevent unwanted interactions while inactive
        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isTriggerOnCooldown) return; // Block activation if on cooldown

        if (sectionManager != null && !sectionManager.isGameRunning) return; // Do not proceed if the game has ended

        if (other.CompareTag(targetTag))
        {
            // Start trigger actions and begin cooldown
            StartCoroutine(HandleTriggerCooldown());
        }
    }

    private IEnumerator HandleTriggerCooldown()
    {
        // Activate the cooldown
        isTriggerOnCooldown = true;

        // Calculate the score based on time
        int basePoints = CalculateBaseScore(out bool shouldIncreaseCombo);

        // Update the combo
        if (shouldIncreaseCombo)
        {
            pointManager?.IncreaseCombo();
        }
        else
        {
            pointManager?.ResetCombo();
        }

        // Calculate the total score with combo
        int totalPoints = basePoints * pointManager.GetComboMultiplier();

        // Update the score
        pointManager?.AddPoints(totalPoints);

        // Notify the BasketSectionManager of the basket made
        sectionManager?.RegisterBasket();

        // Play the sound from the AudioSource
        if (audioSource != null)
        {
            audioSource.Play();
        }

        // Play the goal animation
        if (animator != null)
        {
            animator.Play(goalAnimationName);
        }

        // Instantiate the prefab at the position, rotation, and scale of the spawnPoint
        if (VFXToInstantiate != null && vfxPosition != null)
        {
            GameObject instantiatedObject = Instantiate(
                VFXToInstantiate,
                vfxPosition.position,
                vfxPosition.rotation
            );

            instantiatedObject.transform.localScale = vfxPosition.localScale;

            // Destroy the object after a certain delay
            Destroy(instantiatedObject, destroyDelay);
        }

        // Notify the BasketSectionManager
        sectionManager?.OnPrefabTriggerActivated();

        // Wait for the cooldown time
        yield return new WaitForSeconds(triggerCooldown);

        // Deactivate the cooldown
        isTriggerOnCooldown = false;
    }

    private int CalculateBaseScore(out bool increaseCombo)
    {
        float timeSinceActivation = Time.time - activationTime;
        int baseScore = 0;
        increaseCombo = false;

        if (timeSinceActivation <= firstIntervalTimeLimit)
        {
            baseScore = firstIntervalScore;
            increaseCombo = true;
        }
        else if (timeSinceActivation <= secondIntervalTimeLimit)
        {
            baseScore = secondIntervalScore;
            increaseCombo = true;
        }
        else if (timeSinceActivation <= thirdIntervalTimeLimit)
        {
            baseScore = thirdIntervalScore;
            increaseCombo = false;
        }
        else
        {
            baseScore = 0;
            increaseCombo = false;
        }

        return baseScore;
    }

    public void SetActivationTime(float time)
    {
        activationTime = time;
    }

    public void SetSectionManager(BasketSectionManager manager)
    {
        sectionManager = manager;
    }
}
