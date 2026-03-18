using System.Collections;
using UnityEngine;

public class RangedEnemy : EnemyBase
{
    [Header("Ranged Settings")]
    [SerializeField] private float shootRange = 6f;           // Distance at which it prefers to shoot
    [SerializeField] private float shootDelay = 1f;           // Cooldown after shooting
    [SerializeField] private float projectileSpeed = 7f;
    [SerializeField] private int projectileDamage = 1;

    [Header("Projectile")]
    [SerializeField] private GameObject projectilePrefab;     

    private bool isInShootRange;
    private bool canShoot = true;
    private Coroutine shootCooldownRoutine;

    protected override void Awake()
    {
        base.Awake();
        // You can add ranged-specific Awake logic here if needed
    }

    public override void Initialize(SpawnEnemyManager manager, PoolKey key, float healthMult = 1f)
    {
        base.Initialize(manager, key, healthMult);

        // Reset ranged-specific state
        canShoot = true;
        isInShootRange = false;
        if (shootCooldownRoutine != null)
        {
            manager.StopCoroutine(shootCooldownRoutine);
            shootCooldownRoutine = null;
        }
    }

    protected override void Update()
    {
        if (GameManager.Instance.IsGameOver || isDead || isStunned || player == null)
            return;

        Vector2 toPlayer = player.position - transform.position;
        float distanceSqr = toPlayer.sqrMagnitude;

        // Decide behavior based on distance
        if (distanceSqr <= shootRange * shootRange)
        {
     
            isInShootRange = true;
            TryShoot(toPlayer.normalized);
        }
        else
        {
   
            isInShootRange = false;
            ChasePlayer(toPlayer);
        }
    }

    private void ChasePlayer(Vector2 direction)
    {
        FacePlayer(direction);
        rb.MovePosition(rb.position + direction.normalized * moveSpeed * Time.deltaTime);
    }

    private void TryShoot(Vector2 direction)
    {
        FacePlayer(direction);
        if (!canShoot) return;

        Shoot(direction);
        canShoot = false;

        // Start cooldown
        if (shootCooldownRoutine != null)
            manager.StopCoroutine(shootCooldownRoutine);

        shootCooldownRoutine = manager.RunCoroutine(ShootCooldown());
    }

    private void Shoot(Vector2 direction)
    {
        GameObject proj = PoolManager.Instance.GetFromPool(PoolKey.enemyProjectile);
        if (proj == null) return;

        proj.transform.position = transform.position;


        var projectile = proj.GetComponent<EnemyProjectile>();
        if (projectile != null)
        {
            projectile.Launch(direction.normalized, projectileSpeed, projectileDamage);
        }

    }

    private IEnumerator ShootCooldown()
    {
        yield return new WaitForSeconds(shootDelay);
        canShoot = true;
        shootCooldownRoutine = null;
    }
}