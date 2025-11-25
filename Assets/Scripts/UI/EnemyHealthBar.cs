using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    [Header("UI")]
    public Image fillImage;
    public CanvasGroup canvasGroup;

    [Header("Settings")]
    public float followYOffset = 1.2f;
    public float smoothSpeed = 8f;
    public float fadeSpeed = 4f;

    private Transform target;
    private float targetFill = 1f;
    private float currentFill = 1f;
    private bool isVisible = false;

    public void Init(Transform target)
    {
        this.target = target;
        canvasGroup.alpha = 0f;
        isVisible = false;
    }

    public void SetHP(float current, float max)
    {
        targetFill = current / max;
        if (targetFill < 1f)
            Show();
    }

    void LateUpdate()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        // FOLLOW ENEMY
        transform.position = target.position + Vector3.up * followYOffset;

        // SMOOTH FILL
        currentFill = Mathf.Lerp(currentFill, targetFill, Time.deltaTime * smoothSpeed);
        fillImage.fillAmount = currentFill;

        // AUTO HIDE WHEN FULL
        if (isVisible && targetFill >= 0.999f)
            Hide();
    }

    private void Show()
    {
        isVisible = true;
        StopAllCoroutines();
        StartCoroutine(Fade(1f));
    }

    private void Hide()
    {
        isVisible = false;
        StopAllCoroutines();
        StartCoroutine(Fade(0f));
    }

    private System.Collections.IEnumerator Fade(float targetAlpha)
    {
        while (!Mathf.Approximately(canvasGroup.alpha, targetAlpha))
        {
            canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, targetAlpha, Time.deltaTime * fadeSpeed);
            yield return null;
        }
    }
}
