using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class PointManager : MonoBehaviour
{
    public int score = 0;
    public List<TextMeshProUGUI> pointTexts;  // Lista di TextMeshPro per visualizzare il punteggio

    private void Awake()
    {
        UpdateScoreDisplay();
    }

    public void AddPoints(int points)
    {
        score += points;
        UpdateScoreDisplay();
    }

    private void UpdateScoreDisplay()
    {
        foreach (var pointText in pointTexts)
        {
            pointText.text = "Score: " + score.ToString();
        }
    }
}
