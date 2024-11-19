using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BasketSectionManager : MonoBehaviour
{
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

    [Header("Pre-Game Countdown")]
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

    private float gameTimer; // Game timer in seconds
    private float currentBasketActivationTime; // Activation time of the current prefab
    private PointManager pointManager; // Reference to PointManager to manage the score
    private int maxCombo = 0; // Maximum combo achieved
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

        // Start the pre-game countdown
        StartCoroutine(StartPreGameCountdown());
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

            // Force gameTimer to zero if it becomes negative
            if (gameTimer < 0)
            {
                gameTimer = 0;
            }
        }

        // Time's up, end the game
        StartCoroutine(EndGame());
    }

    private IEnumerator EndGame()
    {
        isGameRunning = false;

        // Deactivate all prefabs
        foreach (var prefab in basketsPrefabs)
        {
            prefab.SetActive(false);
        }

        // Display "Finished" in the preGameCountdownText
        if (preGameCountdownText != null)
        {
            preGameCountdownText.text = "Finished";
            preGameCountdownText.gameObject.SetActive(true);
        }

        // Play the "Finished" sound if available
        if (preGameCountdownSounds != null && preGameCountdownSounds.Count >= 4)
        {
            preGameCountdownSounds[3].Play();
        }

        // Wait for 2 seconds while "Finished" is displayed
        yield return new WaitForSeconds(2f);

        // Clear the "Finished" text
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

        // Additional actions to perform at the end of the game can be added here
        Debug.Log("The game has ended!");
    }

    private void UpdateResultsMenu()
    {
        if (finalScoreText != null && pointManager != null)
        {
            finalScoreText.text = "Final Score: " + pointManager.score.ToString();
        }

        if (maxComboText != null)
        {
            maxComboText.text = "Max Combo: " + maxCombo.ToString();
        }

        if (totalBasketsText != null)
        {
            totalBasketsText.text = "Total Baskets: " + totalBaskets.ToString();
        }
    }

    public void RegisterBasket(int combo)
    {
        totalBaskets++;
        if (combo > maxCombo)
        {
            maxCombo = combo;
        }
    }

    public void OnPrefabTriggerActivated()
    {
        if (!isGameRunning)
            return;

        if (currentActivePrefab != null)
        {
            // Deactivate the current prefab with a delay
            StartCoroutine(DeactivateWithDelay(currentActivePrefab));
        }

        // Activate a new random prefab
        ActivateRandomPrefab();
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

        // Choose a random prefab from the list
        int randomIndex = UnityEngine.Random.Range(0, basketsPrefabs.Count);
        currentActivePrefab = basketsPrefabs[randomIndex];
        currentActivePrefab.SetActive(true);

        // Record the activation time
        currentBasketActivationTime = Time.time;

        // Initialize the BasketScoreHandler for this prefab
        InitializeBasketScoreHandler(currentActivePrefab);
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
