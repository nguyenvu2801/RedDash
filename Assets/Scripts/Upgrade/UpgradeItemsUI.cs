using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeItemsUI : MonoBehaviour
{
    [Header("References (assign in prefab)")]
    public Image iconImage;
    public Image progressFillImage;        // Filled Image (Horizontal
    public Button upgradeButton;
    public TextMeshProUGUI buttonCostText;

    private UpgradeType upgradeType;

    public void Initialize(UpgradeType type)
    {
        upgradeType = type;
        Refresh();

        // Clear old listeners and add new one
        upgradeButton.onClick.RemoveAllListeners();
        upgradeButton.onClick.AddListener(TryPerformUpgrade);
    }

    private void TryPerformUpgrade()
    {
        if (UpgradeManager.Instance.TryUpgrade(upgradeType, out string msg))
        {
            Debug.Log(msg);
        }
        else
        {
            Debug.LogWarning(msg);
        }
        RefreshAllUI(); // Refresh everything after click
    }

    public void Refresh()
    {
        var entry = UpgradeManager.Instance.playerStatsConfig.GetUpgrade(upgradeType);
        if (entry == null) return;

        int current = UpgradeManager.Instance.GetCurrentLevel(upgradeType);
        int max = UpgradeManager.Instance.GetMaxLevel(upgradeType);
        float stat = UpgradeManager.Instance.ComputeStat(upgradeType);
        int cost = UpgradeManager.Instance.GetNextCost(upgradeType);

        // Icon
        if (iconImage != null && entry.icon != null)
            iconImage.sprite = entry.icon;

        // Progress bar
        if (progressFillImage != null)
        {
            progressFillImage.fillAmount = max > 0 ? (float)current / max : 1f;
        }
        // Button cost / MAX
        if (buttonCostText != null)
            buttonCostText.text = current >= max ? "MAX" : cost.ToString();

        // Interactable + visual feedback
        bool canAfford = CurrencyManager.Instance.GetCurrency() >= cost;
        bool isMaxed = current >= max;
        upgradeButton.interactable = !isMaxed && canAfford;

        if (iconImage != null)
            iconImage.color = upgradeButton.interactable ? Color.white : new Color(0.5f, 0.5f, 0.5f);
    }

    private void RefreshAllUI()
    {
        FindObjectOfType<UpgradeUI>()?.RefreshUI();
    }
}