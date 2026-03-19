using System.Collections;
using UnityEngine;

public class ExplosiveEnemy : EnemyBase
{
    [SerializeField] private float explosionRange = 2.5f;
    [SerializeField] private float fuseTime = 2f;
    [SerializeField] private float explosionDamageTime = 12f;
    [SerializeField] private float explosionRadius = 4f;

    [SerializeField] private CharacterAnimator enemyAnimator;

    [Header("Warning Circle")]
    [SerializeField] private SpriteRenderer warningCircle;
    [SerializeField] private Color warningBaseColor = new Color(1f, 0.25f, 0.1f);
    [SerializeField] private float minAlpha = 0.2f;
    [SerializeField] private float maxAlpha = 0.9f;
    [SerializeField] private float pulseSpeed = 4f;

    private bool isPrimedForExplosion;
    private Coroutine fuseCoroutine;
    private Coroutine despawnCoroutine;

    protected override void Awake()
    {
        base.Awake();
        base.moveSpeed = 5f;
    }

    public override void Initialize(SpawnEnemyManager manager, PoolKey key, float healthMult = 1f)
    {
        base.Initialize(manager, key, healthMult);
        isPrimedForExplosion = false;
        StopTrackedCoroutines();

        if (warningCircle != null)
        {
            warningCircle.enabled = false;
            warningCircle.color = new Color(warningBaseColor.r, warningBaseColor.g, warningBaseColor.b, 0f);
        }

        if (enemyAnimator != null)
            enemyAnimator.PlayAnimation("Idle");
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
        if (!gameObject.activeInHierarchy) return;

        isPrimedForExplosion = true;
        rb.velocity = Vector2.zero;

        Vector2 toPlayer = player.position - transform.position;
        FacePlayer(toPlayer);

        if (enemyAnimator != null)
            enemyAnimator.PlayAnimation("Attack");

        if (warningCircle != null)
        {
            warningCircle.enabled = true;
            warningCircle.transform.localScale = Vector3.one * explosionRadius * 2f;
        }

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

            if (warningCircle != null)
            {
                float t = Mathf.Sin(Time.time * pulseSpeed) * 0.5f + 0.5f;
                float alpha = Mathf.Lerp(minAlpha, maxAlpha, t);
                warningCircle.color = new Color(warningBaseColor.r, warningBaseColor.g, warningBaseColor.b, alpha);
            }

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

        if (warningCircle != null)
        {
            warningCircle.color = new Color(1f, 0.95f, 0.6f, 1f);
            StartCoroutine(FadeOutWarning(0.35f));
        }

        despawnCoroutine = manager.RunCoroutine(DelayedDespawn());
    }

    private IEnumerator FadeOutWarning(float duration)
    {
        float elapsed = 0f;
        Color startColor = warningCircle.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            warningCircle.color = Color.Lerp(startColor, new Color(startColor.r, startColor.g, startColor.b, 0f), t);
            yield return null;
        }

        if (warningCircle != null)
            warningCircle.enabled = false;
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

        if (warningCircle != null)
            warningCircle.enabled = false;
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

private IEnumerator DelayedDespawn()
    {
        float deathAnimationLength = 0.7f;
        yield return new WaitForSeconds(deathAnimationLength);

        if (gameObject.activeInHierarchy)
        {
            manager?.DespawnEnemy(this);
        }
    }

}