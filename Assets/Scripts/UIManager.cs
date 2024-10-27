using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    // Références aux éléments UI
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text bestScoreText;
    [SerializeField] private TMP_Text linesText;

    private int score;
    private int bestScore;
    private int lines;

    private void Start()
    {
        // Initialisation des scores
        score = 0;
        lines = 0;
        bestScore = PlayerPrefs.GetInt("BestScore", 0);
        UpdateUI();
    }

    public void AddScore(int points)
    {
        score += points;
        if (score > bestScore)
        {
            bestScore = score;
            PlayerPrefs.SetInt("BestScore", bestScore);
        }
        UpdateUI();
    }

    public void AddLine(int count)
    {
        lines += count;
        UpdateUI();
    }

    public void ResetUI()
    {
        lines = 0;
        score = 0;
        UpdateUI();
    }


    private void UpdateUI()
    {
        scoreText.text = "Score: " + score;
        bestScoreText.text = "Best Score: " + bestScore;
        linesText.text = "Lines: " + lines;
    }
}
