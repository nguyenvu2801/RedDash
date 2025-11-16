using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimerManager : GameSingleton<TimerManager>
{
    [Header("Timer Settings")]
    public float maxTimer = 5f;
    public float currentTimer;
    public float decayRate = 1f; // per second
    public bool isActive = true;
    public event Action OnTimerDepleted;
    public event Action<float> OnTimerChanged;

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
            isActive = false;
            OnTimerDepleted?.Invoke();
        }
    }

    public void AddTime(float amount)
    {
        currentTimer = Mathf.Min(currentTimer + amount, maxTimer);
        OnTimerChanged?.Invoke(currentTimer / maxTimer);
    }

    public void ReduceTime(float seconds)
    {
        currentTimer = Mathf.Max(currentTimer - seconds, 0f);
        OnTimerChanged?.Invoke(currentTimer);
    }

    public void ModifyDecayRate(float modifier)
    {
        decayRate *= modifier; // for upgrades or buffs
    }
}