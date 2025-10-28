using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;  // For Button, etc.
using TMPro;  // If using TextMeshPro

public class UpgradeUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Button[] tabButtons;  // Assign: [0] = Timer Tab, [1] = Dash Tab, [2] = Combat Tab
    [SerializeField] private GameObject[] sectionPanels;  // Assign: [0] = TimerPanel, [1] = DashPanel, [2] = CombatPanel
    [SerializeField] private Transform[] contentParents;  // Assign: The transforms inside each panel where items go (e.g., the panels themselves)
    [SerializeField] private GameObject upgradeItemPrefab;
    [SerializeField] private TextMeshProUGUI currencyText;  // Optional currency display

    private List<List<GameObject>> upgradeItemsPerSection = new List<List<GameObject>>();  // Tracks items per section
    private int activeTabIndex = 0;

    void Start()
    {
        if (UpgradeManager.Instance == null)
        {
            Debug.LogError("UpgradeManager not found!");
            return;
        }

        // Initialize lists for each section
        for (int i = 0; i < sectionPanels.Length; i++)
        {
            upgradeItemsPerSection.Add(new List<GameObject>());
        }

        PopulateUpgradesBySection();
        SetupTabs();
        SwitchTab(0);  // Start with first tab
        RefreshUI();
    }

    private void SetupTabs()
    {
        for (int i = 0; i < tabButtons.Length; i++)
        {
            int index = i;  // Capture for listener
            tabButtons[i].onClick.AddListener(() => SwitchTab(index));
        }
    }

    private void SwitchTab(int index)
    {
        // Deactivate all sections
        for (int i = 0; i < sectionPanels.Length; i++)
        {
            sectionPanels[i].SetActive(i == index);
            // Optional: Highlight tab button
            tabButtons[i].image.color = (i == index) ? Color.yellow : Color.white;
        }
        activeTabIndex = index;
    }

    private void PopulateUpgradesBySection()
    {
        // Define groups (adjust as needed)
        List<UpgradeType>[] groups = new List<UpgradeType>[]
        {
            new List<UpgradeType> { UpgradeType.MaxTimer, UpgradeType.TimerDecayRate, UpgradeType.KillRechargeAmount, UpgradeType.AnotherLife },  // Timer
            new List<UpgradeType> { UpgradeType.DashRange, UpgradeType.DashCooldown, UpgradeType.DashDamage, UpgradeType.DashPenalty, UpgradeType.FinalDash },  // Dash
            new List<UpgradeType> { UpgradeType.Crit, UpgradeType.ComboDamage, UpgradeType.EssenceGain, UpgradeType.ComboDuration, UpgradeType.Magnet }  // Resource
        };

        var allUpgrades = UpgradeManager.Instance.playerStatsConfig.upgrades;

        for (int section = 0; section < groups.Length; section++)
        {
            var group = groups[section];
            var contentParent = contentParents[section];
            var itemsList = upgradeItemsPerSection[section];

            // Clear existing
            foreach (var item in itemsList)
            {
                Destroy(item);
            }
            itemsList.Clear();

            foreach (var type in group)
            {
                var upgrade = allUpgrades.Find(u => u.type == type);
                if (upgrade == null) continue;

                GameObject item = Instantiate(upgradeItemPrefab, contentParent);
                itemsList.Add(item);

                TextMeshProUGUI[] texts = item.GetComponentsInChildren<TextMeshProUGUI>();
                Button upgradeButton = item.GetComponentInChildren<Button>();

                texts[0].text = type.ToString();  // Name
                UpdateItemTexts(item, type);  // Initial

                upgradeButton.onClick.AddListener(() =>
                {
                    if (UpgradeManager.Instance.TryUpgrade(type, out string message))
                    {
                        Debug.Log(message);
                        RefreshUI();
                    }
                    else
                    {
                        Debug.LogWarning(message);
                    }
                });
            }
        }
    }

    public void RefreshUI()
    {
        if (currencyText != null && CurrencyManager.Instance != null)
        {
            currencyText.text = "Essence: " + CurrencyManager.Instance.GetCurrency().ToString();
        }

        for (int section = 0; section < upgradeItemsPerSection.Count; section++)
        {
            var items = upgradeItemsPerSection[section];
            for (int i = 0; i < items.Count; i++)
            {
                UpgradeType type = GetTypeForSectionItem(section, i);
                UpdateItemTexts(items[i], type);
            }
        }
    }

    private UpgradeType GetTypeForSectionItem(int section, int itemIndex)
    {
        List<UpgradeType>[] groups = new List<UpgradeType>[]
        {
            new List<UpgradeType> { UpgradeType.MaxTimer, UpgradeType.TimerDecayRate, UpgradeType.KillRechargeAmount, UpgradeType.AnotherLife },
            new List<UpgradeType> { UpgradeType.DashRange, UpgradeType.DashCooldown, UpgradeType.DashDamage, UpgradeType.DashPenalty, UpgradeType.FinalDash },
            new List<UpgradeType> { UpgradeType.Crit, UpgradeType.ComboDamage, UpgradeType.EssenceGain, UpgradeType.ComboDuration, UpgradeType.Magnet }
        };
        return groups[section][itemIndex];
    }

    private void UpdateItemTexts(GameObject item, UpgradeType type)
    {
        TextMeshProUGUI[] texts = item.GetComponentsInChildren<TextMeshProUGUI>();
        Button upgradeButton = item.GetComponentInChildren<Button>();

        int currentLevel = UpgradeManager.Instance.GetCurrentLevel(type);
        int maxLevel = UpgradeManager.Instance.GetMaxLevel(type);
        float currentStat = UpgradeManager.Instance.ComputeStat(type);
        int nextCost = UpgradeManager.Instance.GetNextCost(type);

        var entry = UpgradeManager.Instance.playerStatsConfig.GetUpgrade(type);

        texts[1].text = $"Level: {currentLevel}/{maxLevel}";
        texts[2].text = $"Value: {entry.baseValue} + {entry.incrementPerLevel} per level (Current: {currentStat})";
        texts[3].text = $"Next Cost: {nextCost}";

        upgradeButton.interactable = (currentLevel < maxLevel);
    }
}