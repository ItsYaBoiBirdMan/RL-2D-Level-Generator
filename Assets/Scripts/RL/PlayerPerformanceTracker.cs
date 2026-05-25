using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPerformanceTracker : MonoBehaviour
{
    [SerializeField] private int ConsecutiveWins;
    [SerializeField] private int ConsecutiveLosses;
    [SerializeField] private float DifficultyIncrement;

    public float Difficulty;
    
    public void UpdatePerformance(bool playerWon)
    {
        if (playerWon)
        {
            ConsecutiveWins++;
            ConsecutiveLosses = 0;
        }
        else
        {
            ConsecutiveLosses++;
            ConsecutiveWins = 0;
        }

        AdjustDifficulty();
    }
    
    private void AdjustDifficulty()
    {
        if (ConsecutiveWins >= 2)
            Difficulty += DifficultyIncrement;

        if (ConsecutiveLosses >= 2)
            Difficulty -= DifficultyIncrement;

        Difficulty = Mathf.Clamp01(Difficulty);
    }
}
