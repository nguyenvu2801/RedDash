using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimerManager : GameSingleton<TimerManager>
{
    [Header("Base Timer Settings")]
    [SerializeField] private float baseMaxTimer = 5f;
    [SerializeField] private float baseDecayRate = 1f;
    [SerializeField] private float baseKillRechargeAmount = 1f; // Base for AddTime

    private float effectiveMaxTimer;
    private float effectiveDecayRate;
    private float effectiveHitRechargeAmount;
    private bool usedLastBreath = false; // To prevent repeat

    public float currentTimer;
    public bool isActive = true;

    public event Action OnTimerDepleted;
    public event Action<float> OnTimerChanged;

    void Start()
    {
        ApplyUpgrades();
        currentTimer = effectiveMaxTimer;
    }

    private void ApplyUpgrades()
    {
        if (UpgradeManager.Instance != null)
        {
            effectiveMaxTimer = baseMaxTimer + UpgradeManager.Instance.GetStatModifier(StatType.MaxTimer);
            effectiveDecayRate = baseDecayRate * UpgradeManager.Instance.GetStatModifier(StatType.TimerDecayRate);
            effectiveHitRechargeAmount = baseKillRechargeAmount + UpgradeManager.Instance.GetStatModifier(StatType.KillRechargeAmount);
        }
        else
        {
            effectiveMaxTimer = baseMaxTimer;
            effectiveDecayRate = baseDecayRate;
            effectiveHitRechargeAmount = baseKillRechargeAmount;
        }
    }

    void Update()
    {
        if (!isActive) return;

        currentTimer -= effectiveDecayRate * Time.deltaTime;
        currentTimer = Mathf.Clamp(currentTimer, 0, effectiveMaxTimer);
        OnTimerChanged?.Invoke(currentTimer / effectiveMaxTimer);

        if (currentTimer <= 0)
        {
            float grace = UpgradeManager.Instance?.GetStatModifier(StatType.DeathGraceWindow) ?? 0f;
            currentTimer += grace; // Apply grace
            if (currentTimer > 0) return;

            if (UpgradeManager.Instance != null && UpgradeManager.Instance.GetStatModifier(StatType.LastBreath) > 0 && !usedLastBreath)
            {
                usedLastBreath = true;
                currentTimer = 3f; // Placeholder: time for final dash
                // Trigger "final dash" mode/UI/sound
                return;
            }

            isActive = false;
            OnTimerDepleted?.Invoke();
        }
    }

    public void AddTime(float amount)
    {
        currentTimer = Mathf.Min(currentTimer + amount + effectiveHitRechargeAmount, effectiveMaxTimer);
        OnTimerChanged?.Invoke(currentTimer / effectiveMaxTimer);
    }

    public void ReduceTime(float seconds)
    {
        currentTimer = Mathf.Max(currentTimer - seconds, 0f);
        OnTimerChanged?.Invoke(currentTimer / effectiveMaxTimer);
    }

    public void ModifyDecayRate(float modifier)
    {
        effectiveDecayRate *= modifier; // Temp buffs
    }
}
