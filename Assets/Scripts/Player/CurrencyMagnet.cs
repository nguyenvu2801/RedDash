using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurrencyMagnet : MonoBehaviour
{
    [Header("DOTween Magnet Settings")]
    public float moveDuration = 0.5f;       // How long it takes to reach player
    public float pickupDistance = 0.5f;     // Distance to collect

    private Transform player;
    private bool isAttracted = false;
    private Tween moveTween;
    private float attractRange;
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        attractRange = UpgradeManager.Instance.ComputeStat(UpgradeType.Magnet);
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (!isAttracted && distance <= attractRange)
        {
            StartAttraction();
        }

        // Optional: extra safety check to auto collect
        if (isAttracted && distance <= pickupDistance)
        {
            Collect();
        }
    }

    void StartAttraction()
    {
        isAttracted = true;

        // Cancel any previous tween if exists
        moveTween?.Kill();

        // Smoothly move to player using DOTween
        moveTween = transform.DOMove(player.position, moveDuration)
            .SetEase(Ease.InQuad) // Smooth acceleration
            .SetUpdate(true)      // Keeps moving even if game is paused (optional)
            .OnUpdate(() =>
            {
                if (Vector2.Distance(transform.position, player.position) <= pickupDistance)
                {
                    Collect();
                }
            });
    }

    void Collect()
    {
        moveTween?.Kill();

        // Add to your currency system
        CurrencyManager.Instance.AddCurrency(1);

        // Optional: play sound or VFX
        //Destroy(gameObject);
    }

}

