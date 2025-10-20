using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using TMPro; 
public class UIManager : GameSingleton<UIManager>
{
    [SerializeField] private Image timerBar;
    [SerializeField] private Image screenEdgeWarning;

    [Header("Combo UI")]
    [SerializeField] private Image comboMeterFill;    // circular or horizontal fill image
    [SerializeField] private RectTransform comboPopupRoot; // scale/animate this when combo grows
    [SerializeField] private TextMeshProUGUI comboText; // use TextMeshProUGUI if desired
    [SerializeField] private TextMeshProUGUI currencyText;
    [SerializeField] private float comboPopupScale = 1.6f;
    [SerializeField] private float comboPopupTime = 0.25f;

    private Tween scaleTween;
    private Tween popupTween;

    void Start()
    {
        TimerManager.Instance.OnTimerChanged += UpdateUI;
        // subscribe to combo events
        if (ComboManager.Instance != null)
        {
            ComboManager.Instance.OnComboChanged += UpdateComboUI;
            ComboManager.Instance.OnComboReset += ResetComboUI;
        }
    }

    void UpdateUI(float percent)
    {
        timerBar.fillAmount = percent;
        screenEdgeWarning.color = new Color(1, 0, 0, Mathf.Lerp(0f, 1f, 1 - percent));
        if (CurrencyManager.Instance != null)
        {
            currencyText.text = "Essence: " + CurrencyManager.Instance.GetCurrency().ToString();
        }
        // Scale effect: from 2x when timer starts to 1x when timer ends
        scaleTween?.Kill();

        // Calculate target scale (2 -> 1 as timer decreases)
        float targetScale = Mathf.Lerp(2f, 1f, 1 - percent);

        // Tween the scale smoothly
        scaleTween = screenEdgeWarning.rectTransform.DOScale(targetScale, 0.25f)
            .SetEase(Ease.OutQuad);
    }

    private void UpdateComboUI(int combo, float percentTimeLeft)
    {
        if (combo <= 0)
        {
            ResetComboUI();
            return;
        }

        if (comboText != null)
        {
            comboText.text = "x" + combo;
            // pop effect
            popupTween?.Kill();
            comboPopupRoot.localScale = Vector3.one;
            popupTween = comboPopupRoot.DOScale(comboPopupScale, comboPopupTime).SetEase(Ease.OutBack).OnComplete(() =>
            {
                comboPopupRoot.DOScale(1f, 0.15f).SetEase(Ease.OutQuad);
            });
        }

        if (comboMeterFill != null)
            comboMeterFill.fillAmount = Mathf.Clamp01(percentTimeLeft);
    }

    private void ResetComboUI()
    {
        if (comboText != null) comboText.text = "";
        if (comboMeterFill != null) comboMeterFill.fillAmount = 0f;
        popupTween?.Kill();
        if (comboPopupRoot != null) comboPopupRoot.localScale = Vector3.one;
    }
}
