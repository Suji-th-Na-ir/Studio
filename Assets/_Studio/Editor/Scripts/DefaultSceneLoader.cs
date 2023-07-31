using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoadAttribute]
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
            EditorSceneManager.LoadScene(0);
            loadedDefaultScene = true;
        }

        if (state == PlayModeStateChange.EnteredEditMode)
        {
            loadedDefaultScene = false;
        }
    }
}