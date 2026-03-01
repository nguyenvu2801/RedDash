using System.Collections.Generic;
using UnityEngine;

public class AugmentManager : GameSingleton<AugmentManager>
{
    [SerializeField] private AugmentSO augmentData;

    // How many times each augment has been upgraded
    private Dictionary<AugmentType, int> augmentLevels = new Dictionary<AugmentType, int>();

    public AugmentSO AugmentData => augmentData;

    public int GetAugmentLevel(AugmentType type)
    {
        augmentLevels.TryGetValue(type, out int level);
        return level;
    }

    public bool IsMaxed(AugmentType type)
    {
        var entry = augmentData.GetUpgrade(type);
        return entry != null && GetAugmentLevel(type) >= entry.maxLevel;
    }

    /// <summary>Applies one level of the augment and returns the new total bonus value.</summary>
    public void ApplyAugment(AugmentType type)
    {
        if (IsMaxed(type)) return;

        augmentLevels.TryGetValue(type, out int current);
        augmentLevels[type] = current + 1;

        var entry = augmentData.GetUpgrade(type);
        float totalBonus = augmentLevels[type] * entry.baseValue;

        Debug.Log($"[Augment] {type} upgraded to level {augmentLevels[type]}, total bonus: {totalBonus}");

        // Hook your actual game systems here:
        switch (type)
        {
            case AugmentType.IncreaseDamage:
                // e.g. PlayerStats.Instance.AddDamageBonus(entry.baseValue);
                break;
            case AugmentType.IncreaseCurrency:
                // e.g. CurrencyManager.Instance.AddMultiplier(entry.baseValue);
                break;
            case AugmentType.ReduceDashCD:
                // e.g. PlayerDash.Instance.ReduceCooldown(entry.baseValue);
                break;
                // ... add the rest
        }
    }

    /// <summary>Returns 3 random augments that are not yet maxed.</summary>
    public List<AugmentType> GetRandomChoices(int count = 3)
    {
        var available = new List<AugmentType>();
        foreach (var entry in augmentData.Augment)
        {
            if (!IsMaxed(entry.type))
                available.Add(entry.type);
        }

        // Shuffle
        for (int i = available.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (available[i], available[j]) = (available[j], available[i]);
        }

        return available.GetRange(0, Mathf.Min(count, available.Count));
    }
    public void ResetAugments()
    {
        augmentLevels.Clear();
        Debug.Log("[AugmentManager] All augments reset.");
    }
}