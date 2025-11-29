using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;
using Cinemachine;

public class GameManager : GameSingleton<GameManager>
{
    [Header("Game Over UI")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Image fadeBackground;     // NEW
    [SerializeField] private Button returnToSafeHouseButton;

    [Header("Scenes")]
    [SerializeField] private string safeHouseSceneName = "PreRunLobby";

    private CinemachineVirtualCamera vcam;
    private CinemachineBasicMultiChannelPerlin shakeNoise;
    private CinemachineFramingTransposer framing;

    public bool IsGameOver { get; private set; }

    protected override void Awake()
    {
        base.Awake();

        if (gameOverPanel) gameOverPanel.SetActive(false);

        vcam = FindObjectOfType<CinemachineVirtualCamera>();

        if (vcam)
        {
            framing = vcam.GetCinemachineComponent<CinemachineFramingTransposer>();
            shakeNoise = vcam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        }

        
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

        // *** 1. Slow heavy death moment ***
        Time.timeScale = 0.25f;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;

        
           fadeBackground.DOFade(0.18f, 0.45f);  // softer red, slower fade

        // *** 1c. Camera SHAKE ***
        StartCoroutine(ShakeCamera(0.2f, 0.4f));

        // *** 2. Camera DRAG DOWN (Hades style) ***
        if (framing != null)
        {
            DOTween.To(() => framing.m_TrackedObjectOffset,
                       x => framing.m_TrackedObjectOffset = x,
                       new Vector3(0, -0.45f, 0),
                       1.1f)
                   .SetEase(Ease.OutCubic);
        }

        // *** 3. Heavy fade-in ***
        yield return fadeBackground
            .DOFade(0.88f, 1.6f)   // less dark than before
            .SetEase(Ease.InOutQuad)
            .WaitForCompletion();


        // Restore timescale (slow)
        DOTween.To(() => Time.timeScale, x => Time.timeScale = x, 1f, 1.2f);
        DOTween.To(() => Time.fixedDeltaTime, x => Time.fixedDeltaTime = x, 0.02f, 1.2f);

        // *** 4. Bring in the button (heavy weight) ***
        returnToSafeHouseButton.gameObject.SetActive(true);
        Transform t = returnToSafeHouseButton.transform;

        t.localScale = Vector3.zero;
        t.DOScale(1.15f, 0.8f).SetEase(Ease.OutBack);

        yield return t.DOScale(1f, 0.2f).WaitForCompletion();
    }

    // *** CAMERA SHAKE FUNCTION ***
    private IEnumerator ShakeCamera(float duration, float intensity)
    {
        if (shakeNoise == null)
            yield break;

        shakeNoise.m_AmplitudeGain = intensity;

        yield return new WaitForSecondsRealtime(duration);

        // smooth out
        DOTween.To(() => shakeNoise.m_AmplitudeGain,
                   x => shakeNoise.m_AmplitudeGain = x,
                   0f, 0.6f);
    }

    public void ReturnToSafeHouse()
    {
        if (IsGameOver)
            StartCoroutine(LoadSafeHouseWithFade());
    }

    private IEnumerator LoadSafeHouseWithFade()
    {
        returnToSafeHouseButton.interactable = false;

        // Fade to full black
        yield return fadeBackground
            .DOFade(1f, 0.7f)
            .SetEase(Ease.InCubic)
            .WaitForCompletion();

        IsGameOver = false;
        Time.timeScale = 1f;

        SceneManager.LoadScene(safeHouseSceneName);
    }

    private void Update()
    {
        if (IsGameOver && Input.anyKeyDown)
        {
            ReturnToSafeHouse();
        }
    }
}
