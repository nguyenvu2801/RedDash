using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "UpgradeData", menuName = "Game/UpgradeData", order = 1)]
public class UpgradeData : ScriptableObject
{
    public string upgradeName;
    [TextArea] public string description;
    public StatType statType;
    public List<UpgradeLevel> levels = new List<UpgradeLevel>();
    public List<UpgradeData> prerequisites = new List<UpgradeData>();
    public int maxLevel => levels.Count;
}
[Serializable]
public class UpgradeLevel
{
    public int cost;
    public float value; // Delta (e.g., 1 for +1s, 0.1 for 10% reduction)
}