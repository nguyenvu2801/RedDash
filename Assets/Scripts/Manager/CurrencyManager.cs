using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class PlayerCurrencyData
{
    public int currency;
}

public class CurrencyManager : GameSingleton<CurrencyManager>
{
    private PlayerCurrencyData playerData;
    private string savePath;

    // Augment bonus — starts at 1.0 (no bonus), grows with IncreaseCurrency augment
    private float currencyMultiplier = 1f;

    void Start()
    {
        savePath = Path.Combine(Application.persistentDataPath, "playerData.json");
        LoadCurrency();
    }

    private void LoadCurrency()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            playerData = JsonUtility.FromJson<PlayerCurrencyData>(json);
            Debug.Log("Loaded currency: " + playerData.currency);
        }
        else
        {
            playerData = new PlayerCurrencyData { currency = 0 };
            SaveCurrency();
            Debug.Log("New player data created with starting currency: 0");
        }
    }

    public void SaveCurrency()
    {
        string json = JsonUtility.ToJson(playerData);
        File.WriteAllText(savePath, json);
        Debug.Log("Saved currency: " + playerData.currency);
    }

    public int GetCurrency() => playerData.currency;

    /// <summary>Adds currency, automatically applying the augment multiplier.</summary>
    public void AddCurrency(int amount)
    {
        if (amount <= 0) return;

        int scaled = Mathf.RoundToInt(amount * currencyMultiplier);
        playerData.currency += scaled;
        SaveCurrency();
        Debug.Log($"[Currency] +{scaled} (base {amount} × {currencyMultiplier:F2}). Total: {playerData.currency}");
    }

    public bool SpendCurrency(int amount)
    {
        if (amount > 0 && playerData.currency >= amount)
        {
            playerData.currency -= amount;
            SaveCurrency();
            Debug.Log($"Spent {amount}. Total: {playerData.currency}");
            return true;
        }
        Debug.Log("Not enough currency!");
        return false;
    }

    /// <summary>Called by AugmentManager for IncreaseCurrency. e.g. 0.1 = +10% per level.</summary>
    public void AddCurrencyMultiplier(float bonus)
    {
        currencyMultiplier += bonus;
        Debug.Log($"[CurrencyManager] Multiplier now: {currencyMultiplier:F2}");
    }

    void OnApplicationQuit() => SaveCurrency();
}