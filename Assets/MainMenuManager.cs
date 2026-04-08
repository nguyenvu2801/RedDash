using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;

public class MainMenuManager : MonoBehaviour
{
    public GameObject settingsPanel;
    public Slider musicSlider;
    public Slider sfxSlider;

    public Image transitionPanel;
    public float transitionDuration = 0.5f;

    void Start()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (transitionPanel != null)
        {
            Color c = transitionPanel.color;
            c.a = 0f;
            transitionPanel.color = c;
        }

        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayMusic("Background");

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        RegisterAllButtons();
        InitSliders();
    }

    void InitSliders()
    {
        if (musicSlider != null)
        {
            musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1f);
            musicSlider.onValueChanged.AddListener(OnMusicSliderChanged);
        }

        if (sfxSlider != null)
        {
            sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);
            sfxSlider.onValueChanged.AddListener(OnSFXSliderChanged);
        }
    }

    public void PlayGame()
    {
        StartTransition("PreRunLobby");
    }

    void StartTransition(string sceneName)
    {
        if (transitionPanel == null)
        {
            SceneManager.LoadScene(sceneName);
            return;
        }

        transitionPanel.DOFade(1f, transitionDuration)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                SceneManager.LoadScene(sceneName);
            });
    }

    public void OpenSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(true);
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

    void OnMusicSliderChanged(float value)
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.SetMusicVolume(value);
    }

    void OnSFXSliderChanged(float value)
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.SetSFXVolume(value);
    }

    public void QuitGame()
    {
        if (transitionPanel != null)
        {
            transitionPanel.DOFade(1f, transitionDuration)
                .SetEase(Ease.Linear)
                .OnComplete(() =>
                {
                    Application.Quit();
#if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
#endif
                });
        }
        else
        {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}