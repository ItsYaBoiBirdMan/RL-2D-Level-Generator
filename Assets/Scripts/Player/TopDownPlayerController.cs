using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TopDownPlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed;
    [SerializeField] private float MaxHealth;
    [SerializeField] private UIManager UiManager;
    [SerializeField] private float attackRadius;
    [SerializeField] private int damage;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private GameObject AttackVisualizer;

    private float _currentHealth;
    private bool _isDead;
    
    
    private Rigidbody2D _rb;

    private void Start()
    {
        _rb = GetComponent<Rigidbody2D>();

        _currentHealth = MaxHealth;
    }
    

    public void Attack()
    {
        // Gets all colliders inside the circle
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(
            transform.position,
            attackRadius,
            enemyLayer
        );

        foreach (Collider2D enemy in hitEnemies)
        {
            // Try getting enemy health script
            TopDownEnemyController enemyController = enemy.GetComponent<TopDownEnemyController>();

            if (enemyController != null)
            {
                enemyController.TakeDamage(damage);
            }
        }

        Debug.Log("AOE Attack Hit " + hitEnemies.Length + " enemies!");
    }

    private void TakeDamageAndUpdateUI(float dmg)
    {
        _currentHealth -= dmg;
        UiManager.UpdateHealthBar(_currentHealth, MaxHealth);
    }
    
    private IEnumerator KillPlayerRoutine()
    {
        _isDead = true;
        _rb.velocity = Vector2.zero;
        _rb.gravityScale = 0f;
        transform.GetComponent<BoxCollider2D>().isTrigger = true;
        UiManager.FadeToBlack();
        yield return new WaitForSeconds(1f);
        UiManager.FadeInGameOverScreen();
        EventManager.PlayerDeathEvent.Invoke();
    }
    private IEnumerator AttackRoutine()
    {
        AttackVisualizer.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        AttackVisualizer.SetActive(false);
    }
    
    void Update()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");

        Vector2 dir = new Vector2(x, y).normalized;
        
        _rb.velocity = new Vector2(dir.x * moveSpeed, dir.y * moveSpeed);
        
        if (Input.GetKeyDown(KeyCode.O))
        {
            Attack();
            StartCoroutine(AttackRoutine());
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("EnemyHurtBox"))
        {
            TakeDamageAndUpdateUI(10);
            if (_currentHealth <= 0) StartCoroutine(KillPlayerRoutine());
        }
        
        if (other.CompareTag("Hazard"))
        {
            TakeDamageAndUpdateUI(10);
            if (_currentHealth <= 0) StartCoroutine(KillPlayerRoutine());
        }

        if (other.CompareTag("Pickup"))
        {
            TakeDamageAndUpdateUI(-10f);
        }
    }
    
}
