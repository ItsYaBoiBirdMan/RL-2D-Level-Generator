using System;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class RLGeneratorAgent
{
    private Dictionary<int, float[]> Q = new();

    private float learningRate = 0.1f;
    private float epsilon = 0.2f;
    private int actionCount;

    public GeneratorAction Decide(GeneratorState state, bool[] allowedActions = null)
    {
        actionCount = Enum.GetValues(typeof(GeneratorAction)).Length;

        int key = state.GetHashCode();

        if (!Q.ContainsKey(key))
            Q[key] = new float[actionCount];

        // exploration
        if (Random.value < epsilon)
        {
            if (allowedActions == null)
                return (GeneratorAction)Random.Range(0, actionCount);

            List<int> valid = new();
            for (int i = 0; i < allowedActions.Length; i++)
                if (allowedActions[i])
                    valid.Add(i);

            return (GeneratorAction)valid[Random.Range(0, valid.Count)];
        }

        float[] values = Q[key];

        float bestValue = float.MinValue;
        List<int> bestIndices = new();

        for (int i = 0; i < values.Length; i++)
        {
            if (allowedActions != null && !allowedActions[i])
                continue;

            float value = values[i];
            
            if (value > bestValue)
            {
                bestValue = value;
                bestIndices.Clear();
                bestIndices.Add(i);
            }
            else if (value == bestValue)
            {
                bestIndices.Add(i);
            }
        }

        int best = bestIndices[Random.Range(0, bestIndices.Count)];
        return (GeneratorAction)best;
    }

    public void Learn(List<(GeneratorState, GeneratorAction)> episode, float reward)
    {
        float discount = 0.99f;

        for (int i = episode.Count - 1; i >= 0; i--)
        {
            var step = episode[i];

            int key = step.Item1.GetHashCode();

            if (!Q.ContainsKey(key))
                Q[key] = new float[actionCount];

            int actionIndex = (int)step.Item2;

            // intrinsic penalty for placing enemies    
            float adjustedReward = reward;
            if (step.Item2 == GeneratorAction.Enemy)
                adjustedReward -= 1f;

            Q[key][actionIndex] += learningRate * (adjustedReward - Q[key][actionIndex]);

            reward *= discount;
        }
    }
}
