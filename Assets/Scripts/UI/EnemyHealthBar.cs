using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    [Header("UI References")]
    public Image fillInstant;   // main red bar
    public Image fillDelayed;   // delayed dark red bar
    public Image flashImage;    // white flash overlay
    public CanvasGroup canvasGroup;

    [Header("Behaviour Settings")]
    public float followYOffset = 1.2f;
    public float fillSmoothSpeed = 10f;
    public float delayedSmoothSpeed = 3f;
    public float fadeSpeed = 4f;
    public float shakeAmount = 0.06f;
    public float shakeDuration = 0.15f;
    public float flashDuration = 0.1f;

    private Transform target;
    public Transform visual;
    private float targetFill = 1f;
    private float currentFill = 1f;
    private float delayedFill = 1f;

    private Vector3 originalLocalPos;
    private float shakeTimer = 0f;
    private bool visible = false;

    public void Init(Transform target)
    {
        this.target = target;
        canvasGroup.alpha = 0f;
        visible = false;

        originalLocalPos = visual.localPosition;

        // reset effects
        flashImage.color = new Color(1, 1, 1, 0);
        fillDelayed.fillAmount = 1f;
        fillInstant.fillAmount = 1f;
    }

    public void SetHP(float current, float max)
    {
        targetFill = current / max;

        if (targetFill < 1f)
            Show();

        // On damage, trigger effects
        Flash();
        Shake();
    }

    void LateUpdate()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        // FOLLOW TARGET
        transform.position = target.position + Vector3.up * followYOffset;

        // SMOOTH MAIN BAR
        currentFill = Mathf.Lerp(currentFill, targetFill, Time.deltaTime * fillSmoothSpeed);
        fillInstant.fillAmount = currentFill;

        // SMOOTH DELAYED BAR (always lags behind)
        delayedFill = Mathf.Lerp(delayedFill, targetFill, Time.deltaTime * delayedSmoothSpeed);
        fillDelayed.fillAmount = delayedFill;

        // AUTO-HIDE
        if (visible && targetFill >= 0.999f)
            Hide();

        // SHAKE EFFECT
        if (shakeTimer > 0)
        {
            shakeTimer -= Time.deltaTime;
            float strength = shakeAmount * (shakeTimer / shakeDuration);
            visual.localPosition = originalLocalPos + (Vector3)Random.insideUnitCircle * strength;
        }
        else
        {
            visual.localPosition = originalLocalPos;
        }
    }

    // ---- EFFECTS ----

    private void Flash()
    {
        StopCoroutine("FlashRoutine");
        StartCoroutine("FlashRoutine");
    }

    private IEnumerator FlashRoutine()
    {
        flashImage.color = new Color(1, 1, 1, 0.8f);

        float t = 0f;
        while (t < flashDuration)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(0.8f, 0f, t / flashDuration);
            flashImage.color = new Color(1, 1, 1, a);
            yield return null;
        }

        flashImage.color = new Color(1, 1, 1, 0f);
    }

    private void Shake()
    {
        shakeTimer = shakeDuration;
    }

    private void Show()
    {
        visible = true;
        StopCoroutine("Fade");
        StartCoroutine(Fade(1f));
    }

    private void Hide()
    {
        visible = false;
        StopCoroutine("Fade");
        StartCoroutine(Fade(0f));
    }

    private IEnumerator Fade(float targetAlpha)
    {
        while (!Mathf.Approximately(canvasGroup.alpha, targetAlpha))
        {
            canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, targetAlpha, Time.deltaTime * fadeSpeed);
            yield return null;
        }
    }
}
