using System;
using System.Collections;
using UnityEngine;

public class ComboManager : GameSingleton<ComboManager>
{ 

    [Header("Combo Settings")]
    [SerializeField] private int maxCombo = 999;
    [SerializeField] private float comboResetTime = 3.0f;         // seconds until combo resets
    [SerializeField] private float damageMultiplierPerCombo = 0.02f; // additive per combo step (e.g. 0.08 = +8% per combo)
    [SerializeField] private float extraTimePerEnemyBase = 0.25f; // extra time added per enemy hit (in seconds)
    [SerializeField] private AnimationCurve multiplierCurve = null; // optional curve to shape multiplier

    public Action<int, float> OnComboChanged; // (currentCombo, percentTimeLeft)
    public Action OnComboReset;

    int currentCombo = 0;
    float comboTimer = 0f;
    Coroutine comboCoroutine;

    void Start()
    {
        if (multiplierCurve == null)
        {
            // default linear-ish curve
            multiplierCurve = new AnimationCurve(new Keyframe(0, 1f), new Keyframe(10, 1.8f));
        }
    }

    void Update()
    {
        // optional: keep timer updating so UI can sample percent easily without coroutine
        if (currentCombo > 0)
        {
            comboTimer -= Time.deltaTime;
            OnComboChanged?.Invoke(currentCombo, Mathf.Clamp01(comboTimer / comboResetTime));
            if (comboTimer <= 0f)
            {
                ResetCombo();
            }
        }
    }

    public void RegisterEnemyHit()
    {
        currentCombo = Mathf.Min(currentCombo + 1, maxCombo);
        comboTimer = comboResetTime;

        OnComboChanged?.Invoke(currentCombo, 1f);

        // add extra time per enemy according to combo multiplier
        float multiplier = GetDamageMultiplier();
        float extraTime = extraTimePerEnemyBase * multiplier;
        if (TimerManager.Instance != null)
            TimerManager.Instance.AddTime(extraTime);
    }

    public int GetCombo() => currentCombo;

    public float GetDamageMultiplier()
    {
        // base multiplier from curve * additive per combo
        // Approach: use curve (indexed by combo) then add linear bonus.
        float curveVal = multiplierCurve.Evaluate(Mathf.Clamp(currentCombo, 0, multiplierCurve.keys[multiplierCurve.length - 1].time));
        float additive = 1f + (currentCombo - 1) * damageMultiplierPerCombo;
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
