using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamagePopUpManager : GameSingleton<DamagePopUpManager>
{
    public FloatingDamager popupPrefab;

    public void ShowDamage(int amount, Vector3 worldPos)
    {
        var popup = Instantiate(popupPrefab, worldPos, Quaternion.identity);
        popup.Show(amount);
    }
}

