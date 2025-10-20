// InteractableBase.cs
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public class InteractableBase : MonoBehaviour, IInteractable
{
    public string promptText = "Press E to interact";
    public SpriteRenderer highlightRenderer;
    public float highlightAlpha = 0.6f;

    Color originalColor;
    bool hovered = false;

    void Awake()
    {
        if (highlightRenderer) originalColor = highlightRenderer.color;
    }

    public virtual void Interact(GameObject interactor)
    {
        // override in derived classes
        Debug.Log($"Interacted with {gameObject.name}");
    }

    public virtual void OnHoverEnter()
    {
        if (hovered) return;
        hovered = true;
        if (highlightRenderer)
        {
            var c = originalColor;
            c.a = highlightAlpha;
            highlightRenderer.color = c;
        }
    }

    public virtual void OnHoverExit()
    {
        if (!hovered) return;
        hovered = false;
        if (highlightRenderer) highlightRenderer.color = originalColor;
    }
}
