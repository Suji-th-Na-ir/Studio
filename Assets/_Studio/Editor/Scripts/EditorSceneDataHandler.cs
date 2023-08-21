using UnityEditor;
using UnityEngine;

namespace Terra.Studio.RTEditor
{
    [InitializeOnLoad]
    public static class EditorSceneDataHandler
    {
        private const string SOPATH = "Assets/_Studio/Resources/System/SystemSettings.asset";

        static EditorSceneDataHandler()
        {
            DefaultSceneLoader.IsEnteringPlayerMode += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(bool didEnterPlayMode)
        {
            var asset = AssetDatabase.LoadAssetAtPath<SystemConfigurationSO>(SOPATH);
            if (!asset.PickupSavedData)
            {
                return;
            }
            var fileRef = asset.SceneDataToLoad;
            var filePath = AssetDatabase.GetAssetPath(fileRef);
            var persistentPath = FileService.GetSavedFilePath();
            if (didEnterPlayMode)
            {
                new FileService().CopyFile(filePath, persistentPath);
            }
            else
            {
                new FileService().CopyFile(persistentPath, filePath);
            }
        }

        [MenuItem("Terra/Export Scene Data")]
        public static void ExportSceneData()
        {
            var sceneDataHandler = new SceneDataHandler
            {
                TryGetAssetPath = GetAssetPath
            };
            sceneDataHandler.Save();
            var persistentPath = FileService.GetSavedFilePath();
            var newPath = EditorUtility.SaveFilePanelInProject("Select Save Path", "SaveFile.json", "json", "Please enter a file name to save it in the project");
            if (!string.IsNullOrEmpty(newPath))
            {
                new FileService().CopyFile(persistentPath, newPath);
                AssetDatabase.Refresh();
            }
        }

        [MenuItem("Terra/Import Scene Data")]
        public static void ImportSceneData()
        {
            var asset = AssetDatabase.LoadAssetAtPath<SystemConfigurationSO>(SOPATH);
            var fileRef = asset.SceneDataToLoad;
            var sceneDataHandler = new SceneDataHandler
            {
                TryGetAssetPath = GetAssetPath
            };
            sceneDataHandler.RecreateScene(fileRef.text);
        }

        private static string GetAssetPath(GameObject go)
        {
            if (PrefabUtility.IsPartOfAnyPrefab(go))
            {
                var prefabAbsPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(go);
                var resourcePath = ResourceDB.GetAssetResourcePath(prefabAbsPath);
                return resourcePath;
            }
            return null;
        }
    }
}
