using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : GameSingleton<UIManager>
{
    [SerializeField] private Image timerBar;
    [SerializeField] private Image screenEdgeWarning;

    void Start()
    {
        TimerManager.Instance.OnTimerChanged += UpdateUI;
        
    }

    void UpdateUI(float percent)
    {
        timerBar.fillAmount = percent;
        screenEdgeWarning.color = new Color(1, 0, 0, Mathf.Lerp(0f, 1f, 1 - percent));

        //if (percent < 0.2f)
        //    AudioManager.Instance.PlayHeartbeatFast();
        //else
        //    AudioManager.Instance.PlayHeartbeatNormal();
    }
}
