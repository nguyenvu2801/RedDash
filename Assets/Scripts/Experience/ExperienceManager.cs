using UnityEngine;

public class ExperienceManager : GameSingleton<ExperienceManager>
{
    public int currentLevel = 1;
    public int currentExp = 0;
    public int maxLevel = 10;

    public int baseExpPerLevel = 200; // starting required exp
    public float expGrowthPerLevel = 1.5f; // scales required exp per level
    public int roomsPassed = 0;

    public void AddExperience(int amount)
    {
        if (currentLevel >= maxLevel) return;

        // Scale experience by room
        int scaledAmount = Mathf.CeilToInt(amount * (1f + 0.1f * roomsPassed));

        currentExp += scaledAmount;

        // check level up
        int requiredExp = GetExpForLevel(currentLevel);
        if (currentExp >= requiredExp)
        {
            currentExp -= requiredExp;
            currentLevel++;
            OnLevelUp();
        }
    }

    public int GetExpForLevel(int level)
    {
        return Mathf.CeilToInt(baseExpPerLevel * Mathf.Pow(expGrowthPerLevel, level - 1));
    }

    private void OnLevelUp()
    {
        // placeholder, do nothing for now
        Debug.Log("Leveled Up! Current level: " + currentLevel);
    }

    public void SetRoomsPassed(int rooms)
    {
        roomsPassed = rooms;
    }
}
