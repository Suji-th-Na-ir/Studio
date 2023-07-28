using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using RuntimeInspectorNamespace;
using Terra.Studio.RTEditor;

namespace Terra.Studio
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class EXComponent
    {
        public string type { get; set; }
        public Data data { get; set; }
    }

    public class Data
    {
        public bool CanExecute { get; set; }
        public string ConditionType { get; set; }
        public string ConditionData { get; set; }
        public bool IsConditionAvailable { get; set; }
        public bool IsBroadcastable { get; set; }
        public string Broadcast { get; set; }
        public bool IsTargeted { get; set; }
        public bool IsExecuted { get; set; }
        public int? speed { get; set; }
        public List<float> fromPoint { get; set; }
        public List<float> toPoint { get; set; }
        public bool? loop { get; set; }
        public bool canPlaySFX { get; set; }
        public string sfxName { get; set; }
        public bool canPlayVFX { get; set; }
        public string vfxName { get; set; }
        public bool canUpdateScore { get; set; }
        public float scoreValue { get; set; }
        public bool showScoreUI { get; set; }
        public OscillateComponent oscillateComponent;
    }

    public class Entity
    {
        public int id { get; set; }
        public string primitiveType { get; set; }
        public List<float> position { get; set; }
        public List<float> rotation { get; set; }
        public List<float> scale { get; set; }
        public List<EXComponent> components { get; set; }
    }

    public class Root
    {
        public List<Entity> entities { get; set; }
    }
    public class SceneExporter : MonoBehaviour
    {
        [SerializeField] private GameObject[] sceneObjects;
        
        // Start is called before the first frame update
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.H))
            {
                Debug.Log(ExportJson());
            }
        }

        public string ExportJson()
        {
            // Debug.Log("export json");
            Root rootObj = new Root();
            rootObj.entities = new List<Entity>();

            for (int i =0;i<sceneObjects.Length;i++)
            {
                GameObject go = sceneObjects[i];
                Entity ent = new Entity();
                ent.id = i;
                ent.primitiveType = "Cube";
                ent.position = new List<float>()
                {
                    go.transform.position.x,
                    go.transform.position.y,
                    go.transform.position.z
                };
                ent.rotation = new List<float>()
                {
                    go.transform.rotation.eulerAngles.x,
                    go.transform.rotation.eulerAngles.y,
                    go.transform.rotation.eulerAngles.z
                };
                ent.scale = new List<float>()
                {
                    go.transform.localScale.x,
                    go.transform.localScale.y,
                    go.transform.localScale.z
                };
                ent.components = new List<EXComponent>();
                Collectible collectible = go.GetComponent<Collectible>();
                InspectorStateManager state = go.GetComponent<InspectorStateManager>();
                if (collectible != null)
                {
                    EXComponent component = new EXComponent();
                    component.type = "Terra.Studio.Collectable";
                    component.data = new Data();
                    component.data.IsConditionAvailable = true;
                    component.data.ConditionType = collectible.GetStartEvent();
                    component.data.ConditionData = collectible.GetStartCondition();
                    component.data.IsBroadcastable = collectible.Broadcast != Atom.BroadCast.None;
                    if (component.data.IsBroadcastable)
                        component.data.Broadcast = collectible.Broadcast.ToString();
                    
                    component.data.canPlaySFX = state.GetItem<bool>("sfx_toggle");
                    int sfxIndex = state.GetItem<int>("sfx_dropdown");
                    component.data.sfxName = PlaySFXField.GetSfxClipName(sfxIndex);
                    
                    component.data.canPlayVFX = state.GetItem<bool>("vfx_toggle");
                    int vfxIndex = state.GetItem<int>("vfx_dropdown");
                    component.data.vfxName = PlayVFXField.GetVfxClipName(vfxIndex);

                    component.data.canUpdateScore = collectible.ShowScoreUI;
                    component.data.scoreValue = collectible.ScoreValue;
                
                    ent.components.Add(component);
                }

                DestroyOn dest = go.GetComponent<DestroyOn>();
                if (dest != null)
                {
                    EXComponent component = new EXComponent();
                    if (component.data == null) component.data = new Data();
                    component.type = "Terra.Studio.DestroyOn";
                    // component.data = new Data();
                    component.data.IsConditionAvailable = true;
                    component.data.ConditionType = dest.GetStartEvent();
                    component.data.ConditionData = dest.GetStartCondition();
                    component.data.IsBroadcastable = dest.Broadcast != Atom.BroadCast.None;
                    
                    if (component.data.IsBroadcastable)
                        component.data.Broadcast = dest.Broadcast.ToString();
                    
                    component.data.canPlaySFX = state.GetItem<bool>("sfx_toggle");
                    int sfxIndex = state.GetItem<int>("sfx_dropdown");
                    component.data.sfxName = PlaySFXField.GetSfxClipName(sfxIndex);
                    
                    component.data.canPlayVFX = state.GetItem<bool>("vfx_toggle");
                    int vfxIndex = state.GetItem<int>("vfx_dropdown");
                    component.data.vfxName = PlayVFXField.GetVfxClipName(vfxIndex);

                    component.data.canUpdateScore = collectible.ShowScoreUI;
                    component.data.scoreValue = collectible.ScoreValue;
                    
                    ent.components.Add(component);
                }
                
                Oscillate osc = go.GetComponent<Oscillate>();
                if (osc != null)
                {
                    EXComponent component = new EXComponent();
                    if (component.data == null) component.data = new Data();
                    component.data.oscillateComponent = osc.Component;
                    ent.components.Add(component);
                }
                rootObj.entities.Add(ent);
            }
            return JsonConvert.SerializeObject(rootObj);
        }
    }
}
