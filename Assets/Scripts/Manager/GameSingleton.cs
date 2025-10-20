using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSingleton<T> : MonoBehaviour where T : Component
{
    protected static T _instance;
    public static bool HasInstance => _instance != null;
    public static T TryGetInstance() => HasInstance ? _instance : null;
    public static T Current => _instance;

    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<T>();
                if (_instance == null)
                {
                    // Optionally log a warning instead of creating
                    Debug.LogWarning($"No instance of {typeof(T).Name} found in the scene. Add one manually.");
                    return null;  
                }
            }
            return _instance;
        }
    }

    // Add this flag to control persistence (set in child classes or inspector)
    [SerializeField] protected bool persistAcrossScenes = false;

    protected virtual void Awake()
    {
        InitializeSingleton();
    }

    protected virtual void InitializeSingleton()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        // Duplicate check: Destroy this if another instance exists
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this as T;

        // Optional persistence
        if (persistAcrossScenes)
        {
            DontDestroyOnLoad(gameObject);
        }
    }

    // Optional: Reset on destroy to allow re-creation in new scenes
    protected virtual void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }
}