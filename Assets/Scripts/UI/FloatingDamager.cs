using UnityEngine;
using TMPro;
using System.Collections;

public class FloatingDamager : MonoBehaviour
{
    public TMP_Text text;
    public float lifetime = 0.6f;
    public float riseSpeed = 1.5f;
    public float fadeSpeed = 3f;
    public Vector2 randomOffset = new Vector2(0.7f, 0.4f);

    private Color startColor;

    void Awake()
    {
        startColor = text.color;
    }

    public void Show(int amount)
    {
        // random slight offset like Hades
        transform.localPosition += new Vector3(
            Random.Range(-randomOffset.x, randomOffset.x),
            Random.Range(0, randomOffset.y),
            0
        );

        text.text = amount.ToString();
        text.color = startColor;

        StartCoroutine(PlayAnim());
    }

    private IEnumerator PlayAnim()
    {
        float t = 0f;

        while (t < lifetime)
        {
            // upward move
            transform.position += Vector3.up * riseSpeed * Time.deltaTime;

            // fade
            Color c = text.color;
            c.a = Mathf.Lerp(1f, 0f, t / lifetime);
            text.color = c;

            t += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }
}
