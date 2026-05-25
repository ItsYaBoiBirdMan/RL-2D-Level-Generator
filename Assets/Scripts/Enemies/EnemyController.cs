using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class EnemyController : MonoBehaviour
{
    [SerializeField] private float PatrolSpeed;
    [SerializeField] private Image HpBar;
    [SerializeField] private float MaxHealthPoints;
    [SerializeField] private float KnockedBackDuration;
    [SerializeField] private float ForwardCheckDistance;
    [SerializeField] private float JumpForce;
    [SerializeField] private EnemyDifficultyAdjuster EDA;
    
    
    private float _currentHealthPoints;
    private Rigidbody2D _rb;
    private int _moveDir;
    private bool _isKnockedBack;
    private bool _hpBarVisible;
    
    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _moveDir = Random.value < 0.5f ? -1 : 1;
    }

    private void Start()
    {
        /*MaxHealthPoints = EDA.MaxHealthPoints;
        PatrolSpeed = EDA.MovementSpeed;*/
        _currentHealthPoints = MaxHealthPoints;
    }
    
    public void TakeDamage(float damage)
    {
        _currentHealthPoints -= damage;

        StartCoroutine(UpdateBarRoutine(_currentHealthPoints, MaxHealthPoints));

        if (_currentHealthPoints <= 0)
        {
            EventManager.EnemyDeathEvent.Invoke();
            Destroy(gameObject);
        }
    }

    private IEnumerator UpdateBarRoutine(float current, float max)
    {
        if (!_hpBarVisible)
        {
            HpBar.gameObject.SetActive(true);
            _hpBarVisible = true;
            HpBar.fillAmount = current / max;
            yield return new WaitForSeconds(2f);
            HpBar.gameObject.SetActive(false);
            _hpBarVisible = false;
        }
        else
        {
            HpBar.fillAmount = current / max;
            yield return new WaitForSeconds(2f);
            HpBar.gameObject.SetActive(false);
            _hpBarVisible = false;
        }
    }

    private void HandleMovement()
    {
        if(_isKnockedBack) return;
        _rb.velocity = new Vector2(_moveDir * PatrolSpeed, _rb.velocity.y);
    }

    public void TakeKnockBack(Vector2 direction, float force)
    {
        StartCoroutine(KnockBackRoutine(direction, force));
    }

    private IEnumerator KnockBackRoutine(Vector2 d, float f)
    {
        _isKnockedBack = true;
        _rb.velocity = Vector2.zero;
        _rb.AddForce(d * f, ForceMode2D.Impulse);
        yield return new WaitForSeconds(KnockedBackDuration);
        _isKnockedBack = false;
        _rb.velocity = Vector2.zero;
    }

    private void CheckForStep()
    {
        Vector2 dir = new Vector2(_moveDir, 0f);
        Vector2 lowRayOrigin = transform.position;

        RaycastHit2D lowHit = Physics2D.Raycast(lowRayOrigin, dir, ForwardCheckDistance, LayerMask.GetMask("Ground"));
        
        if(lowHit.collider) Jump();
    }

    private void Jump()
    {
        _rb.velocity = new Vector2(_rb.velocity.x, JumpForce);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Edge")) _moveDir *= -1;
    }

    private void FixedUpdate()
    {
        HandleMovement();
        CheckForStep();
    }

    private void OnEnable()
    {
        MaxHealthPoints = EDA.MaxHealthPoints;
        PatrolSpeed = EDA.MovementSpeed;
    }
}
