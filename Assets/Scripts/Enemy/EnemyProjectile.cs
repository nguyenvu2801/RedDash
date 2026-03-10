using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    private Rigidbody2D rb;

    [SerializeField] private float lifetime = 5f;         
    [SerializeField] private float timePenalty = 8f;     

    private bool hasHit;  

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("EnemyProjectile missing Rigidbody2D!", this);
        }
    }

    public void Launch(Vector2 direction, float speed, int damageAmount)
    {
        
        hasHit = false;

        if (rb != null)
        {
            rb.velocity = direction.normalized * speed;
        }
        Destroy(gameObject, lifetime);

      
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
            else
            {
                Debug.LogWarning("TimerManager.Instance is null - cannot reduce time!");
            }
            if (PoolManager.Instance != null)
            {
                PoolManager.Instance.ReturnToPool(PoolKey.enemyProjectile, gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        // Optional: also hit walls / obstacles?
        // else if (other.CompareTag("Wall") || other.gameObject.layer == LayerMask.NameToLayer("Environment"))
        // {
        //     hasHit = true;
        //     ReturnToPoolOrDestroy();
        // }
    }

    // Safety cleanup
    private void OnDisable()
    {
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
        hasHit = false;
    }
}