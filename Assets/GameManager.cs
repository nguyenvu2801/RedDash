using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : GameSingleton<GameManager>
{
    public bool IsGameOver { get; private set; }
    void Start()
    {
        TimerManager.Instance.OnTimerDepleted += GameOver;
    }

    void GameOver()
    {
        IsGameOver = true;
    }
}
