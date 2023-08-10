using UnityEngine;
using UnityEditor;
using Terra.Studio;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public static class DefaultSceneLoader
{
    private static bool loadedDefaultScene = false;

    static DefaultSceneLoader()
    {
        EditorApplication.playModeStateChanged += LoadDefaultScene;
    }

    static void LoadDefaultScene(PlayModeStateChange state)
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
                UnityEngine.SceneManagement.SceneManager.LoadScene(0);
                loadedDefaultScene = true;
            }
        }

        if (state == PlayModeStateChange.EnteredEditMode)
        {
            loadedDefaultScene = false;
        }
    }
}