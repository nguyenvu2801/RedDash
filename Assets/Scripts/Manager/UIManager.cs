using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : GameSingleton<UIManager>
{
    public GameObject enemyHealthBarPrefab;
    [SerializeField] private Image timerBar;
    [SerializeField] private Image screenEdgeWarning;
  
    [Header("Combo UI")]
    [SerializeField] private Image comboMeterFill;
    [SerializeField] private RectTransform comboPopupRoot;
    [SerializeField] private TextMeshProUGUI comboText;
    [SerializeField] private TextMeshProUGUI currencyText;
    [SerializeField] private float comboPopupScale = 1.6f;
    [SerializeField] private float comboPopupTime = 0.25f;

    [Header("Experience UI")]
    [SerializeField] private Image experienceFill;
    [SerializeField] private TextMeshProUGUI xpText;
    [SerializeField] private float expFillTweenTime = 0.25f;

    private Tween scaleTween;
    private Tween popupTween;
    private Tween expTween;
    [Header("Generic UI Popup")]
    [SerializeField] private GameObject popupUI;
    [SerializeField] private float popupDuration = 0.3f;

    private Tween popupAnim;
    private bool isVisible = false;
    void Start()
    {
        if (TimerManager.Instance != null)
            TimerManager.Instance.OnTimerChanged += UpdateUI;
        SoundManager.Instance.PlayMusic("Gameplay");
        if (ComboManager.Instance != null)
        {
            ComboManager.Instance.OnComboChanged += HandleComboChanged;
            ComboManager.Instance.OnComboReset += ResetComboUI;
        }

        if (ExperienceManager.Instance != null)
        {
            ExperienceManager.Instance.OnExperienceChanged += UpdateExperienceUI;
            UpdateExperienceUI();
        }
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (isVisible)
                HidePopup();
            else
                ShowPopup();

            isVisible = !isVisible;
        }
    }
    void UpdateUI(float percent)
    {
        timerBar.fillAmount = percent;
        screenEdgeWarning.color = new Color(0.8f, 0.1f, 0.1f, Mathf.Lerp(0f, 0.15f, 1 - percent));

        if (CurrencyManager.Instance != null)
            currencyText.text = "Essence: " + CurrencyManager.Instance.GetCurrency().ToString();

        scaleTween?.Kill();
        float targetScale = Mathf.Lerp(2f, 1f, 1 - percent);
        scaleTween = screenEdgeWarning.rectTransform.DOScale(targetScale, 0.25f).SetEase(Ease.OutQuad);
    }

    public void UpdateExperienceUI()
    {
        if (experienceFill == null || ExperienceManager.Instance == null) return;

        int currentExp = ExperienceManager.Instance.currentExp;
        int currentLevel = ExperienceManager.Instance.currentLevel;
        int expForNextLevel = ExperienceManager.Instance.GetExpForLevel(currentLevel);

        float targetFill = Mathf.Clamp01((float)currentExp / expForNextLevel);

        expTween?.Kill();
        expTween = experienceFill.DOFillAmount(targetFill, expFillTweenTime).SetEase(Ease.OutQuad);

        if (xpText != null)
            xpText.text = $"{currentExp} / {expForNextLevel}";
    }

    // Bridges the single-int event to UpdateComboUI by fetching the timer separately
    private void HandleComboChanged(int combo)
    {
        float timerPercent = ComboManager.Instance != null
            ? ComboManager.Instance.GetComboTimerNormalized()
            : 0f;

        UpdateComboUI(combo, timerPercent);
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
            popupTween?.Kill();
            comboPopupRoot.localScale = Vector3.one;
            popupTween = comboPopupRoot.DOScale(comboPopupScale, comboPopupTime)
                .SetEase(Ease.OutBack)
                .OnComplete(() => comboPopupRoot.DOScale(1f, 0.15f).SetEase(Ease.OutQuad));
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
    public void ShowPopup()
    {
        if (popupUI == null) return;

        popupAnim?.Kill();

        popupUI.SetActive(true);

        RectTransform rect = popupUI.GetComponent<RectTransform>();
        CanvasGroup cg = popupUI.GetComponent<CanvasGroup>();

        if (cg == null) cg = popupUI.AddComponent<CanvasGroup>();

        rect.localScale = Vector3.zero;
        cg.alpha = 0f;

        popupAnim = DOTween.Sequence()
            .Append(rect.DOScale(1.15f, popupDuration).SetEase(Ease.OutBack))
            .Append(rect.DOScale(1f, 0.1f).SetEase(Ease.OutQuad))
            .Join(cg.DOFade(1f, popupDuration));
    }

    public void HidePopup()
    {
        if (popupUI == null) return;

        popupAnim?.Kill();

        RectTransform rect = popupUI.GetComponent<RectTransform>();
        CanvasGroup cg = popupUI.GetComponent<CanvasGroup>();

        if (cg == null) cg = popupUI.AddComponent<CanvasGroup>();

        popupAnim = DOTween.Sequence()
            .Append(rect.DOScale(0.8f, 0.15f).SetEase(Ease.InQuad))
            .Append(rect.DOScale(0f, 0.15f).SetEase(Ease.InBack))
            .Join(cg.DOFade(0f, 0.2f))
            .OnComplete(() => popupUI.SetActive(false));
    }
}