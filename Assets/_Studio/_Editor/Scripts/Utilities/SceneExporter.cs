using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using RuntimeInspectorNamespace;
using UnityEngine.SceneManagement;
using System.IO;
using UnityEditor;
using System.Text.RegularExpressions;
using PlayShifu.Terra;

namespace Terra.Studio
{
    public static class SceneExporter
    {
        static string filePath;
#if UNITY_EDITOR
        [MenuItem("Terra/Export Scene")]
        public static void ExportSceneFromHirearchy()
        {
            //ResourceDB.TriggerUpdate();
            SceneExporter.ExportJson();
        }
#endif
        public static void Init()
        {
            // Debug.Log("selection handler ");
            LoadScene();
        }

        private static List<GameObject> GetAllSceneGameObjects()
        {
            var objects = SceneManager.GetActiveScene().GetRootGameObjects();
            var newList = new List<GameObject>();
            foreach (var obj in objects)
            {
                if (obj.GetComponentInParent<HideInHierarchy>())
                {
                    continue;
                }
                if (!obj.activeSelf)
                {
                    continue;
                }
                newList.Add(obj);
            }
            return newList;
        }

        private static List<GameObject> GetAllGameObjectsInHirearchy()
        {
            List<GameObject> newList = new List<GameObject>();
            GameObject[] objects = SceneManager.GetActiveScene().GetRootGameObjects();


            foreach (var obj in objects)
            {
                if (obj.GetComponentInParent<HideInHierarchy>())
                {
                    continue;
                }
                if (obj.activeInHierarchy && obj.activeSelf)
                {
                    newList.Add(obj);
                }
            }

            return newList;
        }

        static string RemoveContentInParentheses(string input)
        {
            string pattern = @"\([^)]*\)";
            string result = Regex.Replace(input, pattern, "");
            result = result.Trim(); // Remove trailing spaces
            return result;
        }

        public static void ExportDataToPersistantPath()
        {
            ExportJson();
        }
        public static string ExportJson()
        {
            List<GameObject> sceneObjects;
            if (Application.isPlaying)
            {
                sceneObjects = GetAllSceneGameObjects();
            }
            else
            {
                sceneObjects = GetAllGameObjectsInHirearchy();
            }

            var entities = new List<VirtualEntity>();
            for (int i = 0; i < sceneObjects.Count; i++)
            {
                //if (sceneObjects[i].GetComponent<MeshFilter>() == null)
                //    continue;
                string sceneObjectName = "";
                if (Application.isPlaying)
                {
                    sceneObjectName = RemoveContentInParentheses(sceneObjects[i].name);
                }
                else
                {
#if UNITY_EDITOR
                    sceneObjectName = PrefabUtility.GetCorrespondingObjectFromSource(sceneObjects[i]).name;
#endif
                }
                if (ResourceDB.GetStudioAsset(sceneObjectName) == null)
                    continue;
                var virutalEntity = new VirtualEntity
                {
                    id = i,
                    name = sceneObjectName,
                    assetPath = ResourceDB.GetStudioAsset(sceneObjectName).ShortPath,

                    position = sceneObjects[i].transform.position,
                    rotation = sceneObjects[i].transform.eulerAngles,
                    scale = sceneObjects[i].transform.localScale
                };
                var components = new List<EntityBasedComponent>();
                var attachedComps = sceneObjects[i].GetComponents<IComponent>();
                foreach (var component in attachedComps)
                {
                    var data = component.Export();
                    components.Add(new EntityBasedComponent()
                    {
                        type = data.type,
                        data = data.data
                    });
                }
                virutalEntity.components = components.ToArray();
                int index = 0;
                virutalEntity.childCompenentDictionary = GetNestedChildrenComponentsWithRelativeIds(sceneObjects[i].transform, ref index);
                entities.Add(virutalEntity);
            }
            var worldData = new WorldData()
            {
                entities = entities.ToArray()
            };
            var json = JsonConvert.SerializeObject(worldData, Formatting.Indented);
            Debug.Log($"Generated scene data: {json}");
            SaveScene(json);
            return json;
        }

        private static void SaveScene(string data)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                filePath = Application.dataPath + $"/Resources/{DateTime.Now}.json";
            }
            else
            {

                if (!SystemOp.Resolve<System>().ConfigSO.PickupSavedData)
                {
                    return;
                }
                filePath = Application.dataPath + "/Resources" + ResourceDB.GetStudioAsset(SystemOp.Resolve<System>().ConfigSO.SceneDataToLoad.name).Path + ".json";

            }
            UpdateJsonFile(data);
            AssetDatabase.Refresh();
#else

            filePath = Helper.GetCoreDataSavePath() + $"/{DateTime.Now}.json";
            UpdateJsonFile(data);
#endif

        }

        private static void UpdateJsonFile(string data)
        {
            string directory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            File.WriteAllText(filePath, data);
        }

        private static void LoadScene()
        {
            string jsonData = null;
            if (SystemOp.Resolve<System>().PreviousStudioState == StudioState.Bootstrap)
            {

                if (!SystemOp.Resolve<System>().ConfigSO.PickupSavedData)
                {
                    return;
                }
                else
                {
                    jsonData = SystemOp.Resolve<System>().ConfigSO.SceneDataToLoad.text;
                }

            }
            else
            {
                jsonData = SystemOp.Resolve<CrossSceneDataHolder>().Get();
            }
            if (!string.IsNullOrEmpty(jsonData))
            {
                // Debug.Log("Loading scene data " + jsonData);
                WorldData wData = JsonConvert.DeserializeObject<WorldData>(jsonData);
                ReCreateScene(wData);
            }
        }

        private static void ReCreateScene(WorldData _data)
        {
            List<GameObject> sceneObjects = GetAllSceneGameObjects();
            foreach (var gObject in sceneObjects)
            {
                UnityEngine.Object.Destroy(gObject);
            }

            for (int i = 0; i < _data.entities.Length; i++)
            {
                var entity = _data.entities[i];
                Vector3[] trs = new Vector3[3];
                trs[0] = entity.position;
                trs[1] = entity.rotation;
                trs[2] = entity.scale;
                GameObject genObject = RuntimeWrappers.SpawnGameObject(entity.assetPath, trs);

                if (genObject != null)
                {
                    genObject.name = _data.entities[i].name;
                  
                    if (entity.components.Length > 0)
                        AddComponents(entity, genObject);
                    var children =Helper.GetChildren(genObject.transform, true);
                    for (int j = 0; j < children.Count; j++)
                    {
                        if(entity.childCompenentDictionary.TryGetValue(j, out EntityBasedComponent[] components))
                        {
                            AddComponents(components, children[j].gameObject);
                        }
                    }
                }
                else
                {
                    Debug.Log("GameObject could not be spawned!!");
                }
            }
        }

        private static Dictionary<int, EntityBasedComponent[]> GetNestedChildrenComponentsWithRelativeIds(Transform parent,ref int index)
        {
            /** Get a dictionary hierarchy of all children under a given parent. **/

            Dictionary<int, EntityBasedComponent[]> children = new Dictionary<int, EntityBasedComponent[]>();

            foreach (Transform child in parent)
            {
                var components = new List<EntityBasedComponent>();
                var attachedComps = child.GetComponents<IComponent>();
                foreach (var component in attachedComps)
                {
                    var data = component.Export();
                    components.Add(new EntityBasedComponent()
                    {
                        type = data.type,
                        data = data.data
                    });
                }
                if(components.Count>0)
                children.Add(index, components.ToArray());

                int childIndex = index + 1;
                var childChildren = GetNestedChildrenComponentsWithRelativeIds(child, ref childIndex);
                foreach (var kvp in childChildren)
                {
                    if(kvp.Value.Length>0)
                    children.Add(kvp.Key, kvp.Value);
                }

                index = childIndex;
            }

            return children;
        }


        private static void AddComponents(VirtualEntity _entity, GameObject _gameObject)
        {
            foreach (EntityBasedComponent comp in _entity.components)
            {
                Type type = EditorOp.Resolve<DataProvider>().GetVariance(comp.type);
                var component = _gameObject.AddComponent(type) as IComponent;
                component.Import(comp);
            }
        }

        private static void AddComponents(EntityBasedComponent[] components, GameObject _gameObject)
        {
            foreach (EntityBasedComponent comp in components)
            {
                Type type = EditorOp.Resolve<DataProvider>().GetVariance(comp.type);
                var component = _gameObject.AddComponent(type) as IComponent;
                component.Import(comp);
            }
        }

    }
}