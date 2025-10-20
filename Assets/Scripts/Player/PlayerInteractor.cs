using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerInteractor : MonoBehaviour
{
    public float interactRadius = 1.5f;
    public LayerMask interactableLayer;

    IInteractable current = null;
    InteractableBase currentBase = null;

    void Update()
    {
        FindNearestInteractable();

        // keyboard / mouse input
        if (Input.GetKeyDown(KeyCode.E))
        {
            TryInteract();
        }

        // mouse left click (desktop)
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 wp = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            TryTapAtPoint(wp);
        }

        // touch (mobile)
        if (Input.touchCount > 0)
        {
            var touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Ended)
            {
                Vector2 wp = Camera.main.ScreenToWorldPoint(touch.position);
                TryTapAtPoint(wp);
            }
        }
    }

    void FindNearestInteractable()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, interactRadius, interactableLayer);
        IInteractable closest = null;
        float best = float.MaxValue;
        InteractableBase closestBase = null;

        foreach (var c in hits)
        {
            var inter = c.GetComponentInParent<IInteractable>();
            if (inter == null) continue;
            float d = Vector2.SqrMagnitude((Vector2)c.transform.position - (Vector2)transform.position);
            if (d < best)
            {
                best = d;
                closest = inter;
                closestBase = c.GetComponentInParent<InteractableBase>();
            }
        }

        if (closest != current)
        {
            if (currentBase != null) currentBase.OnHoverExit();
            current = closest;
            currentBase = closestBase;
            if (currentBase != null) currentBase.OnHoverEnter();
        }
    }

    void TryInteract()
    {
        if (current != null)
        {
            current.Interact(gameObject);
        }
    }

    void TryTapAtPoint(Vector2 worldPoint)
    {
        RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero, 0f, interactableLayer);
        if (hit.collider != null)
        {
            var inter = hit.collider.GetComponentInParent<IInteractable>();
            if (inter != null)
            {
                inter.Interact(gameObject);
                return;
            }
        }

        // if no direct tap, fallback to nearest
        TryInteract();
    }

    // Optional: visualise radius in editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }
}
