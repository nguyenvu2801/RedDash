using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    public GameObject settingsPanel;

    void Start()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayMusic("Background");
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        RegisterAllButtons();
    }

    public void PlayGame()
    {
        SceneManager.LoadScene("PreRunLobby");
    }

    public void OpenSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(true);
    }
    public void OnMusicSliderChanged(float value)
    {
        SoundManager.Instance.SetMusicVolume(value);
    }

    public void OnSFXSliderChanged(float value)
    {
        SoundManager.Instance.SetSFXVolume(value);
    }
    public void CloseSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }
    void RegisterAllButtons()
    {
        Button[] allButtons = FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (Button btn in allButtons)
        {
            btn.onClick.AddListener(() =>
            {
                SoundManager.Instance.PlaySFX("Button");
            });
        }
    }
    public void QuitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}