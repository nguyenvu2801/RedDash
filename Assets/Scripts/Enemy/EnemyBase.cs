using System.Collections;
using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    [HideInInspector] public PoolKey poolKey;
    protected EnemyManager manager;

    [Header("Enemy Stats")]
    [SerializeField] private int maxHP = 3;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float chaseRange = 8f;

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
    }

    public virtual void Initialize(EnemyManager manager, PoolKey key)
    {
        this.manager = manager;
        this.poolKey = key;
        currentHP = maxHP;
        isStunned = false;
    }

    protected virtual void Update()
    {
        if (!isStunned && player != null)
        {
            Vector2 dir = player.position - transform.position;
            float dist = dir.magnitude;

            if (dist < chaseRange)
            {
                rb.MovePosition(rb.position + dir.normalized * moveSpeed * Time.deltaTime);

                // Optional: flip sprite to face player
                if (dir.x > 0)
                    transform.localScale = new Vector3(1, 1, 1);
                else
                    transform.localScale = new Vector3(-1, 1, 1);
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

        // --- Time reward on kill ---
        if (TimerManager.Instance != null)
        {
            // Base kill time (tweakable)
            float baseKillTime = 1f;

            // Add upgrade-based bonus (reuse HitRechargeAmount stat or create a new stat if you prefer)
            float upgradeBonus = 0f;
            if (UpgradeManager.Instance != null)
                upgradeBonus = UpgradeManager.Instance.GetStatModifier(StatType.KillRechargeAmount);

            // Optionally scale by combo multiplier (so a kill during a big combo gives more time)
            float comboMult = 1f;
            if (ComboManager.Instance != null)
                comboMult = ComboManager.Instance.GetDamageMultiplier();

            float totalTimeToAdd = (baseKillTime + upgradeBonus) * comboMult;
            TimerManager.Instance.AddTime(totalTimeToAdd);
        }

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
        EnemyManager.Instance.RunCoroutine(Knockback(dashDirection, 0.3f * power));
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
