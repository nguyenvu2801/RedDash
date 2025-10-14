using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : GameSingleton<EnemyManager>
{
    [Header("Spawn Settings")]
    [SerializeField] private PoolKey enemyPoolKey = PoolKey.enemy;
    [SerializeField] private Transform spawnCenter;
    [SerializeField] private float spawnRadius = 10f;
    [SerializeField] private int maxActiveEnemies = 15;
    [SerializeField] private float spawnInterval = 1f;

    private List<EnemyBase> activeEnemies = new List<EnemyBase>();
    private float spawnTimer;

    void Start()
    {
        if (spawnCenter == null && GameObject.FindGameObjectWithTag("Player") != null)
            spawnCenter = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0f && activeEnemies.Count < maxActiveEnemies)
        {
            SpawnEnemy();
            spawnTimer = spawnInterval;
        }
    }
    public void RunCoroutine(IEnumerator routine)
    {
        StartCoroutine(routine);
    }
    private void SpawnEnemy()
    {
        GameObject enemyObj = PoolManager.Instance.GetFromPool(enemyPoolKey);
        if (enemyObj == null) return;

        Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
        Vector3 spawnPos = (spawnCenter != null)
            ? spawnCenter.position + new Vector3(randomCircle.x, randomCircle.y, 0f)
            : new Vector3(randomCircle.x, randomCircle.y, 0f);

        enemyObj.transform.position = spawnPos;

        EnemyBase enemy = enemyObj.GetComponent<EnemyBase>();
        if (enemy != null)
        {
            enemy.Initialize(this, enemyPoolKey);
            activeEnemies.Add(enemy);
        }
    }

    public void DespawnEnemy(EnemyBase enemy)
    {
        if (activeEnemies.Contains(enemy))
            activeEnemies.Remove(enemy);

        PoolManager.Instance.ReturnToPool(enemy.poolKey, enemy.gameObject);
    }

    public void NotifyDashHit(Vector2 dashPos, Vector2 dashDir, float dashPower, float hitRadius = 1.5f)
    {
        // Use 2D physics overlap
        Collider2D[] hits = Physics2D.OverlapCircleAll(dashPos, hitRadius);

        foreach (var hit in hits)
        {
            EnemyBase enemy = hit.GetComponent<EnemyBase>();
            if (enemy != null)
            {
                enemy.OnDashHit(dashDir, dashPower);
            }
        }
    }
}
