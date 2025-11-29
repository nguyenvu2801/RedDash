using System.Collections;
using UnityEngine;

public class Thornling : EnemyBase
{
    [Header("Thorn Settings")]
    [SerializeField] private float thornRadius = 2.2f;
    [SerializeField] float timeDrainOnThornHit = 10f;
    [SerializeField] float thornTickRate = 0.3f;
    [SerializeField] float safeDuration = 4f;
    [SerializeField] float thornDuration = 1.5f;
    [SerializeField] float thornScaleMultiplier = 1.15f;

    [Header("Animation")]
    [SerializeField] private CharacterAnimator thornAnimator;

    private enum EnemyState { Safe, Thorny }
    private EnemyState currentState = EnemyState.Safe;
    private float stateTimer;
    private float damageTickTimer;
    private SpriteRenderer sr;
    private Vector3 baseScale;

    protected override void Awake()
    {
        base.Awake();
        sr = GetComponent<SpriteRenderer>();
        thornAnimator = GetComponent<CharacterAnimator>(); // Autofind on this GameObject
        if (thornAnimator == null)
        {
            Debug.LogError("CharacterAnimator component missing on Thornling: " + gameObject.name);
        }
        baseScale = transform.localScale;
    }

    public override void Initialize(SpawnEnemyManager manager, PoolKey key, float healthMult = 1f)
    {
        base.Initialize(manager, key, healthMult);
        currentState = EnemyState.Safe;
        stateTimer = safeDuration * Random.Range(0.8f, 1.2f);
        damageTickTimer = thornTickRate;
        SetSafeVisuals();
        if (thornAnimator != null)
            thornAnimator.PlayAnimation("Idle");
    }

    protected override void Update()
    {
        if (GameManager.Instance.IsGameOver || player == null || isStunned || isDead) return; 

        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0f)
        {
            currentState = currentState == EnemyState.Safe ? EnemyState.Thorny : EnemyState.Safe;
            stateTimer = currentState == EnemyState.Thorny ? thornDuration : safeDuration;

            if (currentState == EnemyState.Thorny)
            {
                SetThornyVisuals();
                thornAnimator?.PlayAnimation("Attack");
            }
            else
            {
                SetSafeVisuals();
                thornAnimator?.PlayAnimation("Idle");
            }
        }

        if (currentState == EnemyState.Safe)
            base.Update(); // This will now respect isDead
        else
            HandleThornDrain();
    }

    private void HandleThornDrain()
    {
        if (Vector2.Distance(transform.position, player.position) > thornRadius) return;
        damageTickTimer -= Time.deltaTime;
        if (damageTickTimer <= 0f)
        {
            TimerManager.Instance.ReduceTime(timeDrainOnThornHit);
            damageTickTimer = thornTickRate;
        }
    }

    private void SetSafeVisuals()
    {
        if (sr)
        {
           
            transform.localScale = baseScale;
        }
    }

    private void SetThornyVisuals()
    {
        if (sr)
        {
           
            transform.localScale = baseScale * thornScaleMultiplier;
        }
    }

    public override void TakeDamage(int dmg)
    {
        if (currentState == EnemyState.Thorny)
        {
            TimerManager.Instance.ReduceTime(timeDrainOnThornHit);
            return;
        }
        base.TakeDamage(dmg); // Triggers HitVFX  Hurt anim
    }

    public override void OnDashHit(Vector2 dashDirection, float power)
    {
        if (currentState == EnemyState.Thorny)
        {
            TimerManager.Instance.ReduceTime(timeDrainOnThornHit * 1.5f);
            return;
        }
        base.OnDashHit(dashDirection, power); // Triggers Hurt via TakeDamage
    }

    // Override to play Hurt animation + return to Idle after fixed time (like player)
    protected override IEnumerator HitVFX()
    {
        thornAnimator?.PlayAnimation("Hurt");
        yield return new WaitForSeconds(0.2f);
        thornAnimator?.PlayAnimation("Idle");
    }
    protected override void OnDeathStarted()
    {
        // Play death animation
        thornAnimator?.PlayAnimation("Die");

        // Stop any state timers
        stateTimer = 0f;
        currentState = EnemyState.Safe; // optional, just to be clean

        // Delay despawn so player can see the death animation
        StartCoroutine(DelayedDespawn());
    }

    private IEnumerator DelayedDespawn()
    {
        // Wait for the length of your Die animation (or a fixed time)
        float deathAnimLength = 0.6f; // Adjust to match your Die clip length
        yield return new WaitForSeconds(deathAnimLength);

        // Now actually return to pool
        SpawnEnemyManager.Instance.DespawnEnemy(this);
    }
}