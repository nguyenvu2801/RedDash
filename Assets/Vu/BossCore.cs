using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossCore : MonoBehaviour
{
    [SerializeField] private int maxBossHP = 1200;
    private int currentBossHP;

    [SerializeField] private List<BossHeart> hearts = new List<BossHeart>();
    [HideInInspector] public BossSpawner spawner;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform[] shootPoints;
    [SerializeField] private float projectileSpeed = 8f;
    [SerializeField] private float shootInterval = 1.5f;

    [Header("Enemy Spawn Settings - Same style as SpawnEnemyManager")]
    [SerializeField] private List<PoolKey> enemyPoolKeys = new List<PoolKey>();
    [SerializeField] private float spawnRadius = 10f;
    [SerializeField] private int maxActiveEnemies = 12;
    [SerializeField] private float spawnInterval = 5f;
    [SerializeField] private int enemiesToSpawnPerWave = 8;
    [SerializeField] private float healthMultiplier = 1.5f;

    [Header("AoE Damage Settings")]
    [SerializeField] private float aoeDamageInterval = 4f;
    [SerializeField] private float aoeRadius = 2.5f;
    [Header("Player Reference")]
    private Transform player;

    [Header("Warning Indicator")]
    [SerializeField] private SpriteRenderer warningCirclePrefab;
    [SerializeField] private Color warningBaseColor = new Color(1f, 0.25f, 0.1f);
    [SerializeField] private float minAlpha = 0.2f;
    [SerializeField] private float maxAlpha = 0.9f;
    [SerializeField] private float pulseSpeed = 4f;
    [SerializeField] private float warningDuration = 1.8f;

    private bool isBossDead = false;
    private List<EnemyBase> activeSpawnedEnemies = new List<EnemyBase>();
    private float spawnTimer;
    private int spawnedThisWave;

    private List<SpriteRenderer> activeWarningCircles = new List<SpriteRenderer>();

    private void Awake()
    {
        
    }

    private void Start()
    {
        SoundManager.Instance.PlayMusic("Boss");
        currentBossHP = maxBossHP;

        // Auto-find all BossHeart objects in the scene
        BossHeart[] foundHearts = FindObjectsByType<BossHeart>(FindObjectsSortMode.None);
        hearts.Clear();
        foreach (var heart in foundHearts)
        {
            hearts.Add(heart);
        }

        Debug.Log("BossCore found " + hearts.Count + " hearts");

        foreach (var heart in hearts)
        {
            if (heart != null)
                heart.Initialize(this);
        }

        StartCoroutine(ProjectileAttackRoutine());
        StartCoroutine(AoeDamageRoutine());
        StartSpawningWave();
    }

    private void StartSpawningWave()
    {
        spawnedThisWave = 0;
        spawnTimer = 0f;
    }

    private void Update()
    {
        if (isBossDead || GameManager.Instance.IsGameOver) return;

        spawnTimer -= Time.deltaTime;

        if (spawnTimer <= 0f && activeSpawnedEnemies.Count < maxActiveEnemies && spawnedThisWave < enemiesToSpawnPerWave)
        {
            SpawnPooledEnemy();
            spawnedThisWave++;
            spawnTimer = spawnInterval;
        }
    }

    private void SpawnPooledEnemy()
    {
        if (enemyPoolKeys.Count == 0) return;

        PoolKey key = enemyPoolKeys[Random.Range(0, enemyPoolKeys.Count)];
        GameObject enemyObj = PoolManager.Instance.GetFromPool(key);
        if (enemyObj == null) return;

        // Spawn in circle around boss
        Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
        Vector3 spawnPos = transform.position + new Vector3(randomCircle.x, randomCircle.y, 0f);

        enemyObj.transform.position = spawnPos;

        EnemyBase enemy = enemyObj.GetComponent<EnemyBase>();
        if (enemy != null)
        {
            // Fix: Pass SpawnEnemyManager.Instance instead of "this"
            enemy.Initialize(SpawnEnemyManager.Instance, key, healthMultiplier);
            activeSpawnedEnemies.Add(enemy);
        }
    }

    public void DespawnEnemy(EnemyBase enemy)
    {
        if (activeSpawnedEnemies.Remove(enemy))
        {
            PoolManager.Instance.ReturnToPool(enemy.poolKey, enemy.gameObject);
        }
    }

    public void TakeDamage(int dmg)
    {
        if (isBossDead) return;
        Debug.Log(currentBossHP);
        currentBossHP -= dmg;
        DamagePopUpManager.Instance.ShowDamage(dmg, transform.position + Vector3.up * 2f);

        if (currentBossHP <= 0)
        {
            BossDefeated();
        }
    }

    private void BossDefeated()
    {
        if (spawner != null)
            spawner.OnBossDefeated();
        isBossDead = true;
        StopAllCoroutines();
        ClearAllWarnings();

        foreach (var enemy in activeSpawnedEnemies.ToArray())
        {
            if (enemy != null)
                DespawnEnemy(enemy);
        }
    }



    public void OnHeartDestroyed(BossHeart destroyedHeart)
    {
        if (isBossDead) return;

        hearts.Remove(destroyedHeart);
        Debug.Log(currentBossHP);
        TakeDamage(200);

        if (hearts.Count <= 2)
        {
            shootInterval = Mathf.Max(0.6f, shootInterval * 0.7f);
            spawnInterval = Mathf.Max(3f, spawnInterval * 0.7f);
        }

        if (hearts.Count == 0)
            BossDefeated();
    }

    private IEnumerator ProjectileAttackRoutine()
    {
        while (!isBossDead)
        {
            yield return new WaitForSeconds(shootInterval);

            // Cache player reference once (same way your enemies do it)
            if (player == null)
            {
                // Try to find player the same way EnemyBase does
                if (SpawnEnemyManager.Instance != null && SpawnEnemyManager.Instance.spawnCenter != null)
                    player = SpawnEnemyManager.Instance.spawnCenter;
                else if (GameManager.Instance != null)
                    player = GameManager.Instance.transform.Find("Player")?.GetComponent<Transform>(); // fallback

                if (player == null)
                {
                    Debug.LogWarning("BossCore: Player reference not found!");
                    yield return new WaitForSeconds(0.5f); // retry after delay
                    continue;
                }
            }

            // Now shoot toward the player
            foreach (var point in shootPoints)
            {
                if (point == null) continue;

                GameObject proj = Instantiate(projectilePrefab, point.position, Quaternion.identity);

                if (proj.TryGetComponent<Rigidbody2D>(out var rb))
                {
                    Vector2 direction = (player.position - point.position).normalized;
                    rb.velocity = direction * projectileSpeed;

                    // Optional: make projectile face the direction it's flying
                    float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                    proj.transform.rotation = Quaternion.Euler(0, 180, -angle);
                }
            }
        }
    }

    private IEnumerator AoeDamageRoutine()
    {
        while (!isBossDead)
        {
            yield return new WaitForSeconds(aoeDamageInterval);

            ClearAllWarnings();

        
               
                if (warningCirclePrefab != null)
                {
                    SpriteRenderer warning = Instantiate(warningCirclePrefab, this.transform.position, Quaternion.identity);
                    warning.transform.localScale = Vector3.one * aoeRadius * 2f;
                    warning.color = new Color(warningBaseColor.r, warningBaseColor.g, warningBaseColor.b, 0f);
                    activeWarningCircles.Add(warning);

                    StartCoroutine(PulseWarning(warning));
                }

                StartCoroutine(ApplyAoeAfterWarning(this.transform.position));
            
        }
    }

    private IEnumerator PulseWarning(SpriteRenderer warning)
    {
        float elapsed = 0f;
        while (elapsed < warningDuration && warning != null)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Sin(Time.time * pulseSpeed) * 0.5f + 0.5f;
            float alpha = Mathf.Lerp(minAlpha, maxAlpha, t);
            warning.color = new Color(warningBaseColor.r, warningBaseColor.g, warningBaseColor.b, alpha);
            yield return null;
        }
        if (warning != null)
            Destroy(warning.gameObject);
    }

    private IEnumerator ApplyAoeAfterWarning(Vector2 heartPosition)
    {
        yield return new WaitForSeconds(warningDuration);

        Collider2D[] hits = Physics2D.OverlapCircleAll(heartPosition, aoeRadius);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                TimerManager.Instance?.ReduceTime(30f);
            }
        }
    }

    private void ClearAllWarnings()
    {
        foreach (var w in activeWarningCircles)
        {
            if (w != null) Destroy(w.gameObject);
        }
        activeWarningCircles.Clear();
    }

    private void OnDisable()
    {
        ClearAllWarnings();
    }
}