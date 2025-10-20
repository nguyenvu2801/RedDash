using System;
using UnityEngine;

public class ComboManager : GameSingleton<ComboManager>
{
    [Header("Base Combo Settings")]
    [SerializeField] private int maxCombo = 999;
    [SerializeField] private float baseComboResetTime = 3.0f;
    [SerializeField] private float baseDamageMultiplierPerCombo = 0.02f;
    [SerializeField] private float baseExtraTimePerEnemy = 0.25f;
    [SerializeField] private AnimationCurve multiplierCurve = null;

    private float effectiveComboResetTime;
    private float effectiveDamageMultiplierPerCombo;
    private float effectiveExtraTimePerEnemy;

    public Action<int, float> OnComboChanged;
    public Action OnComboReset;

    int currentCombo = 0;
    float comboTimer = 0f;

    void Start()
    {
        if (multiplierCurve == null)
        {
            multiplierCurve = new AnimationCurve(new Keyframe(0, 1f), new Keyframe(10, 1.8f));
        }
        ApplyUpgrades();
    }

    private void ApplyUpgrades()
    {
        if (UpgradeManager.Instance != null)
        {
            effectiveComboResetTime = baseComboResetTime + UpgradeManager.Instance.GetStatModifier(StatType.ComboBonusDuration);
            effectiveDamageMultiplierPerCombo = baseDamageMultiplierPerCombo + UpgradeManager.Instance.GetStatModifier(StatType.DashComboPower);
        }
        else
        {
            effectiveComboResetTime = baseComboResetTime;
            effectiveDamageMultiplierPerCombo = baseDamageMultiplierPerCombo;
            effectiveExtraTimePerEnemy = baseExtraTimePerEnemy;
        }
    }

    void Update()
    {
        if (currentCombo > 0)
        {
            comboTimer -= Time.deltaTime;
            OnComboChanged?.Invoke(currentCombo, Mathf.Clamp01(comboTimer / effectiveComboResetTime));
            if (comboTimer <= 0f)
            {
                ResetCombo();
            }
        }
    }

    public void RegisterEnemyHit()
    {
        currentCombo = Mathf.Min(currentCombo + 1, maxCombo);
        comboTimer = effectiveComboResetTime;

        OnComboChanged?.Invoke(currentCombo, 1f);

        float multiplier = GetDamageMultiplier();
        float extraTime = effectiveExtraTimePerEnemy * multiplier;
       
        if (UpgradeManager.Instance != null)
        {
            float critChance = UpgradeManager.Instance.GetStatModifier(StatType.CriticalDashChance);
            if (UnityEngine.Random.value < critChance)
            {
                TimerManager.Instance.AddTime(extraTime); // Double time placeholder
                // Add extra damage if applicable
            }
        }
    }

    public int GetCombo() => currentCombo;

    public float GetDamageMultiplier()
    {
        float curveVal = multiplierCurve.Evaluate(Mathf.Clamp(currentCombo, 0, multiplierCurve.keys[multiplierCurve.length - 1].time));
        float additive = 1f + (currentCombo - 1) * effectiveDamageMultiplierPerCombo;
        return Mathf.Max(1f, curveVal * additive);
    }

    public void ResetCombo()
    {
        currentCombo = 0;
        comboTimer = 0f;
        OnComboChanged?.Invoke(currentCombo, 0f);
        OnComboReset?.Invoke();
    }
}