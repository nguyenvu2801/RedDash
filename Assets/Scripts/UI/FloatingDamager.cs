using TMPro;
using UnityEngine;
using System.Collections;

public class FloatingDamager : MonoBehaviour
{
    public TMP_Text text;
    public float lifetime = 0.6f;
    public float riseSpeed = 1.5f;
    public float fadeSpeed = 3f;
    public Vector2 randomOffset = new Vector2(0.7f, 0.4f);

    private Color startColor;
    private Coroutine animRoutine;

    void Awake()
    {
        startColor = text.color;
    }

    private void OnEnable()
    {
        // reset alpha instantly when reused
        text.color = startColor;

        // reset local position so offset does not accumulate
        transform.localPosition = Vector3.zero;
    }

    public void Show(int amount)
    {
        // random offset
        transform.localPosition += new Vector3(
            Random.Range(-randomOffset.x, randomOffset.x),
            Random.Range(0, randomOffset.y),
            0
        );

        text.text = amount.ToString();

        // stop previously running coroutine (when reused from pool)
        if (animRoutine != null)
            StopCoroutine(animRoutine);

        animRoutine = StartCoroutine(PlayAnim());
    }

    private IEnumerator PlayAnim()
    {
        float t = 0f;

        while (t < lifetime)
        {
            transform.position += Vector3.up * riseSpeed * Time.deltaTime;

            // fade out
            Color c = text.color;
            c.a = Mathf.Lerp(1f, 0f, t / lifetime);
            text.color = c;

            t += Time.deltaTime;
            yield return null;
        }

        // return to pool, do not destroy
        PoolManager.Instance.ReturnToPool(PoolKey.damagePopup, gameObject);
    }
}
