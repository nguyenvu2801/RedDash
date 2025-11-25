using UnityEngine;

public class DamagePopUpManager : GameSingleton<DamagePopUpManager>
{
    public void ShowDamage(int amount, Vector3 worldPos)
    {
        GameObject obj = PoolManager.Instance.GetFromPool(PoolKey.damagePopup);

        obj.transform.position = worldPos;

        FloatingDamager d = obj.GetComponent<FloatingDamager>();
        d.Show(amount);
    }
}
