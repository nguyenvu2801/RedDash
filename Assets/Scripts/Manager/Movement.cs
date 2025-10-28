using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D))]
public class Movement : MonoBehaviour
{
    [Header("Dash settings")]
    [SerializeField] private float dashDuration = 0.18f;
    [SerializeField] private float minDragDistance = 0.1f;
    [SerializeField] private float dashHitRadius = 1.0f;
    [SerializeField] private LayerMask dashHitMask;              // which layers count as hit (Enemy, EnergyNode)
    [SerializeField] private AnimationCurve dashCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private float DashRange;
    private float DashCooldown;
    private float DashPenalty;
    // movement / physics
    Rigidbody2D rb;
    Vector2 storedVelocityBeforeDash = Vector2.zero;

    // input / drag
    bool isDragging = false;
    Vector2 startDragPos;

    // state
    bool dashing = false;
    float cooldownTimer = 0f;

    // Events
    public Action OnDashStart;
    public Action<bool> OnDashEnd; // bool = hit or miss

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (dashCurve == null)
            dashCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        UpgradeManager.OnUpgradeSuccessful += HandleUpgrade;
        // Compute all stats
        ComputeDashStats();

    }
    void OnDestroy()
    {
        UpgradeManager.OnUpgradeSuccessful -= HandleUpgrade;
    }
    void Update()
    {
        // timers
        cooldownTimer -= Time.deltaTime;

        // input allowed only if not currently dashing and off cooldown
        if (!dashing && cooldownTimer <= 0f)
        {
            // Touch input
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
            else // Mouse input (testing)
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
            // treat as short tap: default to current facing/up
            raw = Vector2.up;
        }

        Vector2 dir = raw.normalized;

        // scale distance by drag length but clamp to max
        float dragLen = Mathf.Clamp(raw.magnitude, 0f, DashRange);
        float distance = Mathf.Lerp(DashRange * 0.4f, DashRange, dragLen / DashRange);

        StartCoroutine(DashRoutine(dir, distance));
    }

    IEnumerator DashRoutine(Vector2 direction, float distance)
    {
        dashing = true;
        OnDashStart?.Invoke();

        // store velocity, zero it to make dash deterministic then blend back
        storedVelocityBeforeDash = rb.velocity;
        rb.velocity = Vector2.zero;

        Vector2 startPos = rb.position;
        Vector2 targetPos = startPos + direction * distance;

        float elapsed = 0f;
        bool hit = false;

        // move using curve in small time steps, checking for hits each frame
        while (elapsed < dashDuration)
        {
            float t = elapsed / dashDuration;
            float eased = dashCurve.Evaluate(t);
            Vector2 desiredPos = Vector2.Lerp(startPos, targetPos, eased);

            // check for collision along the short segment from current position to desiredPos
            Vector2 moveDir = desiredPos - rb.position;
            float moveDist = moveDir.magnitude;

            if (moveDist > 0.0001f)
            {
                RaycastHit2D hitInfo = Physics2D.CircleCast(rb.position, dashHitRadius, moveDir.normalized, moveDist + 0.01f, dashHitMask);
                if (hitInfo.collider != null)
                {
                    // register hit at point of collision
                    hit = true;
                    rb.MovePosition(hitInfo.point - moveDir.normalized * 0.01f); // put slightly before contact
                    break;
                }
            }

            rb.MovePosition(desiredPos);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // if we finished loop without early hit, ensure we reach final target (unless interrupted)
        if (!hit)
        {
            // final check: if there's an overlap at target
            RaycastHit2D finalHit = Physics2D.CircleCast(rb.position, dashHitRadius, (targetPos - rb.position).normalized, Vector2.Distance(rb.position, targetPos) + 0.01f, dashHitMask);
            if (finalHit.collider != null)
            {
                hit = true;
                rb.MovePosition(finalHit.point - (targetPos - rb.position).normalized * 0.01f);
            }
            else
            {
                rb.MovePosition(targetPos);
            }
        }

        // Report to managers (keep your original behavior)
        if (EnemyManager.Instance != null)
        {
            EnemyManager.Instance.NotifyDashHit(transform.position, direction, distance / dashDuration, dashHitRadius);
        }
        if (TimerManager.Instance != null)
        {
            if (hit) TimerManager.Instance.AddTime(1f);
            else TimerManager.Instance.ReduceTime(1f);
        }

        // event
        OnDashEnd?.Invoke(hit);

        // apply cooldown (penalty on miss)
        cooldownTimer = DashCooldown;

        // gently blend back to stored velocity so player doesn't snap
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
    private void HandleUpgrade(UpgradeType type)
    {
        if (IsDashRelevant(type))
        {
            ComputeDashStats();
        }
    }
    private bool IsDashRelevant(UpgradeType type)
    {
        return type == UpgradeType.DashRange ||
               type == UpgradeType.DashCooldown ||
               type == UpgradeType.DashPenalty;
    }
    public void ComputeDashStats()
    {
        DashRange = UpgradeManager.Instance.ComputeStat(UpgradeType.DashRange);
        DashCooldown = UpgradeManager.Instance.ComputeStat(UpgradeType.DashCooldown);
        DashPenalty = UpgradeManager.Instance.ComputeStat(UpgradeType.DashPenalty);
    }
    void OnDrawGizmosSelected()
    {
        // visualize dash hit radius at player position
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, dashHitRadius);
    }
}
