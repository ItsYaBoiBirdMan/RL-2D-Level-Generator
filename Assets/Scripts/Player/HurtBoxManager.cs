using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HurtBoxManager : MonoBehaviour
{
    [SerializeField] private float Damage;
    [SerializeField] private float AttackDuration;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            other.gameObject.GetComponent<EnemyController>().TakeDamage(Damage);
        }
    }

    private void OnEnable()
    {
        Destroy(gameObject, AttackDuration);
    }
}
