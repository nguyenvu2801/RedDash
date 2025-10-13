using UnityEngine;
using System.Collections;
using System;

public class Movement : MonoBehaviour
{
    [SerializeField] private float dashDistance = 6f;
    [SerializeField] private float dashSpeed = 40f; // used for lerp speed
    [SerializeField] private float dashCooldown = 0.5f;
    [SerializeField] private float recoveryAfterMiss = 0.5f; // penalty time multiplier when miss

    Rigidbody2D rb;
    Vector2 dashTarget;
    bool dashing = false;
    float dashTimer = 0f;
    float cooldownTimer = 0f;

    // For drag-to-dash
    bool isDragging = false;
    Vector2 startDragPos;

    // Events to communicate hits/misses
    public Action OnDashStart;
    public Action<bool> OnDashEnd; // bool = hit or miss

    void Awake() { rb = GetComponent<Rigidbody2D>(); }

    void Update()
    {
        cooldownTimer -= Time.deltaTime;
        if (!dashing && cooldownTimer <= 0f)
        {
            if (Input.touchCount > 0) // Mobile touch input
            {
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    isDragging = true;
                    startDragPos = Camera.main.ScreenToWorldPoint(touch.position);
                }

                if (isDragging && touch.phase == TouchPhase.Ended)
                {
                    Vector2 endPos = Camera.main.ScreenToWorldPoint(touch.position);
                    Vector2 dir = endPos - startDragPos; // direction of drag, opposite of slingshot
                    if (dir.sqrMagnitude < 0.001f) dir = Vector2.up; // fallback
                    dir.Normalize();
                    StartDash(dir);
                    isDragging = false;
                }
            }
            else // PC mouse input
            {
                if (Input.GetMouseButtonDown(0))
                {
                    isDragging = true;
                    startDragPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                }

                if (isDragging && Input.GetMouseButtonUp(0))
                {
                    Vector2 endPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    Vector2 dir = endPos - startDragPos; // direction of drag, opposite of slingshot
                    if (dir.sqrMagnitude < 0.001f) dir = Vector2.up; // fallback
                    dir.Normalize();
                    StartDash(dir);
                    isDragging = false;
                }
            }
        }

        if (dashing)
        {
            dashTimer += Time.deltaTime * dashSpeed;
            Vector2 newPos = Vector2.Lerp((Vector2)transform.position, dashTarget, dashTimer);
            rb.MovePosition(newPos);

            if (Vector2.Distance(transform.position, dashTarget) < 0.1f)
            {
                EndDash(false); // ended without colliding
            }
        }
    }

    void StartDash(Vector2 dir)
    {
        dashing = true;
        dashTimer = 0f;
        dashTarget = (Vector2)transform.position + dir * dashDistance;
        cooldownTimer = dashCooldown;
        OnDashStart?.Invoke();
    }

    public void EndDash(bool hit)
    {
        dashing = false;
        // Slight penalty on miss: you could call recovery/punish elsewhere
        OnDashEnd?.Invoke(hit);
    }

    // Called by collision with enemy: stop dash and register hit
    void OnTriggerEnter2D(Collider2D col)
    {
        if (!dashing) return;
        if (col.CompareTag("Enemy") || col.CompareTag("EnergyNode"))
        {
            dashing = false;
            OnDashEnd?.Invoke(true);
            // Optionally push through multiple enemies if piercing is on
        }
    }
}