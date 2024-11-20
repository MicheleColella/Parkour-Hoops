using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using MoreMountains.Feedbacks;

public class BasketSectionManager : MonoBehaviour
{
    [Header("Summary Menu Feedbacks")]
    public List<MMF_Player> menuFeedBacks;

    [Header("Basket Prefabs")]
    [Tooltip("List of basket prefabs present in the scene.")]
    public List<GameObject> basketsPrefabs; // List of prefabs present in the scene

    [Header("Game Settings")]
    [Tooltip("Game duration in format HH:MM:SS or MM:SS or SS (e.g., 0:10 for 10 seconds, 5:00 for 5 minutes).")]
    public string gameDuration = "0:10"; // Game duration

    [Tooltip("TextMeshProUGUI to display the game countdown timer.")]
    public TextMeshProUGUI gameTimerText; // TextMeshProUGUI to display the game countdown

    [Tooltip("Delay after which to deactivate the current basket prefab.")]
    public float deactivateDelay = 2f; // Time after which to deactivate the current prefab

    [Header("Pre-Game Settings")]
    [Tooltip("Delay before starting the pre-game countdown.")]
    public float preGameDelay = 2f; // Delay before the pre-game countdown starts

    [Tooltip("TextMeshProUGUI to display the pre-game countdown.")]
    public TextMeshProUGUI preGameCountdownText; // TextMeshProUGUI for pre-game countdown

    [Tooltip("List of AudioSources to play during pre-game countdown (should have 4 sounds).")]
    public List<AudioSource> preGameCountdownSounds; // List of AudioSources for pre-game countdown sounds

    [Header("End Game Settings")]
    [Tooltip("GameObject of the results menu to activate at the end of the game.")]
    public GameObject resultsMenu; // The results menu GameObject

    [Tooltip("Reference to the menu window GameObject to deactivate if active.")]
    public GameObject menuWindow; // Reference to the menu window from VRUIManager

    [Tooltip("TextMeshProUGUI to display the final score.")]
    public TextMeshProUGUI finalScoreText;

    [Tooltip("TextMeshProUGUI to display the maximum combo achieved.")]
    public TextMeshProUGUI maxComboText;

    [Tooltip("TextMeshProUGUI to display the total baskets made.")]
    public TextMeshProUGUI totalBasketsText;

    [Header("Debug")]
    [ReadOnly]
    [Tooltip("Currently active basket prefab.")]
    public GameObject currentActivePrefab; // Currently active prefab

    [ReadOnly]
    [Tooltip("Indicates whether the game is running.")]
    public bool isGameRunning = false; // Indicates if the game is running

    [Header("Results Display Settings")]
    [Tooltip("Duration over which the scores count up to their final value.")]
    public float countingDuration = 2f; // Duration to count up the numbers

    [Tooltip("Delay between displaying each score.")]
    public float scoreDisplayDelay = 0.5f; // Delay before moving to the next text

    private float gameTimer; // Game timer in seconds
    private float currentBasketActivationTime; // Activation time of the current prefab
    private PointManager pointManager; // Reference to PointManager to manage the score
    private int totalBaskets = 0; // Total baskets made

    private void Start()
    {
        // Automatically find the PointManager instance in the scene
        pointManager = FindObjectOfType<PointManager>();
        if (pointManager == null)
        {
            Debug.LogError("PointManager not found in the scene! Make sure there is an object with the PointManager script.");
        }

        // Deactivate all prefabs at the start
        foreach (var prefab in basketsPrefabs)
        {
            prefab.SetActive(false);
        }

        // Deactivate the results menu at the start
        if (resultsMenu != null)
        {
            resultsMenu.SetActive(false);
        }

        // Deactivate the preGameCountdownText GameObject initially
        if (preGameCountdownText != null)
        {
            preGameCountdownText.gameObject.SetActive(false);
        }

        // Start the pre-game countdown with delay
        StartCoroutine(StartPreGameCountdownWithDelay());
    }

    private IEnumerator StartPreGameCountdownWithDelay()
    {
        // Wait for the specified pre-game delay
        yield return new WaitForSeconds(preGameDelay);

        // Start the pre-game countdown
        yield return StartCoroutine(StartPreGameCountdown());
    }

    private IEnumerator StartPreGameCountdown()
    {
        int countdown = 3;
        while (countdown > 0)
        {
            if (preGameCountdownText != null)
            {
                preGameCountdownText.text = countdown.ToString();
                preGameCountdownText.gameObject.SetActive(true);
            }
            // Play the corresponding sound
            if (preGameCountdownSounds != null && preGameCountdownSounds.Count >= 4 - countdown)
            {
                preGameCountdownSounds[3 - countdown].Play();
            }
            yield return new WaitForSeconds(1f);
            countdown--;
        }

        // Display "Go!"
        if (preGameCountdownText != null)
        {
            preGameCountdownText.text = "Go!";
            preGameCountdownText.gameObject.SetActive(true);
        }
        // Play the "Go!" sound
        if (preGameCountdownSounds != null && preGameCountdownSounds.Count >= 4)
        {
            preGameCountdownSounds[3].Play();
        }
        yield return new WaitForSeconds(1f);

        // Hide the pre-game countdown
        if (preGameCountdownText != null)
        {
            preGameCountdownText.text = "";
            preGameCountdownText.gameObject.SetActive(false);
        }

        // Start the game
        StartGame();
    }

    private void StartGame()
    {
        // Parse game duration string to seconds
        gameTimer = ParseTimeStringToSeconds(gameDuration);
        if (gameTimer <= 0)
        {
            Debug.LogError("Invalid game duration. Ensure 'gameDuration' is in the correct format.");
            return;
        }

        isGameRunning = true;

        // Activate an initial random prefab
        ActivateRandomPrefab();

        // Start the game countdown
        StartCoroutine(GameTimerCountdown());
    }

    private IEnumerator GameTimerCountdown()
    {
        int lastSecond = Mathf.CeilToInt(gameTimer) + 1;

        while (gameTimer > 0)
        {
            // Update the timer text
            if (gameTimerText != null)
            {
                TimeSpan timeSpan = TimeSpan.FromSeconds(gameTimer);
                gameTimerText.text = string.Format("{0:D2}:{1:D2}", timeSpan.Minutes, timeSpan.Seconds);
            }

            // Check if we need to start the end-game countdown
            if (gameTimer <= 3)
            {
                int countdown = Mathf.CeilToInt(gameTimer);

                if (preGameCountdownText != null)
                {
                    preGameCountdownText.text = countdown.ToString();
                    preGameCountdownText.gameObject.SetActive(true);
                }

                // Play the corresponding sound if it's a new second
                if (countdown != lastSecond)
                {
                    int soundIndex = 3 - countdown; // Adjust the index as per the pre-game countdown
                    if (preGameCountdownSounds != null && soundIndex >= 0 && soundIndex < preGameCountdownSounds.Count)
                    {
                        preGameCountdownSounds[soundIndex].Play();
                    }
                    lastSecond = countdown;
                }
            }
            else
            {
                // Clear the preGameCountdownText if not in the end-game countdown
                if (preGameCountdownText != null && preGameCountdownText.text != "")
                {
                    preGameCountdownText.text = "";
                    preGameCountdownText.gameObject.SetActive(false);
                }
            }

            yield return new WaitForSeconds(1f);
            gameTimer--;

            // Ensure gameTimer does not go below zero
            if (gameTimer < 0)
            {
                gameTimer = 0;
            }
        }

        // Clear the game timer text immediately when the game ends
        if (gameTimerText != null)
        {
            gameTimerText.text = "";
        }

        // Clear the preGameCountdownText immediately when the game ends
        if (preGameCountdownText != null)
        {
            preGameCountdownText.text = "";
            preGameCountdownText.gameObject.SetActive(false);
        }

        // Time's up, end the game
        StartCoroutine(EndGame());
    }

    private IEnumerator EndGame()
    {
        // Indicate that the game has ended
        isGameRunning = false;

        // Deactivate all prefabs
        foreach (var prefab in basketsPrefabs)
        {
            prefab.SetActive(false);
        }

        // Hide the game timer text immediately
        if (gameTimerText != null)
        {
            gameTimerText.text = "";
        }

        // Hide the pre-game countdown text immediately
        if (preGameCountdownText != null)
        {
            preGameCountdownText.text = "";
            preGameCountdownText.gameObject.SetActive(false);
        }

        // Deactivate the menuWindow if it's active
        if (menuWindow != null && menuWindow.activeSelf)
        {
            menuWindow.SetActive(false);
        }

        // Update results on the results menu
        UpdateResultsMenu();

        // Activate the results menu
        if (resultsMenu != null)
        {
            resultsMenu.SetActive(true);
        }

        // Use Feedbacks
        foreach (var feed in menuFeedBacks)
        {
            if (feed != null)
            {
                feed.PlayFeedbacks();
            }
        }

        // Wait for any additional effects or transitions
        yield return new WaitForSeconds(1f);

        // Additional actions to perform at the end of the game can be added here
        Debug.Log("The game has ended!");
    }

    private void UpdateResultsMenu()
    {
        // Start the coroutine to display the results with counting effect
        StartCoroutine(DisplayResults());
    }

    private IEnumerator DisplayResults()
    {
        if (finalScoreText != null && pointManager != null)
        {
            yield return StartCoroutine(CountToValue(finalScoreText, "", 0, pointManager.score, countingDuration));
            yield return new WaitForSeconds(scoreDisplayDelay);
        }

        if (maxComboText != null && pointManager != null)
        {
            yield return StartCoroutine(CountToValue(maxComboText, "", 0, pointManager.maxComboAchieved, countingDuration));
            yield return new WaitForSeconds(scoreDisplayDelay);
        }

        if (totalBasketsText != null)
        {
            yield return StartCoroutine(CountToValue(totalBasketsText, "", 0, totalBaskets, countingDuration));
            yield return new WaitForSeconds(scoreDisplayDelay);
        }
    }

    private IEnumerator CountToValue(TextMeshProUGUI textComponent, string prefix, int startValue, int endValue, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            int currentValue = Mathf.RoundToInt(Mathf.Lerp(startValue, endValue, t));
            textComponent.text = prefix + currentValue.ToString();
            yield return null;
        }
        // Ensure the final value is set
        textComponent.text = prefix + endValue.ToString();
    }

    public void RegisterBasket()
    {
        totalBaskets++;
    }

    public void OnPrefabTriggerActivated()
    {
        if (!isGameRunning)
            return;

        // Store a reference to the current active prefab before activating a new one
        GameObject prefabToDeactivate = currentActivePrefab;

        // Activate a new random prefab
        ActivateRandomPrefab();

        // If we have a new prefab activated, deactivate the old one after delay
        if (prefabToDeactivate != null && prefabToDeactivate != currentActivePrefab)
        {
            // Deactivate the old prefab with a delay
            StartCoroutine(DeactivateWithDelay(prefabToDeactivate));
        }
        else
        {
            // No need to deactivate
            Debug.Log("Reusing the same prefab; not deactivating it.");
        }
    }


    private void ActivateRandomPrefab()
    {
        if (!isGameRunning)
            return;

        if (basketsPrefabs.Count == 0)
        {
            Debug.LogError("The basketsPrefabs list is empty! Add at least one prefab to the list.");
            return;
        }

        // Create a list of available prefabs excluding the current active prefab
        List<GameObject> availablePrefabs = new List<GameObject>(basketsPrefabs);

        // Remove the current active prefab from the list
        if (currentActivePrefab != null)
        {
            availablePrefabs.Remove(currentActivePrefab);
        }

        if (availablePrefabs.Count == 0)
        {
            // Only the current prefab is available; reuse it
            Debug.Log("Only one prefab available; reusing the current prefab.");
            // No need to change currentActivePrefab
        }
        else
        {
            // Choose a random prefab from the available prefabs
            int randomIndex = UnityEngine.Random.Range(0, availablePrefabs.Count);
            currentActivePrefab = availablePrefabs[randomIndex];
        }

        // Reset the prefab before activation
        ResetBasketPrefab(currentActivePrefab);

        // Activate the prefab if it's not already active
        if (!currentActivePrefab.activeSelf)
        {
            currentActivePrefab.SetActive(true);
        }

        // Record the activation time
        currentBasketActivationTime = Time.time;

        // Initialize the BasketScoreHandler for this prefab
        InitializeBasketScoreHandler(currentActivePrefab);
    }


    private void ResetBasketPrefab(GameObject prefab)
    {
        // Reset any necessary components or variables on the prefab
        // For example, reset position, rotation, etc., if needed

        // Ensure all child objects are enabled
        foreach (Transform child in prefab.transform)
        {
            child.gameObject.SetActive(true);
        }

        // If the prefab has any scripts that need resetting, call their reset methods here
        // For example, reset the BasketScoreHandler
        BasketScoreHandler scoreHandler = prefab.GetComponentInChildren<BasketScoreHandler>();
        if (scoreHandler != null)
        {
            scoreHandler.SetActivationTime(Time.time);
        }
    }

    private void InitializeBasketScoreHandler(GameObject prefab)
    {
        // The BasketScoreHandler is located in the child of the prefab
        BasketScoreHandler scoreHandler = prefab.GetComponentInChildren<BasketScoreHandler>();
        if (scoreHandler != null)
        {
            scoreHandler.SetSectionManager(this);
            scoreHandler.SetActivationTime(currentBasketActivationTime);
        }
        else
        {
            Debug.LogError("BasketScoreHandler not found in prefab " + prefab.name);
        }
    }

    private IEnumerator DeactivateWithDelay(GameObject prefab)
    {
        yield return new WaitForSeconds(deactivateDelay);
        if (prefab != null)
        {
            prefab.SetActive(false);
        }
    }

    private float ParseTimeStringToSeconds(string timeString)
    {
        string[] timeParts = timeString.Split(':');

        int hours = 0;
        int minutes = 0;
        int seconds = 0;

        if (timeParts.Length == 1)
        {
            // Only seconds provided
            if (int.TryParse(timeParts[0], out seconds))
            {
                return seconds;
            }
        }
        else if (timeParts.Length == 2)
        {
            // Minutes and seconds provided
            if (int.TryParse(timeParts[0], out minutes) && int.TryParse(timeParts[1], out seconds))
            {
                return minutes * 60 + seconds;
            }
        }
        else if (timeParts.Length == 3)
        {
            // Hours, minutes, and seconds provided
            if (int.TryParse(timeParts[0], out hours) && int.TryParse(timeParts[1], out minutes) && int.TryParse(timeParts[2], out seconds))
            {
                return hours * 3600 + minutes * 60 + seconds;
            }
        }

        Debug.LogError("Invalid time format: " + timeString);
        return 0f;
    }
}
