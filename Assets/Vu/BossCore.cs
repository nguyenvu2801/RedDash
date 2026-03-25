using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossCore : MonoBehaviour
{
    [SerializeField] private int maxBossHP = 1200;
    private int currentBossHP;

    [SerializeField] private List<BossHeart> hearts = new List<BossHeart>();

    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform[] shootPoints;
    [SerializeField] private float projectileSpeed = 8f;
    [SerializeField] private float shootInterval = 1.5f;

    [SerializeField] private GameObject[] enemyPrefabsToSpawn;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private float spawnInterval = 8f;
    [SerializeField] private int enemiesPerSpawn = 2;

    [SerializeField] private float aoeDamageInterval = 2f;
    [SerializeField] private float aoeRadius = 2.5f;
    [SerializeField] private int aoeDamage = 15;

    private bool isBossDead = false;

    private void Awake()
    {
        currentBossHP = maxBossHP;
    }

    private void Start()
    {
        foreach (var heart in hearts)
        {
            if (heart != null)
                heart.Initialize(this);
        }

        StartCoroutine(ProjectileAttackRoutine());
        StartCoroutine(SpawnEnemiesRoutine());
        StartCoroutine(AoeDamageRoutine());
    }

    public void TakeDamage(int dmg)
    {
        if (isBossDead) return;

        currentBossHP -= dmg;
        DamagePopUpManager.Instance.ShowDamage(dmg, transform.position + Vector3.up * 2f);

        if (currentBossHP <= 0)
        {
            BossDefeated();
        }
    }

    private void BossDefeated()
    {
        isBossDead = true;
        StopAllCoroutines();
    }

    public void OnHeartDestroyed(BossHeart destroyedHeart)
    {
        if (isBossDead) return;

        hearts.Remove(destroyedHeart);

        TakeDamage(200);

        if (hearts.Count <= 2)
        {
            shootInterval = Mathf.Max(0.6f, shootInterval * 0.7f);
            spawnInterval = Mathf.Max(3.5f, spawnInterval * 0.7f);
        }

        if (hearts.Count == 0)
            BossDefeated();
    }

    private IEnumerator ProjectileAttackRoutine()
    {
        while (!isBossDead)
        {
            yield return new WaitForSeconds(shootInterval);

            foreach (var point in shootPoints)
            {
                if (point == null) continue;
                GameObject proj = Instantiate(projectilePrefab, point.position, Quaternion.identity);
                if (proj.TryGetComponent<Rigidbody2D>(out var rb))
                {
                    Vector2 dir = (point.position - transform.position).normalized;
                    rb.velocity = dir * projectileSpeed;
                }
            }
        }
    }

    private IEnumerator SpawnEnemiesRoutine()
    {
        while (!isBossDead)
        {
            yield return new WaitForSeconds(spawnInterval);

            for (int i = 0; i < enemiesPerSpawn; i++)
            {
                if (enemyPrefabsToSpawn.Length == 0 || spawnPoints.Length == 0) break;
                int p = Random.Range(0, enemyPrefabsToSpawn.Length);
                int s = Random.Range(0, spawnPoints.Length);

                Instantiate(enemyPrefabsToSpawn[p], spawnPoints[s].position, Quaternion.identity);
            }
        }
    }

    private IEnumerator AoeDamageRoutine()
    {
        while (!isBossDead)
        {
            yield return new WaitForSeconds(aoeDamageInterval);

            foreach (var heart in hearts)
            {
                if (heart == null || !heart.gameObject.activeInHierarchy) continue;

                Collider2D[] hits = Physics2D.OverlapCircleAll(heart.transform.position, aoeRadius);
                foreach (var hit in hits)
                {
                    if (hit.CompareTag("Player"))
                    {
                        TimerManager.Instance?.ReduceTime(30f);
                    }
                }
            }
        }
    }

}