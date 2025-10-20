using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartRunInteractable : InteractableBase
{
    public string runSceneName = "RunScene";
    public float fadeTime = 0.5f;
    public override void Interact(GameObject interactor)
    {
        // optionally do fade out, save state, then load run scene
        Debug.Log("Starting run: " + runSceneName);
        // Example:
       SceneManager.LoadScene(runSceneName);
    }
}
