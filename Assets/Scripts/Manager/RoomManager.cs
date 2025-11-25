using System.Collections;
using UnityEngine;
using TMPro;

public class RoomManager : GameSingleton<RoomManager>
{
    [Header("Room Settings")]
    [SerializeField] private GameObject rewardPrefab; // Assign a prefab with Reward script
    [SerializeField] private int baseEnemiesPerRoom = 8;
    [SerializeField] private int additionalEnemiesPerRoom = 4;
    [SerializeField] private float healthMultiplierPerRoom = 1.2f; // Multiplicative increase per room

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI enemiesLeftText; // Assign in inspector
    [SerializeField] private TextMeshProUGUI roomsPassedText;

    private int currentRoom = 1;
    private float currentHealthMultiplier = 1f;
    private bool roomActive = false;
    private int enemiesToSpawn;

    public int CurrentRoomNumber => currentRoom;

    private void Update()
    {
        if (GameManager.Instance.IsGameOver) return;
        // Update UI
        if (roomActive && enemiesLeftText != null)
        {
            enemiesLeftText.text = "Enemies Left: " + SpawnEnemyManager.Instance.ActiveEnemiesCount;
        }

        if (roomsPassedText != null)
        {
            roomsPassedText.text = "Rooms Passed: " + (currentRoom - 1);
        }

        if (roomActive && SpawnEnemyManager.Instance.IsRoomCleared)
        {
            RoomCleared();
        }
    }

    public void StartRoom()
    {
        currentHealthMultiplier = Mathf.Pow(healthMultiplierPerRoom, currentRoom - 1);
        enemiesToSpawn = baseEnemiesPerRoom + (currentRoom - 1) * additionalEnemiesPerRoom;

        // Spawn enemies
        SpawnEnemyManager.Instance.StartSpawning(enemiesToSpawn, currentHealthMultiplier);

        TimerManager.Instance.isActive = true;
        roomActive = true;
    }

    private void RoomCleared()
    {
        roomActive = false;
        TimerManager.Instance.isActive = false;

        // Spawn reward if any
        SpawnReward();
    }

    private void SpawnReward()
    {
        if (rewardPrefab != null && SpawnEnemyManager.Instance.spawnCenter != null)
        {
            // Use PoolManager if reward should be pooled
            GameObject reward = PoolManager.Instance.GetFromPool(PoolKey.reward);
            reward.transform.position = SpawnEnemyManager.Instance.spawnCenter.position;
            reward.SetActive(true);
        }
    }

    public void AdvanceToNextRoom()
    {
        currentRoom++;

        // Update experience system with rooms passed
        ExperienceManager.Instance.SetRoomsPassed(currentRoom - 1);

        StartRoom();
    }

    // Optional: Call this if you want to reset everything
    public void ResetRooms()
    {
        currentRoom = 1;
        currentHealthMultiplier = 1f;
        roomActive = false;
        enemiesToSpawn = 0;
        ExperienceManager.Instance.SetRoomsPassed(0);
    }
}
