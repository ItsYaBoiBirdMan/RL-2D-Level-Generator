using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine;

public class PlayerPerformanceTracker : MonoBehaviour
{
    public static PlayerPerformanceTracker Instance;

    [SerializeField] private int ConsecutiveWins;
    [SerializeField] private int ConsecutiveLosses;
    [SerializeField] private float DifficultyIncrement = 0.1f;

    public float Difficulty;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        
        DontDestroyOnLoad(gameObject);
    }

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
