using System;
using UnityEngine;

public class ComboManager : GameSingleton<ComboManager>
{
    [Header("Combo Settings")]
    [SerializeField] private int maxCombo = 999;
    [SerializeField] private float comboResetTime = 3.0f;

    [SerializeField] private float damageMultiplierPerCombo = 0.05f; 

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

    // CLEAN & SIMPLE — this is all you need now
    public float GetDamageMultiplier()
    {
        if (currentCombo <= 0) return 1f;

        // Every combo after the first gives +X%
        float multiplier = 1f + (currentCombo - 1) * damageMultiplierPerCombo;

        return Mathf.Max(1f, multiplier); // never go below 1x
    }

    public void ResetCombo()
    {
        currentCombo = 0;
        comboTimer = 0f;
        OnComboChanged?.Invoke(0, 0f);
        OnComboReset?.Invoke();
    }
}