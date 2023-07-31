using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using RuntimeInspectorNamespace;
using Terra.Studio.RTEditor;
using UnityEngine.SceneManagement;

namespace Terra.Studio
{
    public class SceneExporter : MonoBehaviour
    {
        private List<GameObject> sceneObjects = new List<GameObject>();

        private void Awake()
        {
            Interop<EditorInterop>.Current.Register(this);
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.H))
            {
                Debug.Log(ExportJson());
            }
        }

        private List<GameObject> GetAllSceneGameObjects()
        {
            GameObject[] objects = SceneManager.GetActiveScene().GetRootGameObjects();
            foreach (GameObject obj in objects)
            {
                if (obj.GetComponentInParent<HideInHierarchy>())
                {
                    continue;
                }
                if (!obj.activeSelf)
                {
                    continue;
                }
                sceneObjects.Add(obj);
            }
            return sceneObjects;
        }

        public string ExportJson()
        {
            sceneObjects = GetAllSceneGameObjects();
            
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
            Debug.Log($"Generated scene data: {json}");
            return json;
        }

        private void OnDestroy()
        {
            Interop<EditorInterop>.Current.Unregister(this);
        }
    }
}
