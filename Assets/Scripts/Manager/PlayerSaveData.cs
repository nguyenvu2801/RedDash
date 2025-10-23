using System.Collections.Generic;
using System;

[Serializable]
public class UpgradeLevelEntry
{
    public UpgradeType type;
    public int level;
}

[Serializable]
public class PlayerSaveData
{
    public List<UpgradeLevelEntry> upgradeLevels = new List<UpgradeLevelEntry>();
}