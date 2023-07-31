using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using RuntimeInspectorNamespace;
using Terra.Studio.RTEditor;

namespace Terra.Studio
{
    public class SceneExporter : MonoBehaviour
    {
        [SerializeField]
        private GameObject[] sceneObjects;

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

        public string ExportJson()
        {
            var entities = new List<VirtualEntity>();
            for (int i = 0; i < sceneObjects.Length; i++)
            {
                var virutalEntity = new VirtualEntity
                {
                    id = i,
                    primitiveType = "Cube",
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
