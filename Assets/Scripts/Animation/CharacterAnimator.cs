using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CharacterAnimator : MonoBehaviour
{
    [SerializeField] private CharacterAnimation animations; // Assign your SO here in Inspector
    private SpriteRenderer spriteRenderer;
    private Dictionary<string, CharacterAnimation.AnimationClip> clipDictionary;
    private Coroutine currentAnimationCoroutine;
    private string currentAnimationName;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer not found on " + gameObject.name);
        }

        // Build a dictionary for quick lookup (since we have minimal animations)
        clipDictionary = new Dictionary<string, CharacterAnimation.AnimationClip>
        {
            { "Idle", animations.idle },
            { "Attack", animations.attack },
            { "Hurt", animations.hurt },
            { "Die", animations.die }
        };
    }

    private void Start()
    {
        // Start with Idle by default
        PlayAnimation("Idle");
    }
    public void PlayAnimation(string animationName)
    {
        if (currentAnimationName == animationName) return; // Already playing

        if (currentAnimationCoroutine != null)
        {
            StopCoroutine(currentAnimationCoroutine);
        }

        if (clipDictionary.TryGetValue(animationName, out var clip))
        {
            currentAnimationCoroutine = StartCoroutine(PlayClip(clip));
            currentAnimationName = animationName;
        }
        else
        {
            Debug.LogWarning("Animation not found: " + animationName);
        }
    }

    private IEnumerator PlayClip(CharacterAnimation.AnimationClip clip)
    {
        float delay = 1f / clip.frameRate;
        int frameIndex = 0;

        while (true)
        {
            if (frameIndex >= clip.frames.Length)
            {
                if (clip.loop)
                {
                    frameIndex = 0; // Loop back
                }
                else
                {
                    // One-shot: Stay on last frame and exit
                    spriteRenderer.sprite = clip.frames[clip.frames.Length - 1];
                }
            }

            spriteRenderer.sprite = clip.frames[frameIndex];
            frameIndex++;

            yield return new WaitForSeconds(delay);
        }
    }
}