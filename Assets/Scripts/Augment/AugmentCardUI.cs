using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AugmentCardUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI levelText;
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
        levelText.text = $"Level {currentLevel}  {currentLevel + 1} / {entry.maxLevel}";

        selectButton.onClick.RemoveAllListeners();
        selectButton.onClick.AddListener(() => onChosen?.Invoke(augmentType));
    }

    private string GetFriendlyName(AugmentType type) => type switch
    {
        AugmentType.IncreaseLifeForcedMax => "Expand Life Force",
        AugmentType.LifeForceGained => "Life Force Gain",
        AugmentType.ReduceDashCD => "Swift Dash",
        AugmentType.SuccessDash => "Dash Mastery",
        AugmentType.EnemyExplode => "Death Explosion",
        AugmentType.IncreaseDamage => "Raw Power",
        AugmentType.IncreaseCurrency => "Essence Hoard",
        _ => type.ToString()
    };

    private string GetDescription(AugmentType type, float baseVal) => type switch
    {
        AugmentType.IncreaseLifeForcedMax => $"+{baseVal * 100:0}% max Life Force",
        AugmentType.LifeForceGained => $"+{baseVal * 100:0}% Life Force on pickup",
        AugmentType.ReduceDashCD => $"-{baseVal * 100:0}% dash cooldown",
        AugmentType.SuccessDash => $"+{baseVal * 100:0}% dash success chance",
        AugmentType.EnemyExplode => $"Enemies explode for {baseVal} damage on death",
        AugmentType.IncreaseDamage => $"+{baseVal * 100:0}% damage",
        AugmentType.IncreaseCurrency => $"+{baseVal * 100:0}% essence gained",
        _ => ""
    };
}