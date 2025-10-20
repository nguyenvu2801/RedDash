using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ShopInteractable : InteractableBase
{
    public GameObject shopPanelPrefab; // assign a shop UI
    [SerializeField] private TextMeshProUGUI currencyText;
    GameObject instance;
    public void Start()
    {
        shopPanelPrefab.SetActive(false);
    }
    public void Update()
    {
        if (CurrencyManager.Instance != null)
        {
            currencyText.text = "Essence: " + CurrencyManager.Instance.GetCurrency().ToString();
        }
        if (Input.GetKeyDown(KeyCode.Escape)) { CurrencyManager.Instance.AddCurrency(10); };
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
