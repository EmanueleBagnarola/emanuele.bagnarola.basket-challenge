using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SceneManagementEvents
{
    public delegate void OnSceneLoadHandler(string sceneName);
    public static event OnSceneLoadHandler OnSceneLoad;

    public delegate void OnSceneLoadedHandler(string sceneName);
    public static event OnSceneLoadedHandler OnSceneLoaded;
    
    public static void TriggerSceneLoad(string sceneName)
    {
        OnSceneLoad?.Invoke(sceneName);
    }

    public static void TriggerSceneLoaded(string sceneName)
    {
        OnSceneLoaded?.Invoke(sceneName);
    }
}
