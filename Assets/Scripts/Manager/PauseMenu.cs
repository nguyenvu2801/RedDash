using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using DG.Tweening;

public class PauseMenu : MonoBehaviour
{
    [Header("UI References")]
    public RectTransform pausePanel;

    [Header("Post Processing")]
    public Volume globalVolume;
    private ColorAdjustments colorAdjustments;

    [Header("Animation Settings")]
    public float duration = 0.5f;
    public Ease openEase = Ease.OutBack;
    public Ease closeEase = Ease.InBack;

    private bool isPaused = false;

    void Start()
    {
        // Hide panel initially
        pausePanel.localScale = Vector3.zero;
        pausePanel.gameObject.SetActive(false);

        // Try to get color adjustment
        if (globalVolume != null)
        {
            if (!globalVolume.profile.TryGet(out colorAdjustments))
            {
                Debug.LogError(" No ColorAdjustments override found in the Volume!");
            }
        }
        else
        {
            Debug.LogError(" Global Volume is not assigned!");
        }
    }
    public void OnMusicSliderChanged(float value)
    {
        SoundManager.Instance.SetMusicVolume(value);
    }

    public void OnSFXSliderChanged(float value)
    {
        SoundManager.Instance.SetSFXVolume(value);
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            print("aaa");
            TogglePause();
        }
    }

    void TogglePause()
    {
        isPaused = !isPaused;
        Debug.Log($"Pause toggled: {isPaused}");

        if (isPaused)
        {
            // Pause gameplay
            Time.timeScale = 0f;

            pausePanel.gameObject.SetActive(true);

            // Animate panel
            pausePanel.DOScale(1f, duration)
                .SetEase(openEase)
                .SetUpdate(true); // run even when Time.timeScale = 0

            // Desaturate screen
            if (colorAdjustments != null)
            {
                DOTween.To(
                    () => colorAdjustments.saturation.value,
                    x => colorAdjustments.saturation.value = x,
                    -100f,
                    duration
                ).SetUpdate(true);
            }
        }
        else
        {
            // Unpause
            pausePanel.DOScale(0f, duration)
                .SetEase(closeEase)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    pausePanel.gameObject.SetActive(false);
                    Time.timeScale = 1f;
                });

            if (colorAdjustments != null)
            {
                DOTween.To(
                    () => colorAdjustments.saturation.value,
                    x => colorAdjustments.saturation.value = x,
                    0f,
                    duration
                ).SetUpdate(true);
            }
        }
    }
}
