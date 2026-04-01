using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AugmentType
{
    IncreaseLifeForcedMax,
    LifeForceGained,
    ReduceDashCD,
    Combo,
    Magnet,
    IncreaseDamage,
    IncreaseCurrency
}
[CreateAssetMenu(menuName = "Augments/AugmentData")]
public class AugmentSO : ScriptableObject
{
    [Serializable]
    public class AugmentEntry
    {
        public AugmentType type;
        public Sprite icon;
        public float baseValue;
        public int maxLevel = 5;

    }
    public List<AugmentEntry> Augment = new List<AugmentEntry>
    {
        new AugmentEntry { type = AugmentType.IncreaseLifeForcedMax,baseValue = 0.1f},
        new AugmentEntry { type = AugmentType.IncreaseCurrency,baseValue = 0.1f},
        new AugmentEntry { type = AugmentType.IncreaseDamage,baseValue = 0.05f},
        new AugmentEntry { type = AugmentType.LifeForceGained,baseValue = 0.1f},
        new AugmentEntry { type = AugmentType.Magnet,baseValue = 10f},
        new AugmentEntry { type = AugmentType.ReduceDashCD,baseValue = 0.1f},
        new AugmentEntry { type = AugmentType.Combo,baseValue = 0.05f},
    };
    public AugmentEntry GetUpgrade(AugmentType type)
    {
        return Augment.Find(u => u.type == type);
    }
}
