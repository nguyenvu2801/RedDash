using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeManager : GameSingleton<UpgradeManager>
{
    [SerializeField] private List<UpgradeData> allUpgrades = new List<UpgradeData>(); // Assign all upgrades here in Inspector
    private Dictionary<StatType, float> statModifiers = new Dictionary<StatType, float>();
    private Dictionary<string, int> purchasedLevels = new Dictionary<string, int>(); // Key: upgradeName
    private int essence = 0;

    public Action OnEssenceChanged;
    public Action<UpgradeData> OnUpgradePurchased;

    void Start()
    {
        LoadProgress();
        ApplyAllUpgrades();
    }

    private void ApplyAllUpgrades()
    {
        // Reset to defaults: 0 for additive, 1 for multiplicative reductions
        foreach (StatType stat in Enum.GetValues(typeof(StatType)))
        {
            bool isReduction = stat == StatType.TimerDecayRate || stat == StatType.DashCooldown || stat == StatType.DashRecovery;
            statModifiers[stat] = isReduction ? 1f : 0f;
        }

        foreach (var upgrade in allUpgrades)
        {
            if (purchasedLevels.TryGetValue(upgrade.upgradeName, out int level) && level > 0)
            {
                float totalValue = 0f;
                for (int i = 0; i < level; i++)
                {
                    totalValue += upgrade.levels[i].value;
                }

                if (upgrade.statType == StatType.LastBreath)
                {
                    statModifiers[upgrade.statType] = level > 0 ? 1f : 0f;
                }
                else if (statModifiers[upgrade.statType] == 1f) // Reduction stats
                {
                    statModifiers[upgrade.statType] *= (1f - totalValue); // e.g., 0.1 value = *0.9
                }
                else
                {
                    statModifiers[upgrade.statType] += totalValue;
                }
            }
        }
    }

    public bool CanPurchase(UpgradeData upgrade, int nextLevel)
    {
        if (nextLevel > upgrade.maxLevel) return false;
        foreach (var prereq in upgrade.prerequisites)
        {
            if (!purchasedLevels.TryGetValue(prereq.upgradeName, out int lvl) || lvl < prereq.maxLevel) return false;
        }
        return essence >= upgrade.levels[nextLevel - 1].cost;
    }

    public void PurchaseUpgrade(UpgradeData upgrade, int nextLevel)
    {
        if (!CanPurchase(upgrade, nextLevel)) return;
        essence -= upgrade.levels[nextLevel - 1].cost;
        purchasedLevels[upgrade.upgradeName] = nextLevel;
        ApplyAllUpgrades(); // Re-apply all for simplicity (efficient for small N)
        SaveProgress();
        OnEssenceChanged?.Invoke();
        OnUpgradePurchased?.Invoke(upgrade);
    }

    public int GetCurrentLevel(UpgradeData upgrade)
    {
        purchasedLevels.TryGetValue(upgrade.upgradeName, out int level);
        return level;
    }

    public void AddEssence(int amount)
    {
        essence += amount;
        OnEssenceChanged?.Invoke();
        SaveProgress();
    }

    public int GetEssence() => essence;

    public float GetStatModifier(StatType stat)
    {
        statModifiers.TryGetValue(stat, out float mod);
        return mod;
    }

    private void SaveProgress()
    {
        PlayerPrefs.SetInt("Essence", essence);
        foreach (var kv in purchasedLevels)
        {
            PlayerPrefs.SetInt("Upgrade_" + kv.Key, kv.Value);
        }
        PlayerPrefs.Save();
    }

    private void LoadProgress()
    {
        essence = PlayerPrefs.GetInt("Essence", 0);
        foreach (var upgrade in allUpgrades)
        {
            int lvl = PlayerPrefs.GetInt("Upgrade_" + upgrade.upgradeName, 0);
            if (lvl > 0) purchasedLevels[upgrade.upgradeName] = lvl;
        }
    }

    // For testing
    public void ResetProgress()
    {
        PlayerPrefs.DeleteAll();
        essence = 0;
        purchasedLevels.Clear();
        ApplyAllUpgrades();
    }
}