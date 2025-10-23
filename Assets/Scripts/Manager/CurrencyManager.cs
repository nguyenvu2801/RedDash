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
            playerData = new PlayerCurrencyData { currency = 0 };  // Starting amount
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

    public int GetCurrency()
    {
        return playerData.currency;
    }

    public void AddCurrency(int amount)
    {
        if (amount > 0)
        {
            playerData.currency += amount;
            SaveCurrency();  // Save after change
            Debug.Log("Added " + amount + ". New total: " + playerData.currency);
        }
    }

    public bool SpendCurrency(int amount)
    {
        if (amount > 0 && playerData.currency >= amount)
        {
            playerData.currency -= amount;
            SaveCurrency();  // Save after change
            Debug.Log("Spent " + amount + ". New total: " + playerData.currency);
            return true;
        }
        Debug.Log("Not enough currency!");
        return false;
    }

    // Optional: Call this on application quit to ensure save
    void OnApplicationQuit()
    {
        SaveCurrency();
    }
}
