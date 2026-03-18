using UnityEngine;
using System.Collections;

public class EnemyProjectile : MonoBehaviour
{
    private Rigidbody2D rb;

    [SerializeField] private float lifetime = 5f;
    [SerializeField] private float timePenalty = 8f;

    private bool hasHit;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Launch(Vector2 direction, float speed, int damageAmount)
    {
        hasHit = false;

        if (rb != null)
        {
            Vector2 vel = direction.normalized * speed;
            rb.velocity = vel;

            float angle = Mathf.Atan2(vel.y, vel.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 180, -angle);
        }

        StopAllCoroutines();
        StartCoroutine(AutoReturnAfterDelay());
    }

    private IEnumerator AutoReturnAfterDelay()
    {
        yield return new WaitForSeconds(lifetime);

        if (!hasHit)
        {
            ReturnToPool();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit) return;

        if (other.CompareTag("Player"))
        {
            hasHit = true;

            if (TimerManager.Instance != null)
            {
                TimerManager.Instance.ReduceTime(timePenalty);
            }

            ReturnToPool();
        }
    }

    private void ReturnToPool()
    {
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        transform.rotation = Quaternion.identity;

        if (PoolManager.Instance != null)
        {
            PoolManager.Instance.ReturnToPool(PoolKey.enemyProjectile, gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private void OnDisable()
    {
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
        hasHit = false;

        StopAllCoroutines();
    }
}