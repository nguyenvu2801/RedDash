using UnityEngine;

[System.Serializable]
public class Sound
{
    public string name;
    public AudioClip clip;
    [Range(0f, 1f)] public float volume = 1f;
    [Range(0.5f, 1.5f)] public float pitch = 1f;
    public bool loop = false;
    public bool useRandomPitch = false;
    public float randomPitchMin = 0.9f;
    public float randomPitchMax = 1.1f;
}