using System.Collections.Generic;
using UnityEngine;

public class BossSpawner : MonoBehaviour
{
    [Header("Boss Prefab (must have BossCore component)")]
    [SerializeField] private GameObject bossPrefab;

    [Header("Preset Spawn Locations")]
    [SerializeField] private Transform[] presetSpawnPoints;

    [Header("Hearts (initially hidden)")]
    [SerializeField] private List<GameObject> heartObjects = new List<GameObject>();

    [Header("Objects to deactivate when boss spawns (make battlefield wider)")]
    [SerializeField] private List<GameObject> objectsToDeactivate = new List<GameObject>();

    private GameObject currentBossInstance;
    private BossCore currentBossCore;
    private bool bossHasSpawned = false;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P) && !bossHasSpawned)
        {
            SpawnBoss();
        }
    }

    public void SpawnBoss()
    {
        if (bossPrefab == null || presetSpawnPoints.Length == 0)
        {
            Debug.LogError("BossSpawner: Boss Prefab or Spawn Points are not assigned!");
            return;
        }

        // Pick a preset location (you can change to Random.Range if you want random)
        int index = 0; // change this number to pick different preset
        // int index = Random.Range(0, presetSpawnPoints.Length); // uncomment for random
        Vector3 spawnPosition = presetSpawnPoints[index].position;

        // 1. Deactivate objects to make battlefield wider
        foreach (var obj in objectsToDeactivate)
        {
            if (obj != null) obj.SetActive(false);
        }

        // 2. Activate all hearts
        foreach (var heart in heartObjects)
        {
            if (heart != null)
                heart.SetActive(true);
        }

        // 3. Spawn the boss
        currentBossInstance = Instantiate(bossPrefab, spawnPosition, Quaternion.identity);
        currentBossCore = currentBossInstance.GetComponent<BossCore>();

        if (currentBossCore != null)
        {
            currentBossCore.spawner = this; // link so boss can tell us when it dies
        }

        bossHasSpawned = true;
        Debug.Log("Boss spawned at preset location!");
    }

    // Called automatically by BossCore when boss dies
    public void OnBossDefeated()
    {
        // Deactivate remaining hearts
        foreach (var heart in heartObjects)
        {
            if (heart != null)
                heart.SetActive(false);
        }

        // Optional: reactivate battlefield objects
        foreach (var obj in objectsToDeactivate)
        {
            if (obj != null) obj.SetActive(true);
        }

        bossHasSpawned = false;
        currentBossInstance = null;
        currentBossCore = null;

        Debug.Log("Boss defeated - hearts hidden, battlefield restored");
    }
}