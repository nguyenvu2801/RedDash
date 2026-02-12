using System;
using UnityEngine;

public class ComboManager : GameSingleton<ComboManager>
{
    [Header("Combo Settings")]
    [SerializeField] private int maxCombo = 999;
    [SerializeField] private float comboResetTime = 3.0f;
    [SerializeField] private float damageMultiplierPerCombo = 5f; // Now in whole percentage (5 = 5%)
    [SerializeField] private float extraTimePerEnemyBase = 0.25f;

    public Action<int, float> OnComboChanged; // (currentCombo, percentTimeLeft)
    public Action OnComboReset;

    private int currentCombo = 0;
    private float comboTimer = 0f;

    void Update()
    {
        if (currentCombo > 0)
        {
            comboTimer -= Time.deltaTime;
            OnComboChanged?.Invoke(currentCombo, Mathf.Clamp01(comboTimer / comboResetTime));

            if (comboTimer <= 0f)
                ResetCombo();
        }
    }

    public void RegisterEnemyHit()
    {
        currentCombo = Mathf.Min(currentCombo + 1, maxCombo);
        comboTimer = comboResetTime;

        OnComboChanged?.Invoke(currentCombo, 1f);

        // Optional: extra time scales with combo too (feels rewarding)
        float currentMultiplier = GetDamageMultiplier();
        float extraTime = extraTimePerEnemyBase * currentMultiplier;
        if (TimerManager.Instance != null)
            TimerManager.Instance.AddTime(extraTime);
    }

    public int GetCombo() => currentCombo;

    public float GetDamageMultiplier()
    {
        if (currentCombo <= 0) return 1f;

        // Convert percentage to decimal (5% becomes 0.05)
        float multiplier = 1f + (currentCombo - 1) * (damageMultiplierPerCombo / 100f);
        Debug.Log($"Combo: {currentCombo}, PerCombo: {damageMultiplierPerCombo}, Multiplier: {multiplier}");

        return Mathf.Max(1f, multiplier);
    }
    public void ResetCombo()
    {
        if (currentCombo >= 20)
        {
            int reward = currentCombo / 5; 
            CurrencyManager.Instance.AddCurrency(reward);
            Debug.Log($"Combo converted to currency: +{reward}");
        }
        currentCombo = 0;
        comboTimer = 0f;
        OnComboChanged?.Invoke(0, 0f);
        OnComboReset?.Invoke();
    }
}