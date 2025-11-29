using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;
using System;

public class GameManager : GameSingleton<GameManager>
{
    [Header("Game Over UI")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Image fadeBackground;     // Black panel that fades in
    [SerializeField] private Button returnToSafeHouseButton;
    [Header("Death FX")]
    [SerializeField] private Image redPulse;       // Transparent red circle or panel
    [SerializeField] private float redPulseMax = 2.5f;
    [SerializeField] private float cameraZoomAmount = 0.3f;
    [SerializeField] private float shakeIntensity = 0.4f;
    [Header("Scenes")]
    [SerializeField] private string safeHouseSceneName = "PreRunLobby";

    public bool IsGameOver { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        // Hide panel at start
        if (gameOverPanel) gameOverPanel.SetActive(false);
    }
    public void TriggerGameOver()
    {
        if (IsGameOver) return;
        IsGameOver = true;
        StartCoroutine(GameOverSequence());
    }

    private IEnumerator GameOverSequence()
    {
        gameOverPanel.SetActive(true);

        // SCREEN SHAKE (requires DOTween Pro or your shake extension)
        Camera.main.transform.DOShakePosition(0.3f, shakeIntensity);

        // CAMERA ZOOM IN
        Camera.main.transform.DOBlendableLocalMoveBy(
            new Vector3(0, 0, cameraZoomAmount), 0.5f
        ).SetEase(Ease.OutQuad);

        // RED PULSE FLASH
        redPulse.color = new Color(1, 0, 0, 0.35f);
        redPulse.transform.localScale = Vector3.zero;
        redPulse.transform.DOScale(redPulseMax, 0.55f).SetEase(Ease.OutCubic);
        redPulse.DOFade(0, 0.55f);

        // BACKGROUND FADE
        fadeBackground.color = new Color(0, 0, 0, 0);
        yield return fadeBackground.DOFade(0.95f, 0.8f)
            .SetEase(Ease.OutCubic).WaitForCompletion();

        // BUTTON APPEAR
        returnToSafeHouseButton.gameObject.SetActive(true);
        returnToSafeHouseButton.transform.localScale = Vector3.zero;
        returnToSafeHouseButton.transform.DOScale(1.1f, 0.4f).SetEase(Ease.OutBack);
        yield return returnToSafeHouseButton.transform.DOScale(1f, 0.15f).WaitForCompletion();
    }


    // Call this from your Button's OnClick()
    public void ReturnToSafeHouse()
    {
        if (IsGameOver)
            StartCoroutine(LoadSafeHouseWithFade());
    }

    private IEnumerator LoadSafeHouseWithFade()
    {
        // Disable button to prevent double click
        returnToSafeHouseButton.interactable = false;

        // Fade to full black
        yield return fadeBackground.DOFade(1f, 0.7f).SetEase(Ease.InCubic).WaitForCompletion();

        // Reset game state
        IsGameOver = false;
        Time.timeScale = 1f;

        // Load Safe House
        SceneManager.LoadScene(safeHouseSceneName);
    }

    // Optional: Allow pressing any key or tap to return faster
    private void Update()
    {
        if (IsGameOver && Input.anyKeyDown)
        {
            ReturnToSafeHouse();
        }
    }
}