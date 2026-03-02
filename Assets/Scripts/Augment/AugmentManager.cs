using System.Collections.Generic;
using UnityEngine;

public class AugmentManager : GameSingleton<AugmentManager>
{
    [SerializeField] private AugmentSO augmentData;

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

    /// <summary>Get the total accumulated bonus for a given augment type.</summary>
    public float GetTotalBonus(AugmentType type)
    {
        var entry = augmentData.GetUpgrade(type);
        if (entry == null) return 0f;
        return GetAugmentLevel(type) * entry.baseValue;
    }

    public void ApplyAugment(AugmentType type)
    {
        if (IsMaxed(type)) return;

        augmentLevels.TryGetValue(type, out int current);
        augmentLevels[type] = current + 1;

        var entry = augmentData.GetUpgrade(type);
        float bonus = entry.baseValue; // single level bonus

        Debug.Log($"[Augment] {type} -> level {augmentLevels[type]}, bonus this level: {bonus}");

        switch (type)
        {
            case AugmentType.IncreaseDamage:
                // DashPower is computed from UpgradeManager; we store augment bonus separately
                Movement.player?.AddDashPowerBonus(bonus);
                break;

            case AugmentType.ReduceDashCD:
                Movement.player?.AddDashCooldownReduction(bonus);
                break;

            case AugmentType.IncreaseLifeForcedMax:
                TimerManager.Instance?.AddMaxLifeForceBonus(bonus);
                break;

            case AugmentType.LifeForceGained:
                TimerManager.Instance?.AddLifeForceGainMultiplier(bonus);
                break;

            case AugmentType.IncreaseCurrency:
                CurrencyManager.Instance?.AddCurrencyMultiplier(bonus);
                break;

            case AugmentType.Magnet:
                // CurrencyMagnet reads GetTotalBonus(Magnet) dynamically — no call needed here
                // but we log it for clarity
                Debug.Log($"[Augment] Magnet radius bonus now: {GetTotalBonus(AugmentType.Magnet)}");
                break;

            case AugmentType.Combo:
                ComboManager.Instance?.AddComboDurationBonus(bonus);
                break;
        }
    }

    public List<AugmentType> GetRandomChoices(int count = 3)
    {
        var available = new List<AugmentType>();
        foreach (var entry in augmentData.Augment)
        {
            if (!IsMaxed(entry.type))
                available.Add(entry.type);
        }

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