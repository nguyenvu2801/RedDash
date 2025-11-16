using System.Collections;
using System.Collections.Generic;
using UnityEditor.EditorTools;
using UnityEngine;

public class GameManager : GameSingleton<GameManager>
{
    public bool IsGameOver { get; private set; }
    void Start()
    {
        TimerManager.Instance.OnTimerDepleted += GameOver;
        RoomManager.Instance.StartRoom();
    }

    void GameOver()
    {
        IsGameOver = true;
    }
}
