using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;


public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject EnemyPrefab;
    [SerializeField] private List<Transform> EnemySpawnPoints;

    [SerializeField] private int MaxNumberOfEnemies;

    private void SpawnEnemies()
    {
        int spawnCount = Mathf.Min(MaxNumberOfEnemies, EnemySpawnPoints.Count);

        List<Transform> shufflePoints = new List<Transform>(EnemySpawnPoints);

        for (int i = 0; i < shufflePoints.Count; i++)
        {
            int rand = Random.Range(i, shufflePoints.Count);

            (shufflePoints[i], shufflePoints[rand]) = (shufflePoints[rand], shufflePoints[i]);
        }

        for (int i = 0; i < spawnCount; i++)
            Instantiate(EnemyPrefab, shufflePoints[i].position, shufflePoints[i].rotation);
    }

    private void Start()
    {
        SpawnEnemies();
    }

    public void RestartGame()
    {
        SceneManager.LoadScene("TestScene");
    }
}
