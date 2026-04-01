using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AugmentCardUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Image iconImage;         // NEW: drag your UI Image here
    [SerializeField] private Image levelFillImage;    // NEW: Image set to Filled type
    [SerializeField] private Button selectButton;

    private AugmentType augmentType;
    private Action<AugmentType> onChosen;

    public void Setup(AugmentType type, Action<AugmentType> callback)
    {
        augmentType = type;
        onChosen = callback;

        int currentLevel = AugmentManager.Instance.GetAugmentLevel(type);
        var entry = AugmentManager.Instance.AugmentData.GetUpgrade(type);

        titleText.text = GetFriendlyName(type);
        descriptionText.text = GetDescription(type, entry.baseValue);

        // Icon
        if (iconImage != null && entry.icon != null)
        {
            iconImage.sprite = entry.icon;
            iconImage.enabled = true;
        }
        else if (iconImage != null)
        {
            iconImage.enabled = false;
        }

        // Level fill bar: fillAmount = currentLevel / maxLevel
        if (levelFillImage != null)
        {
            float fill = entry.maxLevel > 0
                ? (float)currentLevel / entry.maxLevel
                : 0f;
            levelFillImage.fillAmount = fill;
        }

        selectButton.onClick.RemoveAllListeners();
        selectButton.onClick.AddListener(() => onChosen?.Invoke(augmentType));
    }

    private string GetFriendlyName(AugmentType type) => type switch
    {
        AugmentType.IncreaseLifeForcedMax => "Expand Life Force",
        AugmentType.LifeForceGained => "Life Force Gain",
        AugmentType.ReduceDashCD => "Swift Dash",
        AugmentType.Combo => "Combo Mastery",
        AugmentType.Magnet => "Enormous Magnet",
        AugmentType.IncreaseDamage => "Raw Power",
        AugmentType.IncreaseCurrency => "Essence Hoard",
        _ => type.ToString()
    };

    private string GetDescription(AugmentType type, float baseVal) => type switch
    {
        AugmentType.IncreaseLifeForcedMax => $"+{baseVal * 100:0}% max Life Force",
        AugmentType.LifeForceGained => $"+{baseVal * 100:0}% Life Force on pickup",
        AugmentType.ReduceDashCD => $"-{baseVal * 100:0}% dash cooldown",
        AugmentType.Combo => $"+{baseVal * 100:0}s combo duration",
        AugmentType.Magnet => $"+{baseVal} magnet range",
        AugmentType.IncreaseDamage => $"+{baseVal * 100:0}% damage",
        AugmentType.IncreaseCurrency => $"+{baseVal * 100:0}% essence gained",
        _ => ""
    };
}