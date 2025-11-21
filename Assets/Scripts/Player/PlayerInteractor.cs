using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    public float interactRadius = 1.5f;
    public LayerMask interactableLayer;

    IInteractable current = null;
    InteractableBase currentBase = null;

    void Update()
    {
        FindNearestInteractable();

        // ONLY interact by pressing E
        if (Input.GetKeyDown(KeyCode.E))
        {
            TryInteract();
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

    // Optional: visualise radius
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }
}
