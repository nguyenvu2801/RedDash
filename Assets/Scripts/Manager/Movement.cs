using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Cinemachine;
using DG.Tweening;

[RequireComponent(typeof(Rigidbody2D))]
public class Movement : MonoBehaviour
{
    public static Movement player;
    [Header("Dash settings")]
    [SerializeField] private float dashDuration = 0.18f;
    [SerializeField] private float minDragDistance = 0.1f;
    [SerializeField] private float dashHitRadius = 1.0f;
    [SerializeField] private LayerMask dashHitMask; // which layers count as hit (Enemy, EnergyNode)
    [SerializeField] private LayerMask wallMask;
    [SerializeField] private AnimationCurve dashCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private Transform visualTransform;
    [Header("Camera Dash FX")]
    [SerializeField] private float dashCamZoomAmount = 0.15f;
    [SerializeField] private float dashCamOffsetAmount = 0.25f;
    [SerializeField] private float dashCamDuration = 0.15f;
    private float DashRange;
    private float DashCooldown;
    private float DashPenalty; //remember to compute these 2 stats
    private float DashPower = 1;
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
    private bool dashHitSomething = false;
    private Vector2 currentDashDirection;
    [SerializeField] private CharacterAnimator animator;
    [SerializeField] private SpriteRenderer sr;
    private bool recoveringRotation = false;
    private bool isDead = false;  // Prevent multiple deaths
    private CinemachineVirtualCamera vcam;
    private CinemachineFramingTransposer framing;
    private Coroutine hurtCoroutine = null;

    private void OnEnable() => SceneManager.sceneLoaded += TrySubscribe;
    private void OnDisable() => SceneManager.sceneLoaded -= TrySubscribe;

    private void TrySubscribe(Scene _, LoadSceneMode __)
    {
        if (TimerManager.Instance is { } tm)
        {
            tm.OnPlayerDamaged -= HandleDamage; tm.OnPlayerDamaged += HandleDamage;
            tm.OnTimerDepleted -= HandleDeath; tm.OnTimerDepleted += HandleDeath;
        }
    }

    private void TryUnsubscribeFromTimer()
    {
        if (TimerManager.HasInstance && TimerManager.Instance != null)
        {
            TimerManager.Instance.OnPlayerDamaged -= HandleDamage;
            TimerManager.Instance.OnTimerDepleted -= HandleDeath;
        }
    }
    void Awake()
    {
        TrySubscribe(default, default);
        player = this;
        rb = GetComponent<Rigidbody2D>();
        vcam = FindObjectOfType<CinemachineVirtualCamera>();
        if (vcam)
            framing = vcam.GetCinemachineComponent<CinemachineFramingTransposer>();

        if (dashCurve == null)
            dashCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        UpgradeManager.OnUpgradeSuccessful += HandleUpgrade;
        // Compute all stats
        ComputeDashStats();
    }

    void Update()
    {
        if (isDead) return;
        // timers
        cooldownTimer -= Time.deltaTime;
        if (!dashing && !recoveringRotation)
        {
            Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            sr.flipX = (mouseWorldPos.x < transform.position.x);
        }
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
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (isDead) return;
                Vector2 playerPos = rb.position;
                Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

                Vector2 dashDirection = (mouseWorldPos - playerPos).normalized;
                float distance = DashRange;

                StartCoroutine(DashRoutine(dashDirection, distance));
            }
        }
        Debug.Log(isDead);
    }

    void TryStartDashFromDrag(Vector2 start, Vector2 end)
    {
        Vector2 raw = end - start;
        if (raw.sqrMagnitude < minDragDistance * minDragDistance)
            raw = Vector2.up;

        Vector2 dir = raw.normalized;
        float dragLen = Mathf.Clamp(raw.magnitude, 0f, DashRange);
        float distance = Mathf.Lerp(DashRange * 0.4f, DashRange, dragLen / DashRange);

        StartCoroutine(DashRoutine(dir, distance));
    }


    IEnumerator DashRoutine(Vector2 direction, float distance)
    {
        dashing = true;
        dashHitSomething = false;
        currentDashDirection = direction;
        storedVelocityBeforeDash = rb.velocity;
        rb.velocity = Vector2.zero;

        // Play dash animation
        animator?.PlayAnimation("Attack");

        // Face correct direction
        sr.flipX = false;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        visualTransform.rotation = Quaternion.Euler(0f, 0f, angle);

        // --- Camera Dash FX ---
        StartCoroutine(DashCameraEffect(direction));

        // Wall check
        Vector2 startPos = rb.position;
        RaycastHit2D hit = Physics2D.Raycast(startPos, direction, distance, wallMask);
        Vector2 targetPos = hit.collider != null ? hit.point - direction * 0.1f : startPos + direction * distance;

        float elapsed = 0f;
        while (elapsed < dashDuration)
        {
            float t = elapsed / dashDuration;
            float eased = dashCurve.Evaluate(t);
            rb.position = Vector2.Lerp(startPos, targetPos, eased);
            elapsed += Time.deltaTime;
            yield return null;
        }
        rb.position = targetPos;

        // Hit logic
        if (dashHitSomething && GameManager.Instance)
            StartCoroutine(GameManager.Instance.ShakeCamera(0.1f, 2f));

        // Cooldown
        cooldownTimer = DashCooldown;
        if (!isDead && hurtCoroutine == null)
            animator?.PlayAnimation("Idle");

        // Reset rotation
        Quaternion startRotation = visualTransform.rotation;
        float blendTime = 0.08f;
        float timer = 0f;
        while (timer < blendTime)
        {
            float t = timer / blendTime;
            visualTransform.rotation = Quaternion.RotateTowards(startRotation, Quaternion.identity, 720f * t);
            rb.velocity = Vector2.Lerp(Vector2.zero, storedVelocityBeforeDash, t);
            timer += Time.deltaTime;
            yield return null;
        }
        visualTransform.rotation = Quaternion.identity;
        rb.velocity = storedVelocityBeforeDash;

        dashing = false;
    }
    private IEnumerator DashCameraEffect(Vector2 dashDirection)
    {
        if (framing == null || vcam == null) yield break;

        Vector3 originalOffset = framing.m_TrackedObjectOffset;
        Vector3 dashOffset = new Vector3(dashDirection.x, dashDirection.y, 0) * dashCamOffsetAmount * vcam.m_Lens.OrthographicSize;

        // Smooth offset
        DOTween.To(() => framing.m_TrackedObjectOffset,
                   x => framing.m_TrackedObjectOffset = x,
                   originalOffset + dashOffset,
                   dashCamDuration / 2f)
               .SetEase(Ease.OutQuad)
               .OnComplete(() =>
               {
                   DOTween.To(() => framing.m_TrackedObjectOffset,
                              x => framing.m_TrackedObjectOffset = x,
                              originalOffset,
                              dashCamDuration / 2f)
                         .SetEase(Ease.InQuad);
               });

        // Zoom effect
       float zoomAmount = 0.1f;
        float originalSize = vcam.m_Lens.OrthographicSize;
        DOTween.To(() => vcam.m_Lens.OrthographicSize,
                   x => vcam.m_Lens.OrthographicSize = x,
                   originalSize - zoomAmount,
                   dashCamDuration / 2f)
               .SetEase(Ease.OutQuad)
               .OnComplete(() =>
               {
                   DOTween.To(() => vcam.m_Lens.OrthographicSize,
                              x => vcam.m_Lens.OrthographicSize = x,
                              originalSize,
                              dashCamDuration / 2f)
                         .SetEase(Ease.InQuad);
               });

        yield return new WaitForSeconds(dashCamDuration);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!dashing) return;
        if (((1 << other.gameObject.layer) & dashHitMask) != 0)
        {
            dashHitSomething = true;
            EnemyBase enemy = other.GetComponent<EnemyBase>();
            if (enemy != null)
            {
                enemy.OnDashHit(currentDashDirection, DashPower);
            }
        }
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
    private void HandleDamage(float amount)
    {
        if (isDead) return;

        // Stop any previous hurt coroutine
        if (hurtCoroutine != null)
            StopCoroutine(hurtCoroutine);

        // Play hurt animation
        animator?.PlayAnimation("Hurt");

        // Force return to Idle after fixed time (even if dashing!)
        hurtCoroutine = StartCoroutine(ReturnToIdleAfterHurt());
    }

    private IEnumerator ReturnToIdleAfterHurt()
    {
        yield return new WaitForSeconds(0.25f); // Slightly longer than before

        // Only go back to Idle if we're not dashing and not dead
        if (!isDead && !dashing)
        {
            animator?.PlayAnimation("Idle");
        }
        hurtCoroutine = null;
    }
    private void HandleDeath()
    {

        if (isDead) return;
        isDead = true;

        // Stop all movement
        rb.velocity = Vector2.zero;
        dashing = false;

        // Play death animation and STAY on it
        if (animator != null)
        {
            animator.PlayAnimation("Die");
        }


        StartCoroutine(DeathSequenceBeforeGameOver());
    }
    private IEnumerator DeathSequenceBeforeGameOver()
    {
        // Small hit-stop effect
        Time.timeScale = 0.1f;
        yield return new WaitForSecondsRealtime(0.06f);

        Time.timeScale = 1f;

        // Allow animation to play a bit
        yield return new WaitForSeconds(0.4f);

        // Extra delay for dramatic effect
        yield return new WaitForSeconds(0.4f);

        GameManager.Instance.TriggerGameOver();
    }
    void OnDrawGizmosSelected()
    {
        // visualize dash hit radius at player position
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, dashHitRadius);
    }
}