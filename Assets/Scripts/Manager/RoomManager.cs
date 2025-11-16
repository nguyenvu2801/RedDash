using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomManager : GameSingleton<RoomManager>
{
    [Header("Room Settings")]
    [SerializeField] private GameObject rewardPrefab; // Assign a prefab with Reward script
    [SerializeField] private int baseEnemiesPerRoom = 10;
    [SerializeField] private int additionalEnemiesPerRoom = 5;
    [SerializeField] private float healthMultiplierPerRoom = 1.2f; // Multiplicative increase per room

    private int currentRoom = 1;
    private float currentHealthMultiplier = 1f;
    private bool roomActive = false;
    private int enemiesToSpawn;

    void Update()
    {
        if (GameManager.Instance.IsGameOver) return;

        if (roomActive && SpawnEnemyManager.Instance.ActiveEnemiesCount == 0 && SpawnEnemyManager.Instance)
        {
            RoomCleared();
        }
    }

    public void StartRoom()
    {
        currentHealthMultiplier *= healthMultiplierPerRoom; // Accumulate multiplier
        enemiesToSpawn = baseEnemiesPerRoom + (currentRoom - 1) * additionalEnemiesPerRoom;
        SpawnEnemyManager.Instance.StartSpawning(enemiesToSpawn, currentHealthMultiplier);
        TimerManager.Instance.isActive = true;
        roomActive = true;
    }

    private void RoomCleared()
    {
        roomActive = false;
        TimerManager.Instance.isActive = false;
        SpawnReward();
    }

    private void SpawnReward()
    {
        if (rewardPrefab != null && SpawnEnemyManager.Instance.spawnCenter != null)
        {
            Instantiate(rewardPrefab, SpawnEnemyManager.Instance.spawnCenter.position, Quaternion.identity);
        }
    }

    public void AdvanceToNextRoom()
    {
        currentRoom++;
        StartRoom();
    }
}
