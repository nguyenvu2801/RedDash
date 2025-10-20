using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D))]
public class Movement : MonoBehaviour
{
    [Header("Base Dash settings")]
    [SerializeField] private float baseDashMaxDistance = 6f;
    [SerializeField] private float dashDuration = 0.18f;
    [SerializeField] private float baseDashCooldown = 0.5f;
    [SerializeField] private float baseRecoveryAfterMiss = 1.8f;
    [SerializeField] private float baseDashHitRadius = 1.0f;
    [SerializeField] private float minDragDistance = 0.1f;
    [SerializeField] private LayerMask dashHitMask;
    [SerializeField] private AnimationCurve dashCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private float effectiveDashMaxDistance;
    private float effectiveDashCooldown;
    private float effectiveRecoveryAfterMiss;
    private float effectiveDashHitRadius;

    Rigidbody2D rb;
    Vector2 storedVelocityBeforeDash = Vector2.zero;
    bool isDragging = false;
    Vector2 startDragPos;
    bool dashing = false;
    float cooldownTimer = 0f;

    public Action OnDashStart;
    public Action<bool> OnDashEnd;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (dashCurve == null) dashCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        ApplyUpgrades();
    }

    private void ApplyUpgrades()
    {
        if (UpgradeManager.Instance != null)
        {
            effectiveDashMaxDistance = baseDashMaxDistance + UpgradeManager.Instance.GetStatModifier(StatType.DashRange);
            effectiveDashCooldown = baseDashCooldown * UpgradeManager.Instance.GetStatModifier(StatType.DashCooldown);
            effectiveRecoveryAfterMiss = baseRecoveryAfterMiss * UpgradeManager.Instance.GetStatModifier(StatType.DashRecovery);
            effectiveDashHitRadius = baseDashHitRadius + UpgradeManager.Instance.GetStatModifier(StatType.DashImpactRadius);
        }
        else
        {
            effectiveDashMaxDistance = baseDashMaxDistance;
            effectiveDashCooldown = baseDashCooldown;
            effectiveRecoveryAfterMiss = baseRecoveryAfterMiss;
            effectiveDashHitRadius = baseDashHitRadius;
        }
    }

    void Update()
    {
        cooldownTimer -= Time.deltaTime;

        if (!dashing && cooldownTimer <= 0f)
        {
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    isDragging = true;
                    startDragPos = Camera.main.ScreenToWorldPoint(touch.position);
                }
                else if (isDragging && (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled))
                {
                    Vector2 endPos = Camera.main.ScreenToWorldPoint(touch.position);
                    TryStartDashFromDrag(startDragPos, endPos);
                    isDragging = false;
                }
            }
            else
            {
                if (Input.GetMouseButtonDown(0))
                {
                    isDragging = true;
                    startDragPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                }
                else if (isDragging && Input.GetMouseButtonUp(0))
                {
                    Vector2 endPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    TryStartDashFromDrag(startDragPos, endPos);
                    isDragging = false;
                }
            }
        }
    }

    void TryStartDashFromDrag(Vector2 start, Vector2 end)
    {
        Vector2 raw = end - start;
        if (raw.sqrMagnitude < minDragDistance * minDragDistance)
        {
            raw = Vector2.up;
        }

        Vector2 dir = raw.normalized;
        float dragLen = Mathf.Clamp(raw.magnitude, 0f, effectiveDashMaxDistance);
        float distance = Mathf.Lerp(effectiveDashMaxDistance * 0.4f, effectiveDashMaxDistance, dragLen / effectiveDashMaxDistance);

        StartCoroutine(DashRoutine(dir, distance));
    }

    IEnumerator DashRoutine(Vector2 direction, float distance)
    {
        dashing = true;
        OnDashStart?.Invoke();

        storedVelocityBeforeDash = rb.velocity;
        rb.velocity = Vector2.zero;

        Vector2 startPos = rb.position;
        Vector2 targetPos = startPos + direction * distance;

        float elapsed = 0f;
        bool hit = false;

        int pierceCount = (int)UpgradeManager.Instance.GetStatModifier(StatType.PierceCount);
        List<Collider2D> hitColliders = new List<Collider2D>();

        while (elapsed < dashDuration)
        {
            float t = elapsed / dashDuration;
            float eased = dashCurve.Evaluate(t);
            Vector2 desiredPos = Vector2.Lerp(startPos, targetPos, eased);

            Vector2 moveDir = desiredPos - rb.position;
            float moveDist = moveDir.magnitude;

            if (moveDist > 0.0001f)
            {
                RaycastHit2D hitInfo = Physics2D.CircleCast(rb.position, effectiveDashHitRadius, moveDir.normalized, moveDist + 0.01f, dashHitMask);
                if (hitInfo.collider != null && !hitColliders.Contains(hitInfo.collider))
                {
                    hit = true;
                    hitColliders.Add(hitInfo.collider);
                    // Damage logic here if needed, + UpgradeManager.Instance.GetStatModifier(StatType.DashDamage)

                    if (pierceCount > 0)
                    {
                        pierceCount--;
                        rb.MovePosition(hitInfo.point + moveDir.normalized * 0.01f);
                    }
                    else
                    {
                        rb.MovePosition(hitInfo.point - moveDir.normalized * 0.01f);
                        break;
                    }
                }
                else
                {
                    rb.MovePosition(desiredPos);
                }
            }
            else
            {
                rb.MovePosition(desiredPos);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (pierceCount >= 0 || !hit)
        {
            rb.MovePosition(targetPos);
        }

        if (EnemyManager.Instance != null)
        {
            EnemyManager.Instance.NotifyDashHit(transform.position, direction, distance / dashDuration, effectiveDashHitRadius);
        }
        if (TimerManager.Instance != null)
        {
            if (!hit) TimerManager.Instance.ReduceTime(1f);
        }

        OnDashEnd?.Invoke(hit);

        cooldownTimer = effectiveDashCooldown * (hit ? 1f : effectiveRecoveryAfterMiss);

        float blendTime = 0.08f;
        float b = 0f;
        while (b < blendTime)
        {
            rb.velocity = Vector2.Lerp(Vector2.zero, storedVelocityBeforeDash, b / blendTime);
            b += Time.deltaTime;
            yield return null;
        }
        rb.velocity = storedVelocityBeforeDash;

        dashing = false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, effectiveDashHitRadius);
    }
}