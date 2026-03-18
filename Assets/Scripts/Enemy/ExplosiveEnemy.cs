using System.Collections;
using UnityEngine;

public class ExplosiveEnemy : EnemyBase
{
    [SerializeField] private float explosionRange = 2.5f;
    [SerializeField] private float fuseTime = 3f;
    [SerializeField] private float explosionDamageTime = 12f;
    [SerializeField] private float explosionRadius = 4f;

    [Header("Animation")]
    [SerializeField] private CharacterAnimator enemyAnimator;

    private bool isPrimedForExplosion;
    private Coroutine fuseCoroutine;
    private Coroutine despawnCoroutine;

    protected override void Awake()
    {
        base.Awake();
    }

    public override void Initialize(SpawnEnemyManager manager, PoolKey key, float healthMult = 1f)
    {
        base.Initialize(manager, key, healthMult);
        isPrimedForExplosion = false;
        StopTrackedCoroutines();
    }

    protected override void Update()
    {
        if (GameManager.Instance.IsGameOver || isDead || isStunned || player == null)
            return;

        Vector2 toPlayer = player.position - transform.position;
        float distanceSqr = toPlayer.sqrMagnitude;

        if (isPrimedForExplosion)
            return;

        if (distanceSqr <= explosionRange * explosionRange)
        {
            PrimeExplosion();
        }
        else
        {
            rb.MovePosition(rb.position + toPlayer.normalized * moveSpeed * Time.deltaTime);
        }
    }

    private void PrimeExplosion()
    {
        isPrimedForExplosion = true;
        rb.velocity = Vector2.zero;

        Vector2 toPlayer = player.position - transform.position;
        FacePlayer(toPlayer);

        if (enemyAnimator != null)
            enemyAnimator.PlayAnimation("Attack");

        // Use manager coroutine so it survives pool deactivation
        fuseCoroutine = manager.RunCoroutine(StartFuseDelayed());
    }

    private IEnumerator StartFuseDelayed()
    {
        yield return null; // wait one frame

        if (!gameObject.activeInHierarchy || isDead || !isPrimedForExplosion)
            yield break;

        fuseCoroutine = manager.RunCoroutine(FuseCountdown());
    }

    private IEnumerator FuseCountdown()
    {
        float remaining = fuseTime;
        while (remaining > 0f)
        {
            if (!gameObject.activeInHierarchy || isDead)
                yield break;

            remaining -= Time.deltaTime;
            yield return null;
        }

        Explode();
    }

    private void Explode()
    {
        if (isDead) return;
        isDead = true;

        float distToPlayer = Vector2.Distance(transform.position, player.position);
        if (distToPlayer <= explosionRadius)
        {
            TimerManager.Instance?.ReduceTime(explosionDamageTime);
        }

        if (healthBar != null)
        {
            PoolManager.Instance?.ReturnToPool(PoolKey.enemyHealthBar, healthBar.gameObject);
        }

        if (enemyAnimator != null)
            enemyAnimator.PlayAnimation("Die");

        // Use manager coroutine so it survives pool deactivation
        despawnCoroutine = manager.RunCoroutine(DelayedDespawn());
    }

    private IEnumerator DelayedDespawn()
    {
        float deathAnimationLength = 0.7f;
        yield return new WaitForSeconds(deathAnimationLength);

        if (gameObject.activeInHierarchy)
        {
            manager?.DespawnEnemy(this);
        }
    }

    protected override void Die()
    {
        if (isPrimedForExplosion)
        {
            if (fuseCoroutine != null)
            {
                manager.StopCoroutine(fuseCoroutine);
                fuseCoroutine = null;
            }
            Explode();
        }
        else
        {
            base.Die();
        }
    }

    private void OnDisable()
    {
        StopTrackedCoroutines();
        isPrimedForExplosion = false;

        if (enemyAnimator != null)
            enemyAnimator.PlayAnimation("Idle");
    }

    private void StopTrackedCoroutines()
    {
        if (fuseCoroutine != null)
        {
            manager?.StopCoroutine(fuseCoroutine);
            fuseCoroutine = null;
        }

        if (despawnCoroutine != null)
        {
            manager?.StopCoroutine(despawnCoroutine);
            despawnCoroutine = null;
        }
    }
}