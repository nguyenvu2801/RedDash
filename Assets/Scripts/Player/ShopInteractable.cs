using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ShopInteractable : InteractableBase
{
    public GameObject shopPanelPrefab; // assign a shop UI
   
    GameObject instance;
    public void Start()
    {
        shopPanelPrefab.SetActive(false);
        SoundManager.Instance.PlayMusic("PreRunLobby");
    }
    public void Update()
    {
       
        if (Input.GetKeyDown(KeyCode.Escape)) { CloseShop(); };
    }
    public override void Interact(GameObject interactor)
    {
        if (instance != null) return;
        shopPanelPrefab.SetActive(true);
    }

    public void CloseShop()
    {
        shopPanelPrefab.SetActive(false);
    }
}
