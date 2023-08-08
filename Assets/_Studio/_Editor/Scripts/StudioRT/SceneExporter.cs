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
    public class SceneExporter : MonoBehaviour
    {
        private void Awake()
        {
            EditorOp.Register(this);
        }
        
        public void Init()
        {
            // Debug.Log("selection handler ");
            LoadScene();
        }

        private List<GameObject> GetAllSceneGameObjects()
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

        public string ExportJson()
        {
            var sceneObjects = GetAllSceneGameObjects();
            
            var entities = new List<VirtualEntity>();
            for (int i = 0; i < sceneObjects.Count; i++)
            {
                var virutalEntity = new VirtualEntity
                {
                    id = i,
                    primitiveType = sceneObjects[i].GetComponent<MeshFilter>().mesh.name.Split(' ')[0],
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

        private void SaveScene(string data)
        {
            string filePath = Application.persistentDataPath + "/scene_data.json";
            File.WriteAllText(filePath, data);
        }

        private void LoadScene()
        {
#if UNITY_EDITOR
            if (!SystemOp.Resolve<System>().ConfigSO.PickupSavedData && !EditorPrefs.GetBool("InPlayMode",false))
            {
                return;
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

        private void ReCreateScene(WorldData _data)
        {
            List<GameObject> sceneObjects = GetAllSceneGameObjects();
            foreach (var gObject in sceneObjects)
            {
                Destroy(gObject);
            }

            for (int i =0; i< _data.entities.Length; i++)
            {
                var entity = _data.entities[i];
                GameObject genObject = RuntimeWrappers.SpawnPrimitive(entity.primitiveType);

                // if (entity.primitiveType == PrimitiveType.Cube.ToString())
                //     genObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                // else
                //     genObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                             
                
                if (genObject != null)
                {
                    genObject.name = "Cube_" + i;
                    genObject.transform.position = entity.position;
                    genObject.transform.rotation = Quaternion.Euler(
                        entity.rotation.x, entity.rotation.y,entity.rotation.z);
                    genObject.transform.localScale = entity.scale;
                    
                    if(entity.components.Length > 0)
                        AddComponents(entity, genObject);
                }
                else
                {
                    Debug.Log("primitive type not supported");
                }
            }
        }

        private void AddComponents(VirtualEntity _entity, GameObject _gameObject)
        {
            foreach (EntityBasedComponent comp in _entity.components)
            {
                Type type = EditorOp.Resolve<DataProvider>().GetVariance(comp.type);
                var component = _gameObject.AddComponent(type) as IComponent;
                component.Import(comp);
            }
        }

        private void OnDestroy()
        {
            EditorOp.Unregister(this);
        }
    }
}