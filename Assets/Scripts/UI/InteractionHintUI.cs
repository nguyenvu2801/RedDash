using TMPro;
using UnityEngine;
using DG.Tweening;

public class InteractionHintUI : MonoBehaviour
{
    public static InteractionHintUI Instance;

    public TextMeshProUGUI hintText;
    public Vector3 offset = new Vector3(0, 2f, 0); // offset above player
    public float floatAmplitude = 0.3f; // up/down motion
    public float floatSpeed = 1f;       // speed of floating

    private Transform target; // the player
    private CanvasGroup canvasGroup;
    private Vector3 baseScreenPosition;

    private void Awake()
    {
        Instance = this;

        // Ensure canvas group exists
        canvasGroup = hintText.gameObject.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = hintText.gameObject.AddComponent<CanvasGroup>();

        hintText.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (target != null)
        {
            // Base screen position above the player
            baseScreenPosition = Camera.main.WorldToScreenPoint(target.position + offset);

            // Floating motion (sin wave)
            Vector3 floatingPos = baseScreenPosition;
            floatingPos.y += Mathf.Sin(Time.time * floatSpeed) * floatAmplitude * 100f; // multiply to match screen coords

            hintText.transform.position = floatingPos;
        }
    }

    public void ShowHint(string text, Transform playerTransform)
    {
        target = playerTransform;
        hintText.text = text;

        // Reset before animation
        hintText.transform.localScale = Vector3.zero;
        canvasGroup.alpha = 0f;
        hintText.gameObject.SetActive(true);

        // Magical pop animation
        hintText.transform.DOScale(1f, 0.5f).SetEase(Ease.OutBack);
        canvasGroup.DOFade(1f, 0.5f);

        // Glow color effect
        hintText.DOColor(Color.cyan, 0.7f).SetLoops(-1, LoopType.Yoyo);
    }

    public void HideHint()
    {
        target = null;

        // Magic vanish animation
        hintText.transform.DOKill(); // stop any ongoing tweens (color, scale)
        hintText.transform.DOScale(0f, 0.3f).SetEase(Ease.InBack);
        canvasGroup.DOFade(0f, 0.3f).OnComplete(() => hintText.gameObject.SetActive(false));
    }
}
