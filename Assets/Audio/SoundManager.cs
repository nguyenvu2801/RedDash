using UnityEngine;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [SerializeField] private Sound[] musicSounds;
    [SerializeField] private Sound[] sfxSounds;

    private Dictionary<string, Sound> musicDict = new Dictionary<string, Sound>();
    private Dictionary<string, Sound> sfxDict = new Dictionary<string, Sound>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        foreach (var sound in musicSounds)
            musicDict[sound.name] = sound;

        foreach (var sound in sfxSounds)
            sfxDict[sound.name] = sound;

        musicSource.volume = 0.7f;
        sfxSource.volume = 1f;
    }

    public void PlayMusic(string name)
    {
        if (musicDict.TryGetValue(name, out Sound sound))
        {
            musicSource.clip = sound.clip;
            musicSource.volume = sound.volume;
            musicSource.pitch = sound.pitch;
            musicSource.loop = sound.loop;
            musicSource.Play();
        }
    }

    public void StopMusic() => musicSource.Stop();
    public void PauseMusic() => musicSource.Pause();
    public void ResumeMusic() => musicSource.UnPause();

    public void PlaySFX(string name)
    {
        if (sfxDict.TryGetValue(name, out Sound sound))
        {
            float pitch = sound.pitch;
            if (sound.useRandomPitch)
                pitch = Random.Range(sound.randomPitchMin, sound.randomPitchMax);

            sfxSource.pitch = pitch;
            sfxSource.PlayOneShot(sound.clip, sound.volume);
        }
    }

    public void PlaySFX(string name, Vector3 position)
    {
        if (sfxDict.TryGetValue(name, out Sound sound))
        {
            AudioSource.PlayClipAtPoint(sound.clip, position, sound.volume);
        }
    }

    public void SetMusicVolume(float volume)
    {
        musicSource.volume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat("MusicVolume", volume);
    }

    public void SetSFXVolume(float volume)
    {
        sfxSource.volume = Mathf.Clamp01(volume);
        PlayerPrefs.SetFloat("SFXVolume", volume);
    }

    public float GetMusicVolume() => PlayerPrefs.GetFloat("MusicVolume", 0.7f);
    public float GetSFXVolume() => PlayerPrefs.GetFloat("SFXVolume", 1f);
}