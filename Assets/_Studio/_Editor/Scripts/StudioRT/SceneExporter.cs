using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using RuntimeInspectorNamespace;
using Terra.Studio.RTEditor;
using UnityEngine.SceneManagement;
using System.IO;
using UnityEditor;
namespace Terra.Studio
{
    public static class SceneExporter 
    {
        [MenuItem("Terra/Export Scene")]
        public static void ExportSceneFromHirearchy()
        {
           //ResourceDB.TriggerUpdate();
            SceneExporter.ExportJson();
        }

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
            GameObject[] objects = GameObject.FindObjectsOfType<GameObject>();


            foreach (var obj in objects)
            {
                if (obj.activeInHierarchy && obj.activeSelf)
                {
                    newList.Add(obj);
                }
            }

            return newList;
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
                var sceneObjectName = sceneObjects[i].name.Replace("(Clone)","");
                if (ResourceDB.GetAsset(sceneObjectName) == null)
                    continue;
                var virutalEntity = new VirtualEntity
                {
                    id = i,
                    name = sceneObjectName,
                    assetPath = Path.Combine(ResourceDB.GetAsset(sceneObjectName).Path, sceneObjectName),
                    
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
            string filePath = Application.persistentDataPath + "/scene_data.json";
            File.WriteAllText(filePath, data);
        }

        private static void LoadScene()
        {

#if UNITY_EDITOR
            if (SystemOp.Resolve<System>().PreviousStudioState == StudioState.Bootstrap)
            {
                if (!SystemOp.Resolve<System>().ConfigSO.PickupSavedData)
                {
                    return;
                }
            }
#endif
            string filePath = Application.persistentDataPath + "/scene_data.json";
            if (File.Exists(filePath))
            {
                string jsonData = File.ReadAllText(filePath);

                Debug.Log("Loading scene data "+jsonData);
                
                WorldData wData = JsonConvert.DeserializeObject<WorldData>(jsonData);
                
                ReCreateScene(wData);
            }
            else
            {
                Debug.Log("save file do not exists.");
            }
        }

        private static void ReCreateScene(WorldData _data)
        {
            List<GameObject> sceneObjects = GetAllSceneGameObjects();
            foreach (var gObject in sceneObjects)
            {
                UnityEngine.Object.Destroy(gObject);
            }

            for (int i =0; i< _data.entities.Length; i++)
            {
                var entity = _data.entities[i];
                Vector3[] trs = new Vector3[3];
                trs[0] = entity.position;
                trs[1] = entity.rotation;
                trs[2] = entity.scale;
                GameObject genObject = RuntimeWrappers.SpawnGameObject(entity.assetPath,trs);

                if (genObject != null)
                {
                    genObject.name = _data.entities[i].name;
                    if (entity.components.Length > 0)
                        AddComponents(entity, genObject);
                }
                else
                {
                    Debug.Log("primitive type not supported");
                }
            }
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

    }
}