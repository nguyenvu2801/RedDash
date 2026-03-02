using System;
using UnityEngine;

public class ComboManager : GameSingleton<ComboManager>
{
    [Header("Combo Settings")]
    [SerializeField] private float baseComboDuration = 2f;
    [SerializeField] private float damageMultiplierPerCombo = 0.1f;
    [SerializeField] private int maxComboCount = 10;

    private float comboDurationBonus = 0f;

    private int currentCombo = 0;
    private float comboTimer = 0f;
    private bool comboActive = false;

    public event Action<int> OnComboChanged;
    public event Action OnComboReset;

    private float ComboDuration => baseComboDuration + comboDurationBonus;

    void Update()
    {
        if (!comboActive) return;

        comboTimer -= Time.deltaTime;

        if (comboTimer <= 0f)
            ResetCombo();
    }

    public void RegisterEnemyHit()
    {
        currentCombo = Mathf.Min(currentCombo + 1, maxComboCount);
        comboTimer = ComboDuration;
        comboActive = true;

        OnComboChanged?.Invoke(currentCombo);
        Debug.Log($"[Combo] Count: {currentCombo} | Multiplier: {GetDamageMultiplier():F2}x | Timer: {ComboDuration:F2}s");
    }

    public float GetDamageMultiplier()
    {
        return 1f + (currentCombo * damageMultiplierPerCombo);
    }

    public int GetComboCount() => currentCombo;

    public float GetComboTimerNormalized()
    {
        if (!comboActive) return 0f;
        return Mathf.Clamp01(comboTimer / ComboDuration);
    }

    public void ResetCombo()
    {
        currentCombo = 0;
        comboTimer = 0f;
        comboActive = false;

        OnComboChanged?.Invoke(0);
        OnComboReset?.Invoke();
        Debug.Log("[Combo] Reset.");
    }

    public void AddComboDurationBonus(float bonus)
    {
        comboDurationBonus += bonus;
        Debug.Log($"[Combo] Duration now: {ComboDuration:F2}s");
    }
}