using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimerManager : GameSingleton<TimerManager>
{
    [Header("Timer Settings")]
    public float maxTimer = 5f;
    public float currentTimer;
    private float maxTimerBonus = 0f;
    private float lifeForceGainMultiplier = 1f;
    public float decayRate = 1f; // per second
    public bool isActive = true;
    public event Action OnTimerDepleted;
    public event Action<float> OnTimerChanged;
    public event Action<float> OnPlayerDamaged;
    void Start()
    {
        currentTimer = maxTimer;
    }

    void Update()
    {
        if (GameManager.Instance.IsGameOver) return;

        if (!isActive) return;
        currentTimer -= decayRate * Time.deltaTime;
        currentTimer = Mathf.Clamp(currentTimer, 0, maxTimer);
        OnTimerChanged?.Invoke(currentTimer / maxTimer);
        if (currentTimer <= 0)
        {

            OnTimerDepleted?.Invoke();
            isActive = false;
            
        }
    }

    public void AddTime(float amount)
    {
        float scaled = amount * lifeForceGainMultiplier;
        currentTimer = Mathf.Min(currentTimer + scaled, maxTimer);
        OnTimerChanged?.Invoke(currentTimer / maxTimer);
    }

    public void AddMaxLifeForceBonus(float bonus)
    {
        float ratio = currentTimer / maxTimer;   // preserve fill %
        maxTimerBonus += bonus;
        maxTimer += bonus;                        // grow the bar
        currentTimer = maxTimer * ratio;
        OnTimerChanged?.Invoke(currentTimer / maxTimer);
    }

    public void AddLifeForceGainMultiplier(float bonus)
    {
        lifeForceGainMultiplier += bonus;         // e.g. 1.1, 1.2, 1.3...
    }

    public void ReduceTime(float seconds)
    {
        currentTimer = Mathf.Max(currentTimer - seconds, 0f);
        OnTimerChanged?.Invoke(currentTimer);
        OnPlayerDamaged?.Invoke(seconds);
        if (currentTimer <= 0f)
        {
            OnTimerDepleted?.Invoke();
            isActive = false;
        }
    }

    public void ModifyDecayRate(float modifier)
    {
        decayRate *= modifier; // for upgrades or buffs
    }
}