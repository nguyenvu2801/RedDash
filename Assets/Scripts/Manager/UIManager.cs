using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : GameSingleton<UIManager>
{
    [SerializeField] private Image timerBar;
    [SerializeField] private Image screenEdgeWarning;
    private Tween scaleTween;
    void Start()
    {
        TimerManager.Instance.OnTimerChanged += UpdateUI;
        
    }

    void UpdateUI(float percent)
    {
        timerBar.fillAmount = percent;
        screenEdgeWarning.color = new Color(1, 0, 0, Mathf.Lerp(0f, 1f, 1 - percent));

        // Scale effect: from 2x when timer starts to 1x when timer ends
        scaleTween?.Kill();

        // Calculate target scale (2 -> 1 as timer decreases)
        float targetScale = Mathf.Lerp(2f, 1f, 1 - percent);

        // Tween the scale smoothly
        scaleTween = screenEdgeWarning.rectTransform.DOScale(targetScale, 0.25f)
            .SetEase(Ease.OutQuad);
        //if (percent < 0.2f)
        //    AudioManager.Instance.PlayHeartbeatFast();
        //else
        //    AudioManager.Instance.PlayHeartbeatNormal();
    }
}
