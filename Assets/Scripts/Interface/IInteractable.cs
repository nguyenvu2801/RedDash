using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public interface IInteractable
{
    // Called when player interacts
    void Interact(GameObject interactor);
    // Optional: show hover/highlight (called by Interactor)
    void OnHoverEnter();
    void OnHoverExit();
}