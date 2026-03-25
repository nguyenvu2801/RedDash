using System.Collections;
using UnityEngine;

public class BossHeart : MonoBehaviour
{
    [SerializeField] private int maxHeartHP = 280;

    private BossCore bossCore;
    private int currentHP;

    // We reuse the same health bar system as normal enemies
    private EnemyHealthBar heartHealthBar;

    public void Initialize(BossCore boss)
    {
        bossCore = boss;
        currentHP = maxHeartHP;

        // Create health bar for this heart (exactly like EnemyBase)
        if (heartHealthBar == null)
        {
            GameObject hbObj = PoolManager.Instance.GetFromPool(PoolKey.enemyHealthBar);
            heartHealthBar = hbObj.GetComponent<EnemyHealthBar>();
        }

        heartHealthBar.Init(transform);
        heartHealthBar.gameObject.SetActive(true);
        heartHealthBar.SetHP(currentHP, maxHeartHP);
    }

    public void TakeDamage(int damage)
    {
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
        if (heartHealthBar != null)
        {
            PoolManager.Instance.ReturnToPool(PoolKey.enemyHealthBar, heartHealthBar.gameObject);
            heartHealthBar = null;
        }

        bossCore?.OnHeartDestroyed(this);
        gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            TakeDamage(40);         
            
        }
    }

    private void OnDisable()
    {
        if (heartHealthBar != null)
        {
            PoolManager.Instance.ReturnToPool(PoolKey.enemyHealthBar, heartHealthBar.gameObject);
        }
    }
}