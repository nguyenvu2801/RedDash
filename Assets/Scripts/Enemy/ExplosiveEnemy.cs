using System.Collections;
using UnityEngine;

public class ExplodingEnemy : EnemyBase
{
    [SerializeField] private float explosionRange = 2.5f;
    [SerializeField] private float fuseTime = 3f;
    [SerializeField] private float explosionDamageTime = 12f;
    [SerializeField] private float explosionRadius = 4f;

    private bool isPrimedForExplosion;
    private Coroutine fuseCoroutine;

    protected override void Awake()
    {
        base.Awake();
    }

    public override void Initialize(SpawnEnemyManager manager, PoolKey key, float healthMult = 1f)
    {
        base.Initialize(manager, key, healthMult);

        isPrimedForExplosion = false;
        if (fuseCoroutine != null)
        {
            manager.StopCoroutine(fuseCoroutine);
            fuseCoroutine = null;
        }
    }

    protected override void Update()
    {
        if (GameManager.Instance.IsGameOver || isDead || isStunned || player == null)
            return;

        Vector2 toPlayer = player.position - transform.position;
        float distanceSqr = toPlayer.sqrMagnitude;

        if (isPrimedForExplosion)
        {
            return;
        }

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

        fuseCoroutine = manager.RunCoroutine(FuseCountdown());
    }

    private IEnumerator FuseCountdown()
    {
        float remaining = fuseTime;

        while (remaining > 0f)
        {
            if (!gameObject.activeInHierarchy || isDead) yield break;

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
            if (TimerManager.Instance != null)
            {
                TimerManager.Instance.ReduceTime(explosionDamageTime);
            }
        }

        if (healthBar != null)
        {
            PoolManager.Instance?.ReturnToPool(PoolKey.enemyHealthBar, healthBar.gameObject);
        }

        PoolManager.Instance?.ReturnToPool(poolKey, gameObject);
    }

    protected override void Die()
    {
        if (isPrimedForExplosion)
        {
            if (fuseCoroutine != null)
                manager.StopCoroutine(fuseCoroutine);
            Explode();
        }
        else
        {
            base.Die();
        }
    }

    private void OnDisable()
    {
        if (fuseCoroutine != null)
        {
            manager?.StopCoroutine(fuseCoroutine);
            fuseCoroutine = null;
        }
        isPrimedForExplosion = false;
    }
}