using System.Collections;
using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    [HideInInspector] public PoolKey poolKey;
    protected SpawnEnemyManager manager;

    [Header("Enemy Stats")]
    [SerializeField] private int maxHP = 3;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float chaseRange = 8f;
    [Header("Experience Settings")]
    [SerializeField] private int baseExp = 10;
    private int baseMaxHP;
    private int currentHP;
    private Transform player;
    private bool isStunned;
    private Rigidbody2D rb;
    private EnemyHealthBar healthBar;

    // We track the coroutine handle so we can stop it later
    private Coroutine knockbackCoroutine;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;

        baseMaxHP = maxHP;
    }

    public virtual void Initialize(SpawnEnemyManager manager, PoolKey key, float healthMult = 1f)
    {
        this.manager = manager;
        this.poolKey = key;

        // Reset state from previous life (critical for pooling!)
        isStunned = false;
        StopCurrentKnockback();

        maxHP = Mathf.CeilToInt(baseMaxHP * healthMult);
        currentHP = maxHP;

        // Reuse health bar instead of destroying
        if (healthBar == null)
        {
            GameObject hbObj = PoolManager.Instance.GetFromPool(PoolKey.enemyHealthBar);
            healthBar = hbObj.GetComponent<EnemyHealthBar>();
        }

        healthBar.Init(transform); // ALWAYS re-init when enemy spawns
        healthBar.gameObject.SetActive(true);
        healthBar.SetHP(currentHP, maxHP);
    }

    protected virtual void Update()
    {
        if (GameManager.Instance.IsGameOver || isStunned || player == null) return;

        Vector2 dir = player.position - transform.position;
        if (dir.sqrMagnitude < chaseRange * chaseRange)
        {
            rb.MovePosition(rb.position + dir.normalized * moveSpeed * Time.deltaTime);
        }
    }

    public virtual void TakeDamage(int dmg)
    {
        currentHP -= dmg;
        healthBar?.SetHP(currentHP, maxHP);
        DamagePopUpManager.Instance.ShowDamage(dmg, transform.position);

        if (currentHP <= 0)
            Die();
        else
            manager.RunCoroutine(HitVFX());
    }

    protected virtual IEnumerator HitVFX()
    {
        // TODO: flash sprite, particles, etc.
        yield return null;
    }

    protected virtual void Die()
    {
        StopCurrentKnockback();
        isStunned = false;
        SpawnExperience();
        if (healthBar != null)
        {
            PoolManager.Instance.ReturnToPool(PoolKey.enemyHealthBar, healthBar.gameObject);
        }

        manager.DespawnEnemy(this);
    }

    public virtual void OnDashHit(Vector2 dashDirection, float power)
    {
        // Damage
        float mult = ComboManager.Instance != null ? ComboManager.Instance.GetDamageMultiplier() : 1f;
        int damage = Mathf.CeilToInt(power * mult);
        TakeDamage(damage);

        // Combo
        ComboManager.Instance?.RegisterEnemyHit();

        // Knockback - safely restart
        StopCurrentKnockback();
        knockbackCoroutine = manager.RunCoroutine(KnockbackRoutine(dashDirection.normalized, 0.3f * power));
    }

    private IEnumerator KnockbackRoutine(Vector2 direction, float strength)
    {
        isStunned = true;
        float timer = 0.2f;

        while (timer > 0f)
        {
            if (!gameObject.activeInHierarchy) // Despawned mid-knockback?
                yield break;

            rb.MovePosition(rb.position + direction * strength * Time.deltaTime);
            timer -= Time.deltaTime;
            yield return null;
        }

        isStunned = false;
        knockbackCoroutine = null;
    }

    private void StopCurrentKnockback()
    {
        if (knockbackCoroutine != null)
        {
            manager.StopCoroutine(knockbackCoroutine);
            knockbackCoroutine = null;
        }
        isStunned = false;
    }
    private void SpawnExperience()
    {
        if (PoolManager.Instance == null) return;

        GameObject expObj = PoolManager.Instance.GetFromPool(PoolKey.experience);
        expObj.transform.position = transform.position;

        ExperienceDrop drop = expObj.GetComponent<ExperienceDrop>();

        // Scale experience based on rooms passed
        int expValue = Mathf.CeilToInt(baseExp * (1f + 0.1f * RoomManager.Instance.CurrentRoomNumber));
        drop.Init(expValue);
    }
    // Extra safety when object is returned to pool
    private void OnDisable()
    {
        StopCurrentKnockback();
    }
}