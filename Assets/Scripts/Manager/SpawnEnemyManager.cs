using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnEnemyManager : GameSingleton<SpawnEnemyManager>
{
    [Header("Spawn Settings")]
    [SerializeField] private List<PoolKey> enemyPoolKeys = new List<PoolKey> { PoolKey.enemy }; // Add more keys for different monsters
    public Transform spawnCenter;
    [SerializeField] private float spawnRadius = 10f;
    [SerializeField] private int maxActiveEnemies = 15;
    [SerializeField] private float spawnInterval = 1f;

    private List<EnemyBase> activeEnemies = new List<EnemyBase>();
    private float spawnTimer;
    private int enemiesToSpawn;
    private int spawnedCount;
    private float healthMultiplier;

    public int ActiveEnemiesCount => activeEnemies.Count;
    public bool IsSpawningDone => spawnedCount >= enemiesToSpawn;

    void Start()
    {
        if (spawnCenter == null && GameObject.FindGameObjectWithTag("Player") != null)
            spawnCenter = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        if (GameManager.Instance.IsGameOver) return;

        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0f && activeEnemies.Count < maxActiveEnemies && spawnedCount < enemiesToSpawn)
        {
            SpawnEnemy();
            spawnedCount++;
            spawnTimer = spawnInterval;
        }
    }

    public void StartSpawning(int count, float healthMult)
    {
        enemiesToSpawn = count;
        spawnedCount = 0;
        healthMultiplier = healthMult;
        spawnTimer = 0f; // Start spawning immediately
    }

    public void RunCoroutine(IEnumerator routine)
    {
        StartCoroutine(routine);
    }

    private void SpawnEnemy()
    {
        PoolKey key = enemyPoolKeys[Random.Range(0, enemyPoolKeys.Count)];
        GameObject enemyObj = PoolManager.Instance.GetFromPool(key);
        if (enemyObj == null) return;

        Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
        Vector3 spawnPos = (spawnCenter != null)
            ? spawnCenter.position + new Vector3(randomCircle.x, randomCircle.y, 0f)
            : new Vector3(randomCircle.x, randomCircle.y, 0f);

        enemyObj.transform.position = spawnPos;
        EnemyBase enemy = enemyObj.GetComponent<EnemyBase>();
        if (enemy != null)
        {
            enemy.Initialize(this, key, healthMultiplier);
            activeEnemies.Add(enemy);
        }
    }

    public void DespawnEnemy(EnemyBase enemy)
    {
        if (activeEnemies.Contains(enemy))
            activeEnemies.Remove(enemy);
        PoolManager.Instance.ReturnToPool(enemy.poolKey, enemy.gameObject);
    }

   
}