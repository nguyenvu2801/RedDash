using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Reward : InteractableBase
{

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            CurrencyManager.Instance.AddCurrency(10);
            RoomManager.Instance.AdvanceToNextRoom();
            PoolManager.Instance.ReturnToPool(PoolKey.reward, gameObject);
           
        }
    }
}
