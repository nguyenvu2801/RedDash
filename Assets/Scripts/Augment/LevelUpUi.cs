using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class LevelUpUi : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private CanvasGroup panelCanvasGroup;
    [SerializeField] private RectTransform panelRoot;

    [Header("Card Slots (assign 3 in Inspector)")]
    [SerializeField] private List<AugmentCardUI> cards; // exactly 3

    private void Start()
    {
        ExperienceManager.Instance.OnLevelUp += ShowLevelUpChoices;
        panelCanvasGroup.alpha = 0f;
        panelRoot.gameObject.SetActive(false);
    }

    private void ShowLevelUpChoices()
    {
        var choices = AugmentManager.Instance.GetRandomChoices(3);
        if (choices.Count == 0) return;

        // Pause game
        Time.timeScale = 0f;

        panelRoot.gameObject.SetActive(true);

        for (int i = 0; i < cards.Count; i++)
        {
            if (i < choices.Count)
            {
                cards[i].gameObject.SetActive(true);
                cards[i].Setup(choices[i], OnCardChosen);
            }
            else
            {
                cards[i].gameObject.SetActive(false);
            }
        }

        // Fade + scale in (unscaled so it works while paused)
        panelRoot.localScale = Vector3.one * 0.8f;
        panelCanvasGroup.alpha = 0f;
        panelCanvasGroup.DOFade(1f, 0.3f).SetUpdate(true);
        panelRoot.DOScale(1f, 0.3f).SetEase(Ease.OutBack).SetUpdate(true);
    }

    private void OnCardChosen(AugmentType chosen)
    {
        AugmentManager.Instance.ApplyAugment(chosen);
        HidePanel();
    }

    private void HidePanel()
    {
        panelCanvasGroup.DOFade(0f, 0.2f).SetUpdate(true).OnComplete(() =>
        {
            panelRoot.gameObject.SetActive(false);
            Time.timeScale = 1f; // resume
        });
    }
}