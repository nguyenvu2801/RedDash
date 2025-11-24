using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class UpgradeDetailPanel : MonoBehaviour
{
    [Header("UI References")]
    public Image iconImage;
    public TMP_Text titleText;
    public TMP_Text descriptionText;
    public TMP_Text statText;

    [Header("Frame-by-Frame Animation")]
    public Image animationImage;            // Image that shows animation
    public Sprite[] animationFrames;        // Sprite sequence
    public float frameRate = 0.05f;         // Seconds per frame

    private Coroutine animationCoroutine;
    private bool isAnimating = false;
    private UpgradeType currentType;

    public static UpgradeDetailPanel Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        gameObject.SetActive(false);
    }

    public void Show(UpgradeType type)
    {
        // If already animating, ignore new requests
        if (isAnimating)
            return;

        // If panel is already active (means switching to another upgrade)
        if (gameObject.activeSelf && type != currentType)
        {
            // Play reverse animation first, then update info after it finishes
            if (animationCoroutine != null)
                StopCoroutine(animationCoroutine);

            animationCoroutine = StartCoroutine(SwitchUpgradeWithReverse(type));
        }
        else
        {
            // Normal show (first time opening)
            currentType = type;
            UpdateUIContent(type);

            gameObject.SetActive(true);

            if (animationCoroutine != null)
                StopCoroutine(animationCoroutine);
            animationCoroutine = StartCoroutine(PlayAnimation(forward: true));
        }
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
        }
    }

    private IEnumerator SwitchUpgradeWithReverse(UpgradeType newType)
    {
        isAnimating = true;

        // Play reverse animation (closing)
        yield return PlayAnimation(forward: false);

        // Update UI content after reverse animation finishes
        UpdateUIContent(newType);

        // Play forward animation (opening)
        yield return PlayAnimation(forward: true);

        isAnimating = false;
    }

    private IEnumerator PlayAnimation(bool forward)
    {
        if (animationFrames == null || animationFrames.Length == 0 || animationImage == null)
            yield break;

        int start = forward ? 0 : animationFrames.Length - 1;
        int end = forward ? animationFrames.Length : -1;
        int step = forward ? 1 : -1;

        for (int i = start; i != end; i += step)
        {
            animationImage.sprite = animationFrames[i];
            yield return new WaitForSeconds(frameRate);
        }
    }

    private void UpdateUIContent(UpgradeType type)
    {
        currentType = type;
        var upgradeEntry = UpgradeManager.Instance.playerStatsConfig.GetUpgrade(type);
        if (upgradeEntry == null) return;

        int currentLevel = UpgradeManager.Instance.GetCurrentLevel(type);
        int maxLevel = UpgradeManager.Instance.GetMaxLevel(type);
        float currentValue = upgradeEntry.baseValue + (currentLevel - 1) * upgradeEntry.incrementPerLevel;
        float nextValue = currentLevel < maxLevel ? currentValue + upgradeEntry.incrementPerLevel : currentValue;

        titleText.text = type.ToString();
        descriptionText.text = $"Increase {type} by {upgradeEntry.incrementPerLevel} per level.";
        statText.text = $"<color=white>{currentValue:F1}</color> <color=yellow>{nextValue:F1}</color>";
        iconImage.sprite = upgradeEntry.icon;
    }
}
