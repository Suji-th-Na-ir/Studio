using System.IO;
using UnityEditor;
using UnityEngine;
using Newtonsoft.Json;
using System.Collections.Generic;
using PlayShifu.Terra;

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
            if (!asset.PickupSavedData || asset.ServeFromCloud)
            {
                return;
            }
            var fileRef = asset.SceneDataToLoad;
            var filePath = AssetDatabase.GetAssetPath(fileRef);
            string persistentPath = FileService.GetSavedFilePath(filePath);
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
            return FileService.GetSavedFilePath(resourceObj.SceneDataToLoad.name);
        }

        [MenuItem("Terra/Miscellaneous/Move All Prefabs To Resources")]
        public static void MoveAllPrefabsToScenePrefabFolder()
        {
            var dest = "Assets/_Studio/Resources/Common/ScenePrefabs/";
            var selection = Selection.activeGameObject;
            var paths = new List<string>();
            for (int i = 0; i < selection.transform.childCount; i++)
            {
                var child = selection.transform.GetChild(i).gameObject;
                var path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(child);
                if (!string.IsNullOrEmpty(path) && !paths.Contains(path) && !path.Contains(dest))
                {
                    paths.Add(path);
                }
                if (child.transform.childCount > 0)
                {
                    var childrenPaths = GetPrefabPathsFromChild(child.transform);
                    foreach (var childPath in childrenPaths)
                    {
                        if (!paths.Contains(childPath) && !childPath.Contains(dest))
                        {
                            paths.Add(childPath);
                        }
                    }
                }
            }
            foreach (var path in paths)
            {
                var assetName = Path.GetFileName(path);
                var newPath = Path.Combine(dest, assetName);
                AssetDatabase.MoveAsset(path, newPath);
                Debug.Log($"Moved {path} to {newPath}");
            }
            AssetDatabase.Refresh();
        }

        private static List<string> GetPrefabPathsFromChild(Transform child)
        {
            var paths = new List<string>();
            for (int i = 0; i < child.childCount; i++)
            {
                var furtherChild = child.GetChild(i).gameObject;
                var path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(furtherChild);
                if (!paths.Contains(path)) paths.Add(path);
                if (furtherChild.transform.childCount > 0)
                {
                    for (int j = 0; j < furtherChild.transform.childCount; j++)
                    {
                        var childrenPaths = GetPrefabPathsFromChild(furtherChild.transform.GetChild(j));
                        foreach (var childPath in childrenPaths)
                        {
                            if (!paths.Contains(childPath)) paths.Add(childPath);
                        }
                    }
                }
            }
            return paths;
        }
    }
}
