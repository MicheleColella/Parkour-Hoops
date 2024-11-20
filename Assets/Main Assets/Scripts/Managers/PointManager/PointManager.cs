using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class PointManager : MonoBehaviour
{
    [Header("Score Settings")]
    [Tooltip("Current score.")]
    [ReadOnly]
    public int score = 0;

    [Tooltip("Current combo multiplier.")]
    [ReadOnly]
    public int comboMultiplier = 1;

    [Tooltip("Maximum combo achieved.")]
    [ReadOnly]
    public int maxComboAchieved = 1;

    [Tooltip("List of TextMeshProUGUI to display the score.")]
    public List<TextMeshProUGUI> pointTexts;  // List of TextMeshPro to display the score

    [Tooltip("TextMeshProUGUI to display the combo.")]
    public TextMeshProUGUI comboText; // TextMeshProUGUI to display the combo

    private void Awake()
    {
        UpdateScoreDisplay();
        UpdateComboDisplay();
    }

    public void AddPoints(int points)
    {
        score += points;
        UpdateScoreDisplay();
    }

    public void IncreaseCombo()
    {
        comboMultiplier++;
        if (comboMultiplier > maxComboAchieved)
        {
            maxComboAchieved = comboMultiplier;
        }
        UpdateComboDisplay();
    }

    public void ResetCombo()
    {
        comboMultiplier = 1;
        UpdateComboDisplay();
    }

    public int GetComboMultiplier()
    {
        return comboMultiplier;
    }

    private void UpdateScoreDisplay()
    {
        foreach (var pointText in pointTexts)
        {
            pointText.text = "Score: " + score.ToString();
        }
    }

    private void UpdateComboDisplay()
    {
        if (comboText != null)
        {
            if (comboMultiplier > 1)
            {
                comboText.text = "x" + comboMultiplier.ToString();
            }
            else
            {
                comboText.text = "";
            }
        }
    }
}
