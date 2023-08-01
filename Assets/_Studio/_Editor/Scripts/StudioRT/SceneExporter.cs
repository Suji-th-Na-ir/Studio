using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using RuntimeInspectorNamespace;
using Terra.Studio.RTEditor;
using UnityEngine.SceneManagement;
using System.IO;

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
            var json = JsonConvert.SerializeObject(worldData);
            // Debug.Log($"Generated scene data: {json}");
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
            string filePath = Application.persistentDataPath + "/scene_data.json";
            if (File.Exists(filePath))
            {
                string jsonData = File.ReadAllText(filePath);

                Debug.Log(jsonData);
                
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
            
            int objIndex = 0;
            foreach (var entity in _data.entities)
            {
                GameObject genObject = null;
                if(entity.primitiveType == "Cube")
                    genObject = GameObject.CreatePrimitive(PrimitiveType.Cube);

                if (genObject != null)
                {
                    genObject.name = "Cube_" + objIndex;
                    objIndex++;
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
                // add inspector state manager 
                _gameObject.AddComponent<InspectorStateManager>();

                if (comp.type == "Terra.Studio.Collectable")
                {
                    Collectible cc = _gameObject.AddComponent<Collectible>();
                    cc.Import(comp);
                }
            }
        }

        private void OnDestroy()
        {
            EditorOp.Unregister(this);
        }
    }
}