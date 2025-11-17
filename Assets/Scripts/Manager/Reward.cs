using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Reward : InteractableBase
{

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // TODO: Grant actual reward here if needed (e.g., upgrade, points)
            // For now, just advance
            CurrencyManager.Instance.AddCurrency(10);
            RoomManager.Instance.AdvanceToNextRoom();
            Destroy(gameObject);
        }
    }
}
