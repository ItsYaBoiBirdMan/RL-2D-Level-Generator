using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TopDownEnemyController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float MoveSpeed;
    [SerializeField] private float DirectionChangeTime = 2f;

    [Header("Combat")]
    [SerializeField] private Image HpBar;
    [SerializeField] private float MaxHealthPoints;
    [SerializeField] private float KnockedBackDuration;

    [Header("Collision")]
    [SerializeField] private float WallCheckDistance;
    [SerializeField] private LayerMask WallLayer;

    [Header("Difficulty")]
    [SerializeField] private EnemyDifficultyAdjuster EDA;

    private float _currentHealthPoints;

    private Rigidbody2D _rb;

    private Vector2 _moveDir;
    private bool _isKnockedBack;
    private bool _hpBarVisible;

    private float _directionTimer;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();

        PickRandomDirection();
    }

    private void Start()
    {
        MaxHealthPoints = EDA.MaxHealthPoints;
        MoveSpeed = EDA.MovementSpeed;
        _currentHealthPoints = MaxHealthPoints;
    }

    private void FixedUpdate()
    {
        HandleMovement();
        CheckWallAhead();
    }

    // =========================
    // MOVEMENT
    // =========================

    private void HandleMovement()
    {
        if (_isKnockedBack) return;

        _directionTimer -= Time.fixedDeltaTime;

        if (_directionTimer <= 0f)
        {
            PickRandomDirection();
        }

        Vector2 cleanDirection = Vector2.zero;

        // Force exact cardinal movement
        if (Mathf.Abs(_moveDir.x) > 0)
        {
            cleanDirection = new Vector2(Mathf.Sign(_moveDir.x), 0f);
        }
        else if (Mathf.Abs(_moveDir.y) > 0)
        {
            cleanDirection = new Vector2(0f, Mathf.Sign(_moveDir.y));
        }

        _rb.velocity = cleanDirection * MoveSpeed;
    }

    private void PickRandomDirection()
    {
        Vector2[] directions =
        {
            Vector2.up,
            Vector2.down,
            Vector2.left,   
            Vector2.right
        };

        _moveDir = directions[Random.Range(0, directions.Length)];

        // Extra safety normalization
        _moveDir = _moveDir.normalized;

        _directionTimer = DirectionChangeTime;
    }

    private void CheckWallAhead()
    {
        Vector2 origin = transform.position;

        RaycastHit2D hit = Physics2D.Raycast(
            origin,
            _moveDir,
            WallCheckDistance,
            WallLayer
        );

        if (hit.collider != null)
        {
            PickRandomDirection();
        }
    }

    // =========================
    // DAMAGE
    // =========================

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
        HpBar.gameObject.SetActive(true);

        _hpBarVisible = true;

        HpBar.fillAmount = current / max;

        yield return new WaitForSeconds(2f);

        HpBar.gameObject.SetActive(false);

        _hpBarVisible = false;
    }

    // =========================
    // KNOCKBACK
    // =========================

    public void TakeKnockBack(Vector2 direction, float force)
    {
        StartCoroutine(KnockBackRoutine(direction, force));
    }

    private IEnumerator KnockBackRoutine(Vector2 direction, float force)
    {
        _isKnockedBack = true;

        _rb.velocity = Vector2.zero;

        _rb.AddForce(direction * force, ForceMode2D.Impulse);

        yield return new WaitForSeconds(KnockedBackDuration);

        _isKnockedBack = false;

        _rb.velocity = Vector2.zero;
    }

    // =========================
    // DIFFICULTY
    // =========================
}