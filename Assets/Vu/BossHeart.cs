using System.Collections;
using UnityEngine;

public class BossHeart : MonoBehaviour
{
    [SerializeField] private int maxHeartHP = 280;
    private BossCore bossCore;
    private int currentHP;
    private EnemyHealthBar heartHealthBar;
    private bool isInitialized = false;

    public void Initialize(BossCore boss)
    {
        Debug.Log("aaaa");
        bossCore = boss;
        currentHP = maxHeartHP;
        isInitialized = true;

        // Always fetch fresh from pool - never reuse stale reference
        GameObject hbObj = PoolManager.Instance.GetFromPool(PoolKey.enemyHealthBar);
        Debug.Log("Health bar obj: " + (hbObj == null ? "NULL" : hbObj.name));
        Debug.Log("Health bar active: " + (hbObj != null ? hbObj.activeSelf.ToString() : "N/A"));
        if (hbObj != null)
        {
            heartHealthBar = hbObj.GetComponent<EnemyHealthBar>();
            heartHealthBar.Init(transform);
            heartHealthBar.gameObject.SetActive(true);
            heartHealthBar.SetHP(currentHP, maxHeartHP);
            heartHealthBar.ForceShow();
        }
    }

    public void TakeDamage(int damage)
    {
        if (!isInitialized) return;

        currentHP -= damage;
        heartHealthBar?.SetHP(currentHP, maxHeartHP);
        DamagePopUpManager.Instance.ShowDamage(damage, transform.position);

        if (currentHP <= 0)
            Die();
        else
            StartCoroutine(HitFlash());
    }

    private IEnumerator HitFlash()
    {
        if (TryGetComponent<SpriteRenderer>(out var sr))
        {
            Color original = sr.color;
            sr.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            sr.color = original;
        }
    }

    private void Die()
    {
        isInitialized = false;
        ReturnHealthBar();
        bossCore?.OnHeartDestroyed(this);
        gameObject.SetActive(false);
    }

    private void ReturnHealthBar()
    {
        if (heartHealthBar != null)
        {
            PoolManager.Instance.ReturnToPool(PoolKey.enemyHealthBar, heartHealthBar.gameObject);
            heartHealthBar = null;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isInitialized) return;
        if (other.CompareTag("Player"))
        {
            TakeDamage(40);
        }
    }

    private void OnDisable()
    {
        isInitialized = false;
        ReturnHealthBar(); 
    }
}