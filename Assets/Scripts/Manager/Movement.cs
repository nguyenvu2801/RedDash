using UnityEngine;
using System;
using System.Collections;
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
    [SerializeField] private LayerMask dashHitMask;
    [SerializeField] private LayerMask wallMask;
    [SerializeField] private AnimationCurve dashCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Camera Dash FX")]
    [SerializeField] private float dashCamZoomAmount = 0.15f;
    [SerializeField] private float dashCamOffsetAmount = 0.25f;
    [SerializeField] private float dashCamDuration = 0.15f;

    // Base stats from UpgradeManager
    private float DashRange;
    private float DashCooldown;
    private float DashPenalty;
    private float DashPower;

    // Augment bonuses stacked on top of upgrades
    private float augmentDashPowerBonus = 0f;
    private float augmentCooldownReduction = 0f; // fraction, e.g. 0.3 = 30% shorter CD

    Rigidbody2D rb;
    Vector2 storedVelocityBeforeDash = Vector2.zero;

    bool isDragging = false;
    Vector2 startDragPos;
    bool dashing = false;
    float cooldownTimer = 0f;

    public Action OnDashStart;
    public Action<bool> OnDashEnd;
    private bool dashHitSomething = false;
    private Vector2 currentDashDirection;

    [SerializeField] private CharacterAnimator animator;
    [SerializeField] private SpriteRenderer sr;
    private bool isDead = false;

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
        ComputeDashStats();
    }

    void Update()
    {
        if (isDead) return;
        cooldownTimer -= Time.deltaTime;

        if (!dashing)
        {
            Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            sr.flipX = (mouseWorldPos.x < transform.position.x);
        }

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

            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (isDead) return;
                SoundManager.Instance.PlaySFX("Dash");
                Vector2 playerPos = rb.position;
                Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector2 dashDirection = (mouseWorldPos - playerPos).normalized;
                StartCoroutine(DashRoutine(dashDirection, DashRange));
            }
        }
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

        animator?.PlayAnimation("Attack");
        sr.flipX = (direction.x < 0);
        StartCoroutine(DashCameraEffect(direction));

        Vector2 startPos = rb.position;
        RaycastHit2D hit = Physics2D.Raycast(startPos, direction, distance, wallMask);
        Vector2 targetPos = hit.collider != null ? hit.point - direction * 0.1f : startPos + direction * distance;

        float elapsed = 0f;
        while (elapsed < dashDuration)
        {
            rb.position = Vector2.Lerp(startPos, targetPos, dashCurve.Evaluate(elapsed / dashDuration));
            elapsed += Time.deltaTime;
            yield return null;
        }
        rb.position = targetPos;

        if (dashHitSomething && GameManager.Instance)
            StartCoroutine(GameManager.Instance.ShakeCamera(0.1f, 2f));

        cooldownTimer = DashCooldown;

        if (!isDead && hurtCoroutine == null)
            animator?.PlayAnimation("Idle");

        float blendTime = 0.08f;
        float timer = 0f;
        while (timer < blendTime)
        {
            rb.velocity = Vector2.Lerp(Vector2.zero, storedVelocityBeforeDash, timer / blendTime);
            timer += Time.deltaTime;
            yield return null;
        }
        rb.velocity = storedVelocityBeforeDash;
        dashing = false;
    }

    private IEnumerator DashCameraEffect(Vector2 dashDirection)
    {
        if (framing == null || vcam == null) yield break;

        Vector3 originalOffset = framing.m_TrackedObjectOffset;
        Vector3 dashOffset = new Vector3(dashDirection.x, dashDirection.y, 0) * dashCamOffsetAmount * vcam.m_Lens.OrthographicSize;

        DOTween.To(() => framing.m_TrackedObjectOffset, x => framing.m_TrackedObjectOffset = x,
                   originalOffset + dashOffset, dashCamDuration / 2f).SetEase(Ease.OutQuad)
               .OnComplete(() => DOTween.To(() => framing.m_TrackedObjectOffset,
                   x => framing.m_TrackedObjectOffset = x, originalOffset, dashCamDuration / 2f).SetEase(Ease.InQuad));

        float originalSize = vcam.m_Lens.OrthographicSize;
        DOTween.To(() => vcam.m_Lens.OrthographicSize, x => vcam.m_Lens.OrthographicSize = x,
                   originalSize - 0.1f, dashCamDuration / 2f).SetEase(Ease.OutQuad)
               .OnComplete(() => DOTween.To(() => vcam.m_Lens.OrthographicSize,
                   x => vcam.m_Lens.OrthographicSize = x, originalSize, dashCamDuration / 2f).SetEase(Ease.InQuad));

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
                enemy.OnDashHit(currentDashDirection, DashPower);
        }
    }

    // Augment Hooks 

    /// <summary>IncreaseDamage augment: stacks a flat bonus on top of upgrade-based DashPower.</summary>
    public void AddDashPowerBonus(float bonus)
    {
        augmentDashPowerBonus += bonus;
        // Recompute so the new bonus is baked in immediately
        ComputeDashStats();
    }

    /// <summary>ReduceDashCD augment: each level cuts cooldown by baseValue fraction.</summary>
    public void AddDashCooldownReduction(float bonus)
    {
        augmentCooldownReduction = Mathf.Min(augmentCooldownReduction + bonus, 0.9f); // hard cap at 90%
        ComputeDashStats();
    }

    // Upgrade Hooks 

    private void HandleUpgrade(UpgradeType type)
    {
        if (IsDashRelevant(type))
            ComputeDashStats();
    }

    private bool IsDashRelevant(UpgradeType type) =>
        type == UpgradeType.DashRange ||
        type == UpgradeType.DashCooldown ||
        type == UpgradeType.DashPower ||
        type == UpgradeType.DashPenalty;

    public void ComputeDashStats()
    {
        DashRange = UpgradeManager.Instance.ComputeStat(UpgradeType.DashRange);
        DashPenalty = UpgradeManager.Instance.ComputeStat(UpgradeType.DashPenalty);

        // Augment multiplies down the upgrade-computed cooldown
        float baseCooldown = UpgradeManager.Instance.ComputeStat(UpgradeType.DashCooldown);
        DashCooldown = baseCooldown * (1f - augmentCooldownReduction);

        // Augment adds a flat bonus on top of upgrade-computed power
        float basePower = UpgradeManager.Instance.ComputeStat(UpgradeType.DashPower);
        DashPower = basePower + augmentDashPowerBonus;
    }

    // Damage / Death 

    private void HandleDamage(float amount)
    {
        if (isDead) return;
        if (hurtCoroutine != null) StopCoroutine(hurtCoroutine);
        animator?.PlayAnimation("Hurt");
        hurtCoroutine = StartCoroutine(ReturnToIdleAfterHurt());
    }

    private IEnumerator ReturnToIdleAfterHurt()
    {
        yield return new WaitForSeconds(0.25f);
        if (!isDead && !dashing)
            animator?.PlayAnimation("Idle");
        hurtCoroutine = null;
    }

    private void HandleDeath()
    {
        if (isDead) return;
        isDead = true;
        rb.velocity = Vector2.zero;
        dashing = false;
        animator?.PlayAnimation("Die");
        StartCoroutine(DeathSequenceBeforeGameOver());
    }

    private IEnumerator DeathSequenceBeforeGameOver()
    {
        Time.timeScale = 0.1f;
        yield return new WaitForSecondsRealtime(0.06f);
        Time.timeScale = 1f;
        yield return new WaitForSeconds(0.8f);
        GameManager.Instance.TriggerGameOver();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, dashHitRadius);
    }
}