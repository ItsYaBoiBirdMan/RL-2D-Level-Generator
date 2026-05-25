using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.PlayerLoop;
using UnityEngine.Serialization;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed;
    [SerializeField] private float jumpHeight;
    [SerializeField] private float fallMultiplier;
    [SerializeField] private float timeToApex;
    [SerializeField] private float riseGravityMultiplier;

    [SerializeField] private Vector2 HitBoxSize;
    [SerializeField] private Vector2 HitBoxOffset;

    [SerializeField] private float AttackDuration;
    [SerializeField] private float AttackCooldown;
    [SerializeField] private float AttackKnockBackForce;
    [SerializeField] private float AttackDamage;
    [SerializeField] private float SurgeDuration;
    [SerializeField] private float AmountOfSurgePerHit;
    [SerializeField] private GameObject LeftHitVisualizer;
    [SerializeField] private GameObject RightHitVisualizer;

    [SerializeField] private Transform RespawnPoint;
    [SerializeField] private Vector2 KnockbackForce;
    [SerializeField] private float KnockbackDuration;
    [SerializeField] private float MaxHealth;
    [SerializeField] private float MaxSurgePoints;

    [SerializeField] private Color SpeedSurgeColor;
    [SerializeField] private Color PowerSurgeColor;
    [SerializeField] private Color DrainSurgeColor;
    
    [SerializeField] private UIManager UiManager;
    [SerializeField] private CameraController CamController;
    
    private Vector2 _moveDirection;
    private Vector2 _lookDirection;
    private bool _isGrounded;
    private bool _canDoubleJump;
    private float _gravity;
    private float _jumpVelocity;
    private bool _canAttack;
    private float _originalGravityScale;
    
    private float _currentHealth;
    private float _currentSurge;
    public bool _isDead;
    private bool _surgeFullyCharged;
    private bool _surgeActive;
    private bool _canDrain;
    
    private bool _hasSpeedPickup;
    private bool _hasPowerPickup;
    
    public bool _isKnockedBack; 
    
    private Coroutine _surgeBarColorShiftRoutine;
    
    
    private Rigidbody2D _rb;
    private SpriteRenderer _sr;
    private Color _originalColor;


    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _sr = GetComponent<SpriteRenderer>();
        _originalColor = _sr.color;

        _currentHealth = MaxHealth;
        _currentSurge = 0f;
        _surgeActive = false;
        _canDrain = false;
        _canAttack = true;
        _gravity = (2f * jumpHeight) / (timeToApex * timeToApex);
        _jumpVelocity = _gravity * timeToApex;

        _lookDirection = Vector2.right;
        _rb.gravityScale = _gravity / Mathf.Abs(Physics2D.gravity.y);
        _originalGravityScale = _rb.gravityScale;
    }

    private void Start()
    {
        _currentHealth = MaxHealth;
        _isDead = false;
    }

    private void Jump()
    {
        if(_isDead) return;
        if(_isKnockedBack) return;
        if (_isGrounded)
        {
            //_rb.AddForce(Vector2.up * jumpHeight, ForceMode2D.Impulse);
            _rb.velocity = new Vector2(_rb.velocity.x, _jumpVelocity);
            _canDoubleJump = true;
        }

        if (_canDoubleJump && !_isGrounded)
        {
            //_rb.AddForce(Vector2.up * jumpHeight, ForceMode2D.Impulse);
            _rb.velocity = new Vector2(_rb.velocity.x, _jumpVelocity);
            _canDoubleJump = false;
        }
    }

    private void MeleeAttack()
    {
        if(_isDead) return;
        if(!_canAttack) return;
        if(_isKnockedBack) return;
        if (_lookDirection == Vector2.left && _canAttack) StartCoroutine(LeftAttack());
        if (_lookDirection == Vector2.right && _canAttack) StartCoroutine(RightAttack());
    }
    
    public void SetAsGrounded()
    {
        _isGrounded = true;
    }
    
    public void SetAsInAir()
    {
        _isGrounded = false;
        _canDoubleJump = true;
    }
    
    private void HandleJumpPhysics()
    {
        if (_rb.velocity.y > 0)
        {
            _rb.velocity += Vector2.up * (Physics2D.gravity.y * (riseGravityMultiplier - 1) * Time.deltaTime);
        }
        
        if (_rb.velocity.y < 0)
            _rb.velocity += Vector2.up * (Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime);
    }

    private void Move()
    {
        if(_isDead) return;
        if(_isKnockedBack) return;
        _rb.velocity = new Vector2(_moveDirection.x * moveSpeed, _rb.velocity.y);
    }

    private IEnumerator LeftAttack()
    {
        _canAttack = false;
        LeftHitVisualizer.SetActive(true);
        HashSet<EnemyController> hitEnemies = new HashSet<EnemyController>();

        float timer = 0f;

        while (timer < AttackDuration)
        {
            Vector2 origin = (Vector2)transform.position - HitBoxOffset;

            Collider2D[] hits = Physics2D.OverlapBoxAll(
                origin,
                HitBoxSize,
                0f,
                LayerMask.GetMask("Enemy")
            );

            foreach (Collider2D hit in hits)
            {
                EnemyController ec = hit.GetComponent<EnemyController>();

                if (ec && !hitEnemies.Contains(ec))
                {
                    DamageEnemy(ec, -1);
                    hitEnemies.Add(ec);
                }
            }

            timer += Time.deltaTime;
            yield return null;
        }
        LeftHitVisualizer.SetActive(false);
        yield return new WaitForSeconds(AttackCooldown);
        _canAttack = true;
    }
    
    private IEnumerator RightAttack()
    {
        _canAttack = false;
        RightHitVisualizer.SetActive(true);
        HashSet<EnemyController> hitEnemies = new HashSet<EnemyController>();

        float timer = 0f;

        while (timer < AttackDuration)
        {
            Vector2 origin = (Vector2)transform.position + HitBoxOffset;

            Collider2D[] hits = Physics2D.OverlapBoxAll(
                origin,
                HitBoxSize,
                0f,
                LayerMask.GetMask("Enemy")
            );

            foreach (Collider2D hit in hits)
            {
                EnemyController ec = hit.GetComponent<EnemyController>();

                if (ec && !hitEnemies.Contains(ec))
                {
                    DamageEnemy(ec, 1);
                    hitEnemies.Add(ec);
                }
            }

            timer += Time.deltaTime;
            yield return null;
        }
        RightHitVisualizer.SetActive(false);
        yield return new WaitForSeconds(AttackCooldown);
        _canAttack = true;
    }

    private IEnumerator FallingOffOfMapCoroutine()
    {
        UiManager.FadeToBlack();
        ImmobilizePlayer();
        yield return new WaitForSeconds(1f);
        transform.position = RespawnPoint.position;
        CamController.SnapToTarget();
        UiManager.FadeFromBlack();
        RemobilizePlayer();
    }

    private void DamageEnemy(EnemyController enemy, int attackDirection)
    {
        // if attackDirection is -1 => left attack
        // if attackDirection is 1 => right attack
        if(!enemy) return;
        enemy.TakeKnockBack(new Vector2(attackDirection, 0), AttackKnockBackForce);
        enemy.TakeDamage(AttackDamage);

        if (_currentSurge < MaxSurgePoints && !_surgeFullyCharged && !_surgeActive) _currentSurge += AmountOfSurgePerHit;
        if (_currentSurge >= MaxSurgePoints)
        {
            _surgeFullyCharged = true;
            _surgeBarColorShiftRoutine = StartCoroutine(UiManager.SurgeBarColorShiftRoutine(1f, _surgeFullyCharged));
        }
        UiManager.UpdateSurgeBar(_currentSurge, MaxSurgePoints);
        if (_canDrain) DrainHealth(AttackDamage);
    }

    private IEnumerator KnockBackRoutine(Vector2 sourcePos)
    {
        _isKnockedBack = true;
        float dir = transform.position.x < sourcePos.x ? -1f : 1f;
        _rb.velocity = Vector2.zero;
        _rb.AddForce(new Vector2(dir * KnockbackForce.x, KnockbackForce.y), ForceMode2D.Impulse);
        yield return new WaitForSeconds(KnockbackDuration);
        _isKnockedBack = false;
    }

    private void TakeDamageAndKnockBackFromEnemy(Vector2 sourcePosition)
    {
        if(_isKnockedBack) return;
        StartCoroutine(KnockBackRoutine(sourcePosition));
        TakeDamageAndUpdateUI(10f);
    }

    private void TakeDamageAndUpdateUI(float dmg)
    {
        _currentHealth -= dmg;
        UiManager.UpdateHealthBar(_currentHealth, MaxHealth);
    }

    private IEnumerator SurgeColorRoutine(Color flash, float duration)
    {
        float timer = duration;
        float quarterTime = duration * 0.25f;

        _sr.color = flash;

        while (timer > quarterTime)
        {
            timer -= Time.deltaTime;
            yield return null;
        }

        bool showFlashColor = false;

        while (timer > 0f)
        {
            float t = timer / quarterTime;
            float flashInterval = Mathf.Lerp(0.3f, 0.05f, 1f - t);

            showFlashColor = !showFlashColor;
            _sr.color = showFlashColor ? flash : _originalColor;

            timer -= flashInterval;
            yield return new WaitForSeconds(flashInterval);
        }

        _sr.color = _originalColor;
    }
    

    private void DrainHealth(float health)
    {
        if (_currentHealth < MaxHealth) _currentHealth += health;
        else if (_currentHealth >= MaxHealth) _currentHealth = MaxHealth;
        UiManager.UpdateHealthBar(_currentHealth, MaxHealth);
    }

    private IEnumerator DrainSurgeRoutine()
    {
        StopCoroutine(_surgeBarColorShiftRoutine);
        _surgeFullyCharged = false;
        _surgeActive = true;
        _currentSurge = 0f;
        _canDrain = true;
        StartCoroutine(SurgeColorRoutine(DrainSurgeColor, SurgeDuration));
        UiManager.EmptySurgeBarWhenDrainActivated(SurgeDuration);
        yield return new WaitForSeconds(SurgeDuration);
        _surgeActive = false;
        _canDrain = false;
    }
    
    private IEnumerator SpeedSurgeRoutine()
    {
        StopCoroutine(_surgeBarColorShiftRoutine);
        _surgeFullyCharged = false;
        _surgeActive = true;
        _currentSurge = 0f;
        AttackCooldown = 0.1f;
        StartCoroutine(SurgeColorRoutine(SpeedSurgeColor, SurgeDuration));
        UiManager.EmptySurgeBarWhenSpeedActivated(SurgeDuration);
        yield return new WaitForSeconds(SurgeDuration);
        _surgeActive = false;
        AttackCooldown = 0.3f;
    }
    
    private IEnumerator PowerSurgeRoutine()
    {
        StopCoroutine(_surgeBarColorShiftRoutine);
        _surgeFullyCharged = false;
        _surgeActive = true;
        _currentSurge = 0f;
        AttackDamage *= 2;
        StartCoroutine(SurgeColorRoutine(PowerSurgeColor, SurgeDuration));
        UiManager.EmptySurgeBarWhenPowerActivated(SurgeDuration);
        yield return new WaitForSeconds(SurgeDuration);
        _surgeActive = false;
        AttackDamage *= 0.5f;
    }

    private void ActivateDrainSurge()
    {
        if (_surgeFullyCharged && !_surgeActive)
        {
            StopCoroutine(_surgeBarColorShiftRoutine);
            StartCoroutine(DrainSurgeRoutine());
        }
    }
    
    private void ActivateSpeedSurge()
    {
        if (_surgeFullyCharged && !_surgeActive)
        {
            StopCoroutine(_surgeBarColorShiftRoutine);
            StartCoroutine(SpeedSurgeRoutine());
        }
    }
    
    private void ActivatePowerSurge()
    {
        if (_surgeFullyCharged && !_surgeActive)
        {
            StopCoroutine(_surgeBarColorShiftRoutine);
            StartCoroutine(PowerSurgeRoutine());
        }
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

    private IEnumerator RespawnPlayerRoutine()
    {
        _isDead = false;
        _rb.gravityScale = _originalGravityScale;
        transform.position = RespawnPoint.position;
        _currentHealth = MaxHealth;
        transform.GetComponent<BoxCollider2D>().isTrigger = false;
        yield return new WaitForSeconds(2f);
        UiManager.FadeFromBlack();
    }

    private void HandleFallingOffMap()
    {
        TakeDamageAndUpdateUI(10f);
        StartCoroutine(_currentHealth <= 0 ? KillPlayerRoutine() : FallingOffOfMapCoroutine());
    }
    private void ImmobilizePlayer()
    {
        _isDead = true;
        _rb.velocity = Vector2.zero;
        _rb.gravityScale = 0f;
    }
    
    private void RemobilizePlayer()
    {
        _isDead = false;
        _rb.gravityScale = _originalGravityScale;
    }

    public void UpdateRespawnPoint(Vector2 newPoint)
    {
        RespawnPoint.position = newPoint;
    }
    
    public void RespawnPlayerFromGameOver()
    {
        StartCoroutine(RespawnPlayerRoutine());
    }
    
    private void Update()
    {
        //_moveDirection = moveKeyboard.action.ReadValue<Vector2>();
        
        if(Input.GetButtonDown("Jump"))
            Jump();
        if(Input.GetKeyDown(KeyCode.O))
            MeleeAttack();
        if(Input.GetKeyDown(KeyCode.UpArrow))
            ActivateSpeedSurge();
        if(Input.GetKeyDown(KeyCode.DownArrow))
            ActivatePowerSurge();
        if(Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow))
            ActivateDrainSurge();

        _moveDirection.x = Input.GetAxisRaw("Horizontal");
        
        if(_moveDirection == Vector2.left) _lookDirection = Vector2.left;
        if (_moveDirection == Vector2.right) _lookDirection = Vector2.right;
    }
    
    private void FixedUpdate()
    {
        Move();
        HandleJumpPhysics();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("EnemyHurtBox"))
        {
            TakeDamageAndKnockBackFromEnemy(other.transform.position);
            if (_currentHealth <= 0) StartCoroutine(KillPlayerRoutine());
        }
        
        if (other.CompareTag("Hazard"))
        {
            TakeDamageAndKnockBackFromEnemy(other.transform.position);
            if (_currentHealth <= 0) StartCoroutine(KillPlayerRoutine());
        }

        if (other.CompareTag("Pickup"))
        {
            TakeDamageAndUpdateUI(-10f);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector2 origin1 = (Vector2)transform.position + HitBoxOffset;
        Vector2 origin2 = (Vector2)transform.position - HitBoxOffset;
        Gizmos.DrawWireCube(origin1, HitBoxSize);
        Gizmos.DrawWireCube(origin2, HitBoxSize);
    }
    private void OnEnable()
    {
        /*jumpKeyboard.action.Enable();      // Enable the action so it can fire
        jumpKeyboard.action.performed += Jump;

        attackKeyboard.action.Enable();
        attackKeyboard.action.performed += MeleeAttack;

        drainSurgeKeyboard.action.Enable();
        drainSurgeKeyboard.action.performed += ActivateDrainSurge;

        speedSurgeKeyboard.action.Enable();
        speedSurgeKeyboard.action.performed += ActivateSpeedSurge;

        powerSurgeKeyboard.action.Enable();
        powerSurgeKeyboard.action.performed += ActivatePowerSurge;

        jumpKeyboard.action.performed += ctx => Debug.Log("Jump fired!");*/
        
        EventManager.PlayerFallingOffMapEvent.AddListener(HandleFallingOffMap);
    }

    private void OnDisable()
    {
        /*jumpKeyboard.action.performed -= Jump;
        jumpKeyboard.action.Disable();

        attackKeyboard.action.performed -= MeleeAttack;
        attackKeyboard.action.Disable();

        drainSurgeKeyboard.action.performed -= ActivateDrainSurge;
        drainSurgeKeyboard.action.Disable();

        speedSurgeKeyboard.action.performed -= ActivateSpeedSurge;
        speedSurgeKeyboard.action.Disable();

        powerSurgeKeyboard.action.performed -= ActivatePowerSurge;
        powerSurgeKeyboard.action.Disable();*/

        EventManager.PlayerFallingOffMapEvent.RemoveListener(HandleFallingOffMap);
    }
}
