using UnityEngine;

[CreateAssetMenu(fileName = "CharacterAnimations", menuName = "Animations/CharacterAnimations", order = 1)]
public class CharacterAnimation : ScriptableObject
{
    [System.Serializable]
    public class AnimationClip
    {
        public string name;          // e.g., "Idle", "Attack"
        public Sprite[] frames;      // Array of sprites for each frame
        public float frameRate = 10f; // Frames per second (e.g., 10 means 0.1s per frame)
        public bool loop = true;     // Should this animation loop? (e.g., true for Idle, false for Die)
    }

    public AnimationClip idle;
    public AnimationClip attack;
    public AnimationClip hurt;
    public AnimationClip die;
}