using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PcgGameManager : MonoBehaviour
{
    [SerializeField] private UIManager UiManager;
    [SerializeField] private RLLevelTrainer RLGen;
    [SerializeField] private PlayerPerformanceTracker PerformanceTracker;
    
    private int _enemyCount;
    private int _coinCount;
    private void Start()
    {
        StartCoroutine(InitialRoutine());
    }

    private void UpdateEnemyCountOnEnemyDeath()
    {
        _enemyCount -= 1;
        UiManager.UpdateEnemyCounter(_enemyCount);

        if (_enemyCount <= 0)
        {
            Debug.Log("VICTORY");
            UiManager.FadeInVictoryScreen();
            PerformanceTracker.UpdatePerformance(true);
        }
    }
    
    private void UpdateCoinCountOnCoinCollected()
    {
        _coinCount -= 1;
        UiManager.UpdateCoinCounter(_coinCount);

        if (_coinCount <= 0)
        {
            Debug.Log("VICTORY");
            UiManager.FadeInVictoryScreen();
            PerformanceTracker.UpdatePerformance(true);
        }
    }

    private IEnumerator InitialRoutine()
    {
        yield return new WaitForSeconds(.025f);
        _enemyCount = RLGen.GetEnemyCount();
        _coinCount = RLGen.GetCoinCount();

        if (_enemyCount < _coinCount) UiManager.UpdateCoinCounter(_coinCount);
        else if (_enemyCount > _coinCount) UiManager.UpdateEnemyCounter(_enemyCount);
    }

    public void RestartGame()
    {
        RLGen.FlexibleGeneratorFunction();
    }

    public void QuitGame()
    {
        Debug.Log("QUIT GAME");
    }

    private void UpdatePlayerPerformanceOnDeath()
    {
        PerformanceTracker.UpdatePerformance(false);
    }

    private void OnEnable()
    {
        EventManager.EnemyDeathEvent.AddListener(UpdateEnemyCountOnEnemyDeath);
        EventManager.PlayerDeathEvent.AddListener(UpdatePlayerPerformanceOnDeath);
        EventManager.CoinCollectedEvent.AddListener(UpdateCoinCountOnCoinCollected);
    }
    
    private void OnDisable()
    {
        EventManager.EnemyDeathEvent.RemoveListener(UpdateEnemyCountOnEnemyDeath);
        EventManager.PlayerDeathEvent.RemoveListener(UpdatePlayerPerformanceOnDeath);
        EventManager.CoinCollectedEvent.RemoveListener(UpdateCoinCountOnCoinCollected);
    }
}
