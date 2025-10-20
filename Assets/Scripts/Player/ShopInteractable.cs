using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopInteractable : InteractableBase
{
    public GameObject shopPanelPrefab; // assign a shop UI
    GameObject instance;
    public void Start()
    {
        shopPanelPrefab.SetActive(false);
    }
    public override void Interact(GameObject interactor)
    {
        if (instance != null) return;
        shopPanelPrefab.SetActive(true);
    }

    public void CloseShop()
    {
        if (instance != null) Destroy(instance);
    }
}
