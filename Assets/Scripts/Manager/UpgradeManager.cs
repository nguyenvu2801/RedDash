using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance;

    [Header("Config")]
    [SerializeField] private PlayerStats playerStatsConfig;  // Single config now

    // Shared levels for ALL upgrades (as before)
    private Dictionary<UpgradeType, int> upgradeLevels = new Dictionary<UpgradeType, int>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);  // Persist this GameObject across scenes
        }

        // Load all levels
        LoadUpgradeLevels();

        // Initialize missing levels to 1
        foreach (var entry in playerStatsConfig.upgrades)
        {
            if (!upgradeLevels.ContainsKey(entry.type))
                upgradeLevels[entry.type] = 1;
        }
    }

    public bool TryUpgrade(UpgradeType type, out string message)
    {
        message = "";

        var entry = playerStatsConfig.GetUpgrade(type);
        if (entry == null)
        {
            message = "Invalid upgrade type.";
            return false;
        }

        int currentLevel = upgradeLevels.GetValueOrDefault(type, 1);
        if (currentLevel >= entry.maxLevel)
        {
            message = "Max level reached.";
            return false;
        }

        // Cost calculation (e.g., baseCost * currentLevel)
        int cost = entry.baseCost * currentLevel;

        // Check/spend resources (placeholder)
        if (!CurrencyManager.Instance.SpendCurrency(cost))
        {
            message = $"Not enough resources. Costs {cost}.";
            return false;
        }

        upgradeLevels[type] = currentLevel + 1;
        SaveUpgradeLevels();
        message = $"Upgraded successfully! Cost: {cost}.";
        return true;
    }

    // Compute stat (as before)
    public float ComputeStat(UpgradeType type)
    {
        var entry = playerStatsConfig.GetUpgrade(type);
        if (entry == null) return 0f;

        int level = upgradeLevels.GetValueOrDefault(type, 1);
        return entry.baseValue + (level - 1) * entry.incrementPerLevel;
    }

    // UI helpers (unchanged, but use playerStatsConfig)
    public int GetCurrentLevel(UpgradeType type) => upgradeLevels.GetValueOrDefault(type, 1);
    public int GetMaxLevel(UpgradeType type) => playerStatsConfig.GetUpgrade(type)?.maxLevel ?? 1;
    public int GetNextCost(UpgradeType type)
    {
        var entry = playerStatsConfig.GetUpgrade(type);
        int currentLevel = GetCurrentLevel(type);
        return entry.baseCost * currentLevel;  // Or your formula
    }

    // Save/Load (unchanged)
    private void SaveUpgradeLevels()
    {
        PlayerSaveData saveData = new PlayerSaveData();
        foreach (var kvp in upgradeLevels)
        {
            saveData.upgradeLevels.Add(new UpgradeLevelEntry { type = kvp.Key, level = kvp.Value });
        }

        string json = JsonUtility.ToJson(saveData);
        string path = Application.persistentDataPath + "/playerSave.json";
        File.WriteAllText(path, json);
    }

    private void LoadUpgradeLevels()
    {
        string path = Application.persistentDataPath + "/playerSave.json";
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            PlayerSaveData saveData = JsonUtility.FromJson<PlayerSaveData>(json);
            foreach (var entry in saveData.upgradeLevels)
            {
                upgradeLevels[entry.type] = entry.level;
            }
        }
    }

    void OnApplicationQuit()
    {
        SaveUpgradeLevels();
    }
}
