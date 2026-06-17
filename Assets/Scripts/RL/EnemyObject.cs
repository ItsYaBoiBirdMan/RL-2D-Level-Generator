using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Enemy Object", menuName = "ScriptableObjects/Enemy Object", order = 1)]
public class EnemyObject : ScriptableObject
{
    public GameObject EnemyPrefab;
    [Range(0, 1)] public float MinimumDifficultyAllowed = 0.5f;
    [Range(0, 1)] public float MaximumDifficultyAllowed = 0.5f;

    private void OnValidate()
    {
        if(MaximumDifficultyAllowed < MinimumDifficultyAllowed) 
            MaximumDifficultyAllowed = MinimumDifficultyAllowed;
        
        if(MinimumDifficultyAllowed > MaximumDifficultyAllowed)
            MinimumDifficultyAllowed = MaximumDifficultyAllowed;
    }
}