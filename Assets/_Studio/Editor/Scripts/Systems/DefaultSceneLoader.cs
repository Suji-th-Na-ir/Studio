using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using Terra.Studio;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class DefaultSceneLoader
{
    private static bool loadedDefaultScene = false;
    public static Action<bool> IsEnteringPlayerMode;

    static DefaultSceneLoader()
    {
        EditorApplication.playModeStateChanged += LoadDefaultScene;
    }

    private static void LoadDefaultScene(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode)
        {
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
        }

        if (state == PlayModeStateChange.EnteredPlayMode && !loadedDefaultScene)
        {
            var system = Resources.Load<SystemConfigurationSO>("System/SystemSettings");
            if (system.LoadDefaultSceneOnPlay)
            {
                loadedDefaultScene = true;
                var currentScene = SceneManager.GetActiveScene();
                var scenesInBuildSettings = EditorBuildSettings.scenes;
                if (scenesInBuildSettings != null && scenesInBuildSettings.Length > 0)
                {
                    var scene = Path.GetFileNameWithoutExtension(scenesInBuildSettings[0].path);
                    if (scene.Equals(currentScene.name))
                    {
                        IsEnteringPlayerMode?.Invoke(loadedDefaultScene);
                        return;
                    }
                    SceneManager.LoadScene(0);
                    IsEnteringPlayerMode?.Invoke(loadedDefaultScene);
                }
            }
        }

        if (state == PlayModeStateChange.EnteredEditMode)
        {
            loadedDefaultScene = false;
            IsEnteringPlayerMode?.Invoke(loadedDefaultScene);
        }
    }
}