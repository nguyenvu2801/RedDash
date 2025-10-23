using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "PlayerStats", menuName = "PlayerStats", order = 1)]
public class PlayerStats :ScriptableObject
{
    [Serializable]
    public class UpgradeEntry
    {
        public UpgradeType type;
        public float baseValue;
        public float incrementPerLevel;
        public int maxLevel = 10;// Default max, override per entry
        [Header("Cost Settings")]
        public int baseCost = 100;
        public int costIncrement = 50;
        public float costMultiplier = 1.5f;

    }
    public List<UpgradeEntry> upgrades = new List<UpgradeEntry>
    {
        new UpgradeEntry { type = UpgradeType.MaxTimer,baseValue = 300f, incrementPerLevel = 30f,maxLevel =10,baseCost = 200},
        new UpgradeEntry { type = UpgradeType.TimerDecayRate,baseValue = 5.0f, incrementPerLevel = -0.3f,maxLevel =7,baseCost = 200},
        new UpgradeEntry { type = UpgradeType.AnotherLife,baseValue = 0f, incrementPerLevel = 1f,maxLevel =3,baseCost = 200},
        new UpgradeEntry { type = UpgradeType.DashRange,baseValue = 3f, incrementPerLevel = 0.5f,maxLevel =6,baseCost = 200},
        new UpgradeEntry { type = UpgradeType.DashPenalty,baseValue = 5f, incrementPerLevel = -0.2f,maxLevel =7,baseCost = 200},
        new UpgradeEntry { type = UpgradeType.DashCooldown,baseValue = 0.25f, incrementPerLevel = -0.05f,maxLevel =5,baseCost = 200},
        new UpgradeEntry { type = UpgradeType.Crit,baseValue = 0.01f, incrementPerLevel = 0.02f,maxLevel =10,baseCost = 200},
        new UpgradeEntry { type = UpgradeType.ComboDamage,baseValue = 0.01f, incrementPerLevel = 0.01f,maxLevel =5,baseCost = 200},
        new UpgradeEntry { type = UpgradeType.EssenceGain,baseValue = 0f, incrementPerLevel = 0.05f,maxLevel =10,baseCost = 200},
        new UpgradeEntry { type = UpgradeType.ComboDuration,baseValue = 3.0f, incrementPerLevel = 1f,maxLevel =7,baseCost = 200},
        new UpgradeEntry { type = UpgradeType.DashDamage,baseValue = 1f, incrementPerLevel = 1f,maxLevel =8,baseCost = 200},
        new UpgradeEntry { type = UpgradeType.Magnet,baseValue = 1.0f, incrementPerLevel = 0.5f,maxLevel =5,baseCost = 200},
        new UpgradeEntry { type = UpgradeType.FinalDash,baseValue = 0f, incrementPerLevel = 1f,maxLevel =3,baseCost = 200},
        new UpgradeEntry { type = UpgradeType.KillRechargeAmount,baseValue = 2.0f, incrementPerLevel = 0.5f,maxLevel =6,baseCost = 200},
    };
    public UpgradeEntry GetUpgrade(UpgradeType type)
    {
        return upgrades.Find(u => u.type == type);
    }
   
}
