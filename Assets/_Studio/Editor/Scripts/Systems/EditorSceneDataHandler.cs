using System.IO;
using UnityEditor;
using UnityEngine;
using Newtonsoft.Json;

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
            if (!asset.PickupSavedData || asset.LoadFromCloud)
            {
                return;
            }
            var fileRef = asset.SceneDataToLoad;
            var filePath = AssetDatabase.GetAssetPath(fileRef);
            var persistentPath = FileService.GetSavedFilePath(filePath);
            if (didEnterPlayMode)
            {
                new FileService().CopyFile(filePath, persistentPath);
            }
            else
            {
                new FileService().CopyFile(persistentPath, filePath);
                AssetDatabase.Refresh();
            }
        }

        private static void SyncMetaData(string actualFilePath, string targetFilePath)
        {
            if (!File.Exists(actualFilePath) || !File.Exists(targetFilePath))
            {
                return;
            }
            var actualFileData = File.ReadAllText(actualFilePath);
            var actualObj = JsonConvert.DeserializeObject<WorldData>(actualFileData);
            var targetFileData = File.ReadAllText(targetFilePath);
            var targetObj = JsonConvert.DeserializeObject<WorldData>(targetFileData);
            targetObj.metaData = actualObj.metaData;
            var newTargetFileData = JsonConvert.SerializeObject(targetObj);
            File.WriteAllText(targetFilePath, newTargetFileData);
        }

        [MenuItem("Terra/Export Scene Data")]
        public static void ExportSceneData()
        {
            var sceneDataHandler = new SceneDataHandler
            {
                TryGetAssetPath = GetAssetPath,
                GetAssetName = GetAssetName
            };
            sceneDataHandler.Save();
            var persistentPath = FileService.GetSavedFilePath(GetAssetName());
            var newPath = EditorUtility.SaveFilePanelInProject("Select Save Path", $"{GetAssetName()}.json", "json", "Please enter a file name to save it in the project", "Assets/_Studio/Scenes/Templates/");
            if (!string.IsNullOrEmpty(newPath))
            {
                SyncMetaData(newPath, persistentPath);
                new FileService().CopyFile(persistentPath, newPath);
                var resourceObj = Resources.Load<SystemConfigurationSO>("System/SystemSettings");
                AssetDatabase.Refresh();
                resourceObj.PickupSavedData = true;
                resourceObj.SceneDataToLoad = AssetDatabase.LoadAssetAtPath<TextAsset>(newPath);
                EditorUtility.SetDirty(resourceObj);
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
                TryGetAssetPath = GetAssetPath,
                GetAssetName = GetAssetName
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

        private static string GetAssetName()
        {
            var resourceObj = Resources.Load<SystemConfigurationSO>("System/SystemSettings");
            if (resourceObj.SceneDataToLoad == null)
            {
                return "SaveFile";
            }
            return resourceObj.SceneDataToLoad.name;
        }
    }
}
