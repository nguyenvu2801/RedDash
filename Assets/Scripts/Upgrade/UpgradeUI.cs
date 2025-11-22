using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Button[] tabButtons;
    [SerializeField] private GameObject[] sectionPanels;
    [SerializeField] private Transform[] contentParents;
    [SerializeField] private GameObject upgradeItemPrefab;
    [SerializeField] private TextMeshProUGUI currencyText;

    private List<List<GameObject>> upgradeItemsPerSection = new List<List<GameObject>>();

    void Start()
    {
        for (int i = 0; i < sectionPanels.Length; i++)
            upgradeItemsPerSection.Add(new List<GameObject>());

        SetupTabs();
        PopulateAllItems();
        SwitchTab(0);
        RefreshUI();
    }

    private void SetupTabs()
    {
        for (int i = 0; i < tabButtons.Length; i++)
        {
            int index = i;
            tabButtons[i].onClick.AddListener(() => SwitchTab(index));
        }
    }

    private void SwitchTab(int index)
    {
        for (int i = 0; i < sectionPanels.Length; i++)
        {
            sectionPanels[i].SetActive(i == index);
            tabButtons[i].image.color = i == index ? Color.cyan : Color.white;
        }
    }

    private void PopulateAllItems()
    {
        List<UpgradeType>[] groups = new List<UpgradeType>[]
        {
            new List<UpgradeType> { UpgradeType.MaxTimer, UpgradeType.TimerDecayRate, UpgradeType.KillRechargeAmount, UpgradeType.AnotherLife },
            new List<UpgradeType> { UpgradeType.DashRange, UpgradeType.DashCooldown, UpgradeType.DashDamage, UpgradeType.DashPenalty, UpgradeType.FinalDash },
            new List<UpgradeType> { UpgradeType.Crit, UpgradeType.ComboDamage, UpgradeType.EssenceGain, UpgradeType.ComboDuration, UpgradeType.Magnet }
        };

        for (int s = 0; s < groups.Length; s++)
        {
            var list = upgradeItemsPerSection[s];
            foreach (Transform child in contentParents[s])
                Destroy(child.gameObject);
            list.Clear();

            foreach (var type in groups[s])
            {
                GameObject obj = Instantiate(upgradeItemPrefab, contentParents[s]);
                var itemScript = obj.GetComponent<UpgradeItemsUI>();
                itemScript.Initialize(type);
                list.Add(obj);
            }
        }
    }

    public void RefreshUI()
    {
        if (currencyText && CurrencyManager.Instance)
            currencyText.text = "Essence: " + CurrencyManager.Instance.GetCurrency();

        foreach (var section in upgradeItemsPerSection)
            foreach (var item in section)
                item.GetComponent<UpgradeItemsUI>()?.Refresh();
    }
}