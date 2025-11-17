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
    private int baseMaxHP; // To store the original serialized value
    private int currentHP;
    private Transform player;
    private bool isStunned;
    private Rigidbody2D rb;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
        baseMaxHP = maxHP; // Store base on awake
    }

    public virtual void Initialize(SpawnEnemyManager manager, PoolKey key, float healthMult = 1f)
    {
        this.manager = manager;
        this.poolKey = key;
        maxHP = Mathf.CeilToInt(baseMaxHP * healthMult);
        currentHP = maxHP;
        isStunned = false;
    }

    protected virtual void Update()
    {
        if (GameManager.Instance.IsGameOver) return;

        if (!isStunned && player != null)
        {
            Vector2 dir = player.position - transform.position;
            float dist = dir.magnitude;
            if (dist < chaseRange)
            {
                rb.MovePosition(rb.position + dir.normalized * moveSpeed * Time.deltaTime);
            }
        }
    }

    public virtual void TakeDamage(int dmg)
    {
        currentHP -= dmg;
        if (currentHP <= 0)
            Die();
        else
            StartCoroutine(HitVFX());
    }

    protected IEnumerator HitVFX()
    {
        // TODO: Add sprite flash or hit effect
        yield return null;
    }

    protected virtual void Die()
    {
        StopAllCoroutines();
        isStunned = false;
        manager.DespawnEnemy(this);
    }

    public virtual void OnDashHit(Vector2 dashDirection, float power)
    {
        int baseDamage = Mathf.CeilToInt(power);
        float damageMultiplier = 1f;
        if (ComboManager.Instance != null)
            damageMultiplier = ComboManager.Instance.GetDamageMultiplier();
        int finalDamage = Mathf.CeilToInt(baseDamage * damageMultiplier);
        TakeDamage(finalDamage);
        // Notify combo manager (per-enemy)
        if (ComboManager.Instance != null)
            ComboManager.Instance.RegisterEnemyHit();
        // Knockback (unchanged except referencing manager coroutine)
        SpawnEnemyManager.Instance.RunCoroutine(Knockback(dashDirection, 0.3f * power));
    }

    private IEnumerator Knockback(Vector2 dir, float strength)
    {
        isStunned = true;
        float timer = 0.2f;
        while (timer > 0f)
        {
            if (!gameObject.activeInHierarchy) yield break; // stop early if despawned
            rb.MovePosition(rb.position + dir.normalized * strength * Time.deltaTime);
            timer -= Time.deltaTime;
            yield return null;
        }
        isStunned = false;
    }
}