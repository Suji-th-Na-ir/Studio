using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using System.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace Terra.Studio.RTEditor
{
    [CustomEditor(typeof(ResourceDB))]
    public class ResourceDBEditor : Editor
    {
        private int count;
        private int prevCount;
        private bool validate;
        private ResourceDB resourceDB;
        private List<Object> watchFolders;
        public static readonly Type[] SUPPORTED_TYPES = new[] { typeof(GameObject), typeof(AudioClip) };

        private void OnEnable()
        {
            resourceDB = (ResourceDB)target;
            prevCount = resourceDB.watchFolders.Count;
            count = prevCount;
            watchFolders = new();
            for (int i = 0; i < prevCount; i++)
            {
                var asset = AssetDatabase.LoadAssetAtPath<DefaultAsset>(resourceDB.watchFolders[i]);
                watchFolders.Add(asset);
            }
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            GUILayout.Space(10);
            EditorGUILayout.LabelField("Watch Folders", EditorStyles.boldLabel);
            count = EditorGUILayout.IntField(new GUIContent("Count"), count);
            if (prevCount != count)
            {
                AdjustWatchFolderList();
                prevCount = count;
            }
            GUILayout.Space(5);
            for (int i = 0; i < count; i++)
            {
                var cachedFolder = watchFolders[i];
                watchFolders[i] = EditorGUILayout.ObjectField(new GUIContent($"Folder {i + 1}"), watchFolders[i], typeof(DefaultAsset), false);
                if (watchFolders[i] != null && cachedFolder != watchFolders[i])
                {
                    var assetPath = AssetDatabase.GetAssetPath(watchFolders[i]);
                    if (!assetPath.Contains("/Resources/"))
                    {
                        watchFolders[i] = cachedFolder;
                        EditorUtility.DisplayDialog("ResourceDB", "The folder path is not under resources!", "Ok");
                    }
                }
            }
            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Clear Saved Data"))
            {
                ClearSavedData();
            }
            if (GUILayout.Button("Force Update"))
            {
                ForceUpdate();
                EditorUtility.SetDirty(target);
            }
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(10);
            EditorGUILayout.HelpBox("Remember to force update whenever asset renamed or moved!", MessageType.Info);
            GUILayout.Space(20);
            if (resourceDB.ItemsData.Count > 1)
            {
                EditorGUILayout.BeginVertical("GroupBox");
                EditorGUILayout.LabelField("Recorded Resource Items", EditorStyles.boldLabel);
                for (int i = 0; i < SUPPORTED_TYPES.Length; i++)
                {
                    GUILayout.Space(5);
                    var label = i == 0 ? "GameObjects" : "Audio Clips";
                    EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
                    for (int j = 0; j < resourceDB.ItemsData.Count; j++)
                    {
                        if (!resourceDB.ItemsData[j].Type.Equals(SUPPORTED_TYPES[i].AssemblyQualifiedName))
                        {
                            continue;
                        }
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField($"{j + 1}. {resourceDB.ItemsData[j].Name}");
                        GUILayout.Space(10);
                        if (GUILayout.Button("Ping"))
                        {
                            var typeData = resourceDB.ItemsData[j].Type;
                            var type = Type.GetType(typeData);
                            var asset = AssetDatabase.LoadAssetAtPath(resourceDB.ItemsData[j].AbsolutePath, type);
                            EditorGUIUtility.PingObject(asset);
                        }
                        if (GUILayout.Button("Copy Data"))
                        {
                            var json = JsonConvert.SerializeObject(resourceDB.ItemsData[j]);
                            GUIUtility.systemCopyBuffer = json;
                        }
                        EditorGUILayout.EndHorizontal();
                        if (validate)
                        {
                            var type = Type.GetType(resourceDB.ItemsData[j].Type);
                            var asset = AssetDatabase.LoadAssetAtPath(resourceDB.ItemsData[j].AbsolutePath, type);
                            if (!asset)
                            {
                                var msg = $"Did not find {resourceDB.ItemsData[j].Name} at {resourceDB.ItemsData[j].AbsolutePath}";
                                Debug.LogError(msg);
                            }
                            else
                            {
                                Debug.Log($"Found: {resourceDB.ItemsData[j].AbsolutePath}");
                            }
                        }
                    }
                }
                GUILayout.Space(5);
                validate = false;
                if (GUILayout.Button("Validate"))
                {
                    validate = true;
                }
                EditorGUILayout.EndVertical();
            }
        }

        private void AdjustWatchFolderList()
        {
            if (count == 0)
            {
                watchFolders.Clear();
                return;
            }
            var delta = count - prevCount;
            if (delta < 0)
            {
                for (int i = 0; i < delta * -1; i++)
                {
                    watchFolders.RemoveAt(resourceDB.watchFolders.Count - 1);
                }
            }
            else
            {
                Object data = null;
                if (watchFolders.Count > 0)
                {
                    data = watchFolders[^1];
                }
                for (int i = 0; i < delta; i++)
                {
                    watchFolders.Add(data);
                }
            }
        }

        private void ForceUpdate()
        {
            UpdateAssetPathIntoSO();
            for (int i = 0; i < resourceDB.watchFolders.Count; i++)
            {
                for (int k = 0; k < SUPPORTED_TYPES.Length; k++)
                {
                    var folderName = GetTrimmedName(resourceDB.watchFolders[i]);
                    var allItems = Resources.LoadAll(folderName, SUPPORTED_TYPES[k]);
                    for (int j = 0; j < allItems.Length; j++)
                    {
                        InjectAssetData(allItems[j], SUPPORTED_TYPES[k]);
                    }
                }
            }
        }

        public void InjectAssetData(Object obj, Type type)
        {
            var item = obj;
            var fullPath = AssetDatabase.GetAssetPath(item);
            var doesContain = resourceDB.ItemsData.Any(x => x.AbsolutePath.Equals(fullPath));
            if (!doesContain)
            {
                var resourcePath = GetTrimmedName(fullPath);
                var guid = AssetDatabase.GUIDFromAssetPath(fullPath);
                var resourceItemData = new ResourceDB.ResourceItemData(item.name, fullPath, resourcePath, type.AssemblyQualifiedName, guid.ToString());
                resourceDB.ItemsData.Add(resourceItemData);
            }
        }

        public static string GetTrimmedName(string fullPath)
        {
            var splits = fullPath.Split('/');
            var newPath = string.Empty;
            for (int i = Array.IndexOf(splits, "Resources") + 1; i < splits.Length; i++)
            {
                var embedData = splits[i];
                if (splits[i].Contains('.'))
                {
                    embedData = splits[i].Split('.')[0];
                }
                if (string.IsNullOrEmpty(newPath))
                {
                    newPath = embedData;
                }
                else
                {
                    newPath = string.Concat(newPath, "/", embedData);
                }
            }
            if (Path.HasExtension(fullPath))
            {
                return newPath;
            }
            else
            {
                return string.Concat(newPath, "/");
            }
        }

        private void ClearSavedData()
        {
            resourceDB.ItemsData.Clear();
        }

        private void UpdateAssetPathIntoSO()
        {
            var paths = new string[count];
            for (int i = 0; i < count; i++)
            {
                paths[i] = AssetDatabase.GetAssetPath(watchFolders[i]);
            }
            resourceDB.watchFolders = paths.ToList();
        }

        private void OnDisable()
        {
            UpdateAssetPathIntoSO();
        }

        public class OnAssetImported : AssetPostprocessor
        {
            private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths, bool _)
            {
                var resourceDB = Resources.Load<ResourceDB>("System/ResourceDB");
                foreach (var assetPath in deletedAssets)
                {
                    if (resourceDB.watchFolders.Any(x => assetPath.Contains(x)))
                    {
                        var result = resourceDB.ItemsData.Find(x => x.AbsolutePath.Equals(assetPath));
                        if (result != null)
                        {
                            resourceDB.ItemsData.Remove(result);
                        }
                    }
                }
                foreach (var assetPath in importedAssets)
                {
                    if (resourceDB.watchFolders.Any(x => assetPath.Contains(x)))
                    {
                        var assetType = AssetDatabase.GetMainAssetTypeAtPath(assetPath);
                        if (SUPPORTED_TYPES.Any(x => x.AssemblyQualifiedName.Equals(assetType.AssemblyQualifiedName)))
                        {
                            var asset = AssetDatabase.LoadAssetAtPath(assetPath, assetType);
                            var resourcePath = GetTrimmedName(assetPath);
                            var guid = AssetDatabase.GUIDFromAssetPath(assetPath);
                            var resourceItemData = new ResourceDB.ResourceItemData(asset.name, assetPath, resourcePath, assetType.AssemblyQualifiedName, guid.ToString());
                            resourceDB.ItemsData.Add(resourceItemData);
                        }
                    }
                }
                foreach (var assetPath in movedAssets)
                {
                    if (resourceDB.watchFolders.Any(x => assetPath.Contains(x)))
                    {
                        var result = resourceDB.ItemsData.Find(x => x.AbsolutePath.Equals(assetPath));
                        if (result != null)
                        {
                            resourceDB.ItemsData.Remove(result);
                        }
                    }
                }
                foreach (var assetPath in movedFromAssetPaths)
                {
                    if (resourceDB.watchFolders.Any(x => assetPath.Contains(x)))
                    {
                        var assetType = AssetDatabase.GetMainAssetTypeAtPath(assetPath);
                        if (assetType == null)
                        {
                            continue;
                        }
                        if (SUPPORTED_TYPES.Any(x => x.AssemblyQualifiedName.Equals(assetType.AssemblyQualifiedName)))
                        {
                            var asset = AssetDatabase.LoadAssetAtPath(assetPath, assetType);
                            var resourcePath = GetTrimmedName(assetPath);
                            var guid = AssetDatabase.GUIDFromAssetPath(assetPath);
                            var resourceItemData = new ResourceDB.ResourceItemData(asset.name, assetPath, resourcePath, assetType.AssemblyQualifiedName, guid.ToString());
                            resourceDB.ItemsData.Add(resourceItemData);
                        }
                    }
                }
            }
        }
    }
}
