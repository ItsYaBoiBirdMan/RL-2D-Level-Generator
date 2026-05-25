using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Random = UnityEngine.Random;

public class LevelGeneratorOptimizer : MonoBehaviour
{
    [SerializeField] private LevelGenerator Generator;
    [SerializeField] private int Seed;
    [SerializeField] private int PopulationSize;
    [SerializeField] private int Generations;

    [SerializeField] private bool SearchForBetterSeedMode;
    [SerializeField] private int SearchAttempts;
    [SerializeField] private float DesiredScore;
    [SerializeField] private float TargetDifficulty;
    
    private void Start()
    {
        SearchForTargetDifficulty(50);
    }

    private GeneratorParameters RandomParameters(System.Random rng, float difficulty)
    {
        int gridHeight = Generator.GetGridHeight();

        // Helper
        float NextFloat(float min, float max) => (float)(rng.NextDouble() * (max - min) + min);

        // Terrain variation controlled by difficulty
        int baseGround = gridHeight / 3;
        int range = Mathf.RoundToInt(Mathf.Lerp(1, gridHeight / 3, difficulty));

        int minGround = rng.Next(
            Mathf.Max(1, baseGround - range),
            Mathf.Max(2, baseGround - range + 1)
        );

        int maxGround = rng.Next(
            Mathf.Min(gridHeight - 2, baseGround + 1),
            Mathf.Min(gridHeight - 1, baseGround + range + 1)
        );

        // Gaps scale with difficulty
        int minGap = Mathf.RoundToInt(Mathf.Lerp(1, 3, difficulty));
        int maxGap = Mathf.RoundToInt(Mathf.Lerp(3, 7, difficulty));

        // Platforms become trickier on harder difficulty
        int minPlatLen = Mathf.RoundToInt(Mathf.Lerp(4, 2, difficulty));
        int maxPlatLen = Mathf.RoundToInt(Mathf.Lerp(10, 6, difficulty));

        int minPlatHeight = Mathf.RoundToInt(Mathf.Lerp(2, 4, difficulty));
        int maxPlatHeight = Mathf.RoundToInt(Mathf.Lerp(5, 8, difficulty));

        return new GeneratorParameters
        {
            BaseSeed = Seed,

            MinGroundHeight = minGround,
            MaxGroundHeight = maxGround,

            GapChance = NextFloat(
                Mathf.Lerp(0.01f, 0.05f, difficulty),
                Mathf.Lerp(0.05f, 0.15f, difficulty)
            ),

            MinGap = minGap,
            MaxGap = maxGap,

            MinDistanceBetweenGaps = Mathf.RoundToInt(Mathf.Lerp(8, 3, difficulty)),

            PlatformAttempts = Mathf.RoundToInt(Mathf.Lerp(600, 350, difficulty)),

            MinPlatformLength = minPlatLen,
            MaxPlatformLength = maxPlatLen,

            MinPlatformHeightFromGround = minPlatHeight,
            MaxPlatformHeightFromGround = maxPlatHeight,

            MinVerticalSeparation = Mathf.RoundToInt(Mathf.Lerp(4, 2, difficulty)),

            EnemyDensity = NextFloat(
                Mathf.Lerp(0.4f, 0.7f, difficulty),
                Mathf.Lerp(0.8f, 1.2f, difficulty)
            ),

            MinDistanceBetweenEnemies = Mathf.RoundToInt(Mathf.Lerp(6, 2, difficulty)),

            PlatformEnemyRatio = NextFloat(
                Mathf.Lerp(0.1f, 0.3f, difficulty),
                Mathf.Lerp(0.4f, 0.8f, difficulty)
            )
        };
    }
    
    /*private float EvaluateParameters(GeneratorParameters p)
    {
        const int seedsToTest = 5;   // start small (3–5)
        float totalScore = 0f;

        for (int i = 0; i < seedsToTest; i++)
        {
            int seed = p.BaseSeed + i * 997;

            Generator.ApplyParameters(p);
            Generator.GenerateRawLevel(seed);

            Vector2Int spawn = Generator.SetPlayerSpawnPoint();
            float score = Generator.EvaluateLevel(spawn);

            totalScore += score;
        }

        return totalScore / seedsToTest;
    }*/
    
    private float EvaluateParameters(GeneratorParameters p)
    {
        Generator.ApplyParameters(p);
        
        Generator.GenerateRawLevel(p.BaseSeed);

        Vector2Int spawn = Generator.SetPlayerSpawnPoint();

        float score = Generator.EvaluateLevel(spawn);

        return score;
    }

    /*private GeneratorParameters Mutate(GeneratorParameters p, System.Random rng)
    {
        GeneratorParameters m = new GeneratorParameters();
        
        m.BaseSeed = p.BaseSeed;

        int NextInt(int min, int maxInclusive) => rng.Next(min, maxInclusive + 1);
        float NextFloat(float min, float max) => (float)(rng.NextDouble() * (max - min) + min);

        // --- Ground ---
        m.MinGroundHeight = Mathf.Clamp(p.MinGroundHeight + NextInt(-1, 1), 2, 10);
        m.MaxGroundHeight = Mathf.Clamp(p.MaxGroundHeight + NextInt(-1, 1), m.MinGroundHeight + 2, 15);

        // --- Gaps ---
        m.GapChance = Mathf.Clamp(p.GapChance + NextFloat(-0.01f, 0.01f), 0.02f, 0.15f);
        m.MinGap = Mathf.Clamp(p.MinGap + NextInt(-1, 1), 1, 5);
        m.MaxGap = Mathf.Clamp(p.MaxGap + NextInt(-1, 1), m.MinGap + 1, 8);
        m.MinDistanceBetweenGaps = Mathf.Clamp(p.MinDistanceBetweenGaps + NextInt(-2, 2), 3, 9);

        // --- Platforms ---
        m.PlatformAttempts = Mathf.Clamp(p.PlatformAttempts + NextInt(-50, 50), 200, 800);
        m.MinPlatformLength = Mathf.Clamp(p.MinPlatformLength + NextInt(-1, 1), 2, 6);
        m.MaxPlatformLength = Mathf.Clamp(p.MaxPlatformLength + NextInt(-1, 1), m.MinPlatformLength + 1, 12);
        m.MinPlatformHeightFromGround = Mathf.Clamp(p.MinPlatformHeightFromGround + NextInt(-1, 1), 3, 6);
        m.MaxPlatformHeightFromGround = Mathf.Clamp(p.MaxPlatformHeightFromGround + NextInt(-1, 1), m.MinPlatformHeightFromGround + 1, 10);
        m.MinVerticalSeparation = Mathf.Clamp(p.MinVerticalSeparation + NextInt(-1, 1), 2, 8);

        // --- Enemies ---
        m.EnemyDensity = Mathf.Clamp(p.EnemyDensity + NextFloat(-0.05f, 0.05f), 0.5f, 1f);
        m.MinDistanceBetweenEnemies = Mathf.Clamp(p.MinDistanceBetweenEnemies + NextInt(-1, 1), 2, 7);
        m.PlatformEnemyRatio = Mathf.Clamp(p.PlatformEnemyRatio + NextFloat(-0.07f, 0.07f), 0.1f, 0.9f);

        return Sanitize(m);
    }*/
    
    private GeneratorParameters Mutate(GeneratorParameters p, System.Random rng, float difficulty)
    {
        GeneratorParameters m = new GeneratorParameters();
        m.BaseSeed = p.BaseSeed;

        int MutRange(int baseRange) => Mathf.RoundToInt(Mathf.Lerp(1, baseRange, difficulty));

        int NextInt(int min, int maxInclusive) => rng.Next(min, maxInclusive + 1);
        float NextFloat(float min, float max) => (float)(rng.NextDouble() * (max - min) + min);

        int groundMut = MutRange(2);
        int gapMut = MutRange(2);
        float gapChanceMut = Mathf.Lerp(0.005f, 0.02f, difficulty);

        // --- Ground ---
        m.MinGroundHeight = Mathf.Clamp(p.MinGroundHeight + NextInt(-groundMut, groundMut), 2, 10);
        m.MaxGroundHeight = Mathf.Clamp(p.MaxGroundHeight + NextInt(-groundMut, groundMut), m.MinGroundHeight + 2, 15);

        // --- Gaps ---
        m.GapChance = Mathf.Clamp(p.GapChance + NextFloat(-gapChanceMut, gapChanceMut), 0.02f, 0.15f);
        m.MinGap = Mathf.Clamp(p.MinGap + NextInt(-gapMut, gapMut), 1, 5);
        m.MaxGap = Mathf.Clamp(p.MaxGap + NextInt(-gapMut, gapMut), m.MinGap + 1, 8);
        m.MinDistanceBetweenGaps = Mathf.Clamp(p.MinDistanceBetweenGaps + NextInt(-2, 2), 3, 9);

        // --- Platforms ---
        int platMut = MutRange(2);
        m.PlatformAttempts = Mathf.Clamp(p.PlatformAttempts + NextInt(-50, 50), 200, 800);
        m.MinPlatformLength = Mathf.Clamp(p.MinPlatformLength + NextInt(-platMut, platMut), 2, 6);
        m.MaxPlatformLength = Mathf.Clamp(p.MaxPlatformLength + NextInt(-platMut, platMut), m.MinPlatformLength + 1, 12);

        m.MinPlatformHeightFromGround = Mathf.Clamp(p.MinPlatformHeightFromGround + NextInt(-1, 1), 3, 6);
        m.MaxPlatformHeightFromGround = Mathf.Clamp(p.MaxPlatformHeightFromGround + NextInt(-1, 1), m.MinPlatformHeightFromGround + 1, 10);

        m.MinVerticalSeparation = Mathf.Clamp(p.MinVerticalSeparation + NextInt(-1, 1), 2, 8);

        // --- Enemies ---
        float enemyMut = Mathf.Lerp(0.02f, 0.07f, difficulty);
        m.EnemyDensity = Mathf.Clamp(p.EnemyDensity + NextFloat(-enemyMut, enemyMut), 0.5f, 1f);

        m.MinDistanceBetweenEnemies = Mathf.Clamp(p.MinDistanceBetweenEnemies + NextInt(-1, 1), 2, 7);

        float ratioMut = Mathf.Lerp(0.03f, 0.1f, difficulty);
        m.PlatformEnemyRatio = Mathf.Clamp(p.PlatformEnemyRatio + NextFloat(-ratioMut, ratioMut), 0.1f, 0.9f);

        return Sanitize(m);
    }

    private void OptimizeGenerator(int startingSeed)
    {
        List<(GeneratorParameters p, float score)> population = new();

        // Load the initial best parameters
        GeneratorParameters bestParams = LoadParametersFromLevelDataJson();
        System.Random rng;

        if (!SearchForBetterSeedMode)
        {
            rng = new System.Random(bestParams.BaseSeed);
        }
        else
        {
            bestParams.BaseSeed = startingSeed;
            rng = new System.Random(Guid.NewGuid().GetHashCode());
        }

        float bestScore = EvaluateParameters(bestParams);
        
        // --- Step 1: Initial random population ---
        for (int i = 0; i < PopulationSize; i++)
        {
            var p = RandomParameters(rng, TargetDifficulty); // New individual with its own seed
            float score = EvaluateParameters(p);

            population.Add((p, score));

            if (score > bestScore)
            {
                bestScore = score;
                bestParams = p;
                Generator.SaveLevel(bestParams, score);
            }
        }

        // --- Step 2: Evolutionary loop ---
        for (int g = 0; g < Generations; g++)
        {
            // Sort population by score descending
            population.Sort((a, b) => b.score.CompareTo(a.score));

            // Keep top 50%
            population = population.GetRange(0, PopulationSize / 2);
            int survivors = population.Count;

            for (int i = 0; i < survivors; i++)
            {
                var mutated = Mutate(population[i].p, rng, TargetDifficulty); // Seed preserved in mutate
                float score = EvaluateParameters(mutated);

                population.Add((mutated, score));

                if (score > bestScore)
                {
                    bestScore = score;
                    bestParams = mutated;
                    bestParams.BaseSeed = startingSeed;
                    Debug.Log("NEW BEST: " + bestScore + " | Seed: " + bestParams.BaseSeed);
                    Generator.SaveLevel(bestParams, score);
                }
            }

            Debug.Log("Generation " + g + " best score: " + population[0].score);
        }

        Debug.LogWarning("FINAL BEST SCORE: " + bestScore);
        Debug.Log("FINAL BEST SEED: " + bestParams.BaseSeed);
        Debug.Log(JsonUtility.ToJson(bestParams, true));
    }
    
    
    private void OptimizeGeneratorWithOutSaving(int startingSeed)
    {
        List<(GeneratorParameters p, float score)> population = new();

        // Load the initial best parameters
        GeneratorParameters bestParams = LoadParametersFromLevelDataJson();
        System.Random rng;

        if (!SearchForBetterSeedMode)
        {
            rng = new System.Random(bestParams.BaseSeed);
        }
        else
        {
            bestParams.BaseSeed = startingSeed;
            rng = new System.Random(Guid.NewGuid().GetHashCode());
        }

        float bestScore = EvaluateParameters(bestParams);
        
        // --- Step 1: Initial random population ---
        for (int i = 0; i < PopulationSize; i++)
        {
            var p = RandomParameters(rng, TargetDifficulty); // New individual with its own seed
            float score = EvaluateParameters(p);

            population.Add((p, score));

            if (score > bestScore)
            {
                bestScore = score;
                bestParams = p;
            }
        }

        // --- Step 2: Evolutionary loop ---
        for (int g = 0; g < Generations; g++)
        {
            // Sort population by score descending
            population.Sort((a, b) => b.score.CompareTo(a.score));

            // Keep top 50%
            population = population.GetRange(0, PopulationSize / 2);
            int survivors = population.Count;

            for (int i = 0; i < survivors; i++)
            {
                var mutated = Mutate(population[i].p, rng, TargetDifficulty); // Seed preserved in mutate
                float score = EvaluateParameters(mutated);

                population.Add((mutated, score));

                if (score > bestScore)
                {
                    bestScore = score;
                    bestParams = mutated;
                    bestParams.BaseSeed = startingSeed;
                    Debug.Log("NEW BEST: " + bestScore + " | Seed: " + bestParams.BaseSeed);
                }
            }

            Debug.Log("Generation " + g + " best score: " + population[0].score);
        }
        
    }
    
    GeneratorParameters Sanitize(GeneratorParameters p)
    {
        if (p.MinGap >= p.MaxGap)
            p.MaxGap = p.MinGap + 1;

        if (p.MinPlatformLength >= p.MaxPlatformLength)
            p.MaxPlatformLength = p.MinPlatformLength + 1;

        if (p.MinPlatformHeightFromGround >= p.MaxPlatformHeightFromGround)
            p.MaxPlatformHeightFromGround = p.MinPlatformHeightFromGround + 1;
    
        if (p.MinGroundHeight >= p.MaxGroundHeight)
            p.MaxGroundHeight = p.MinGroundHeight + 2;

        return p;
    }
    
    private void RunOptimizerToBeatPreviousJson(int maxAttempts)
    {
        if (!SearchForBetterSeedMode)
        {
            Debug.LogError("Search mode disabled.");
            return;
        }

        LevelData before = LoadLevelDataJson();
        float originalScore = before.score;

        for (int i = 0; i < maxAttempts; i++)
        {
            int newSeed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);

            Debug.LogWarning($"Attempt {i + 1}/{maxAttempts} | Seed: {newSeed}");

            OptimizeGenerator(newSeed);

            LevelData after = LoadLevelDataJson();
            if (after.score > originalScore)
            {
                Debug.Log("Score beaten!");
                return;
            }
        }

        Debug.Log("No improvement found.");
    }
    
    private void RunOptimizerToBeatScore(int maxAttempts, float score)
    {
        if (!SearchForBetterSeedMode)
        {
            Debug.LogError("Search mode disabled.");
            return;
        }
        
        float desiredScore = score;

        for (int i = 0; i < maxAttempts; i++)
        {
            int newSeed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);

            Debug.LogWarning($"Attempt {i + 1}/{maxAttempts} | Seed: {newSeed}");

            OptimizeGenerator(newSeed);

            LevelData after = LoadLevelDataJson();

            if (after.score > desiredScore)
            {
                Debug.Log("Score beaten!");
                return;
            }
        }

        Debug.Log("No improvement found.");
    }
    
    private void SearchForTargetDifficulty(int maxAttempts, float tolerance = 0.05f)
    {
        float target = TargetDifficulty;

        for (int i = 0; i < maxAttempts; i++)
        {
            // generate a new random seed
            int seed = Guid.NewGuid().GetHashCode();
            
            // run your optimizer
            OptimizeGenerator(seed);

            // evaluate difficulty
            float difficulty = Generator.EstimateDifficulty();
            Debug.Log($"Attempt {i} | Seed {seed} | Difficulty {difficulty}");

            // stop if close enough to target
            if (Mathf.Abs(difficulty - target) <= tolerance)
            {
                Debug.Log("Target difficulty reached!");
                return;
            }
        }

        Debug.Log("Max attempts reached. Best match not guaranteed.");
    }
    
    private void SearchForLowestDifficulty(int maxAttempts)
    {
        float bestDifficulty = float.MaxValue;

        GeneratorParameters bestParams = null;
        int bestSeed = 0;

        for (int i = 0; i < maxAttempts; i++)
        {
            int seed = Guid.NewGuid().GetHashCode();

            OptimizeGenerator(seed);

            float difficulty = Generator.EstimateDifficulty();

            Debug.Log($"Attempt {i} | Seed {seed} | Difficulty {difficulty}");

            // keep easiest level
            if (difficulty < bestDifficulty)
            {
                bestDifficulty = difficulty;
                bestSeed = seed;

                // store best parameters
                bestParams = Generator.GetCurrentParameters();
                bestParams.BaseSeed = bestSeed;
            }
        }

        Debug.Log($"Easiest level found: {bestDifficulty} | Seed {bestSeed}");

        // restore easiest level
        if (bestParams != null)
        {
            float score = EvaluateParameters(bestParams);
            Generator.SaveLevel(bestParams, score);
        }
    }
    
    private void SearchForHighestDifficulty(int maxAttempts)
    {
        float bestDifficulty = float.MinValue;

        GeneratorParameters bestParams = null;
        int bestSeed = 0;

        for (int i = 0; i < maxAttempts; i++)
        {
            int seed = Guid.NewGuid().GetHashCode();

            OptimizeGenerator(seed);

            float difficulty = Generator.EstimateDifficulty();

            Debug.Log($"Attempt {i} | Seed {seed} | Difficulty {difficulty}");

            // keep easiest level
            if (difficulty > bestDifficulty)
            {
                bestDifficulty = difficulty;
                bestSeed = seed;

                // store best parameters
                bestParams = Generator.GetCurrentParameters();
                bestParams.BaseSeed = bestSeed;
            }
        }

        Debug.Log($"Easiest level found: {bestDifficulty} | Seed {bestSeed}");

        // restore easiest level
        if (bestParams != null)
        {
            float score = EvaluateParameters(bestParams);
            Generator.SaveLevel(bestParams, score);
        }
    }
    
    private GeneratorParameters LoadParametersFromLevelDataJson()
    {
        string path = Application.persistentDataPath + "/saved_level.json";
        
        if (!File.Exists(path))
        {
            Debug.LogWarning("No parameter json file was found. Returning a random set of parameters");
            System.Random rng = new System.Random(Seed);
            return RandomParameters(rng, TargetDifficulty);
        }

        string json = File.ReadAllText(path);

        return JsonUtility.FromJson<LevelData>(json).parameters;
    }
    
    private LevelData LoadLevelDataJson()
    {
        string path = Application.persistentDataPath + "/saved_level.json";
        
        if (!File.Exists(path))
        {
            Debug.LogWarning("No Level Data json file was found.");
            return null;
        }

        string json = File.ReadAllText(path);

        return JsonUtility.FromJson<LevelData>(json);
    }
}   
