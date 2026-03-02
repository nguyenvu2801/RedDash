using DG.Tweening;
using UnityEngine;

public class CurrencyMagnet : MonoBehaviour
{
    [Header("DOTween Magnet Settings")]
    public float moveDuration = 0.5f;
    public float pickupDistance = 0.5f;

    private Transform player;
    private bool isAttracted = false;
    private Tween moveTween;

    // Base range from upgrade + flat bonus from augment
    private float attractRange;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        RefreshAttractRange();
    }

    /// <summary>Recomputes attract range: upgrade base + augment flat bonus.</summary>
    public void RefreshAttractRange()
    {
        float upgradeRange = UpgradeManager.Instance != null
            ? UpgradeManager.Instance.ComputeStat(UpgradeType.Magnet)
            : 0f;

        float augmentBonus = AugmentManager.Instance != null
            ? AugmentManager.Instance.GetTotalBonus(AugmentType.Magnet)
            : 0f;

        attractRange = upgradeRange + augmentBonus;
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (!isAttracted && distance <= attractRange)
            StartAttraction();

        if (isAttracted && distance <= pickupDistance)
            Collect();
    }

    void StartAttraction()
    {
        isAttracted = true;
        moveTween?.Kill();

        moveTween = transform.DOMove(player.position, moveDuration)
            .SetEase(Ease.InQuad)
            .SetUpdate(true)
            .OnUpdate(() =>
            {
                if (Vector2.Distance(transform.position, player.position) <= pickupDistance)
                    Collect();
            });
    }

    void Collect()
    {
        moveTween?.Kill();
        CurrencyManager.Instance.AddCurrency(1);
        // Return to pool or destroy here
    }
}