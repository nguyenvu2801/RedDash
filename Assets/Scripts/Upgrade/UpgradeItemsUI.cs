using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class UpgradeItemsUI : MonoBehaviour
{
    [Header("References (assign in prefab)")]
    public Image iconImage;
    public Image progressFillImage;        // Filled Image (Horizontal
    public Button upgradeButton;
    public TextMeshProUGUI buttonCostText;

    [Header("Level Dividers")]
    public Transform dividerParent;
    public GameObject dividerPrefab;
    private List<GameObject> dividers = new List<GameObject>();
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
        int cost = UpgradeManager.Instance.GetNextCost(upgradeType);

        // ICON stays
        if (iconImage != null && entry.icon != null)
            iconImage.sprite = entry.icon;

        // Progress bar
        if (progressFillImage != null)
            progressFillImage.fillAmount = max > 0 ? (float)current / max : 1f;

        // Spawn dividers only once
        if (dividers.Count == 0 && dividerParent != null && dividerPrefab != null)
            SpawnDividers(max);

        // Cost text
        if (buttonCostText != null)
            buttonCostText.text = current >= max ? "MAX" : cost.ToString();

        // Interactable
        bool canAfford = CurrencyManager.Instance.GetCurrency() >= cost;
        bool isMaxed = current >= max;

        upgradeButton.interactable = !isMaxed && canAfford;

        // Gray out only the button
        Image btnImage = upgradeButton.GetComponent<Image>();
        if (btnImage != null)
            btnImage.color = upgradeButton.interactable ? Color.white : new Color(0.5f, 0.5f, 0.5f);
    }
    private void SpawnDividers(int maxLevel)
    {
        // Cleanup
        foreach (Transform child in dividerParent)
            Destroy(child.gameObject);
        dividers.Clear();

        RectTransform barRect = progressFillImage.GetComponent<RectTransform>();
        float barWidth = barRect.rect.width;

        for (int i = 1; i < maxLevel; i++)
        {
            GameObject divider = Instantiate(dividerPrefab, dividerParent);
            RectTransform dividerRect = divider.GetComponent<RectTransform>();

            // Percent position (0..1)
            float t = (float)i / maxLevel;

            // Set position via anchor
            dividerRect.anchorMin = new Vector2(t, 0);
            dividerRect.anchorMax = new Vector2(t, 1);
            dividerRect.anchoredPosition = Vector2.zero;

            // Optional width
            dividerRect.sizeDelta = new Vector2(6f, 0f);

            dividers.Add(divider);
        }
    }

    private void RefreshAllUI()
    {
        FindObjectOfType<UpgradeUI>()?.RefreshUI();
    }
}