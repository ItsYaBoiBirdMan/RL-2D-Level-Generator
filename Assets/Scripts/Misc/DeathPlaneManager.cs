using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathPlaneManager : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) EventManager.PlayerFallingOffMapEvent.Invoke();
        if (other.CompareTag("Enemy"))
        {
            EventManager.EnemyDeathEvent.Invoke();
            Destroy(other.gameObject);
        }
    }
}
