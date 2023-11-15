using UnityEngine;
using System.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Terra.Studio
{
    [EditorDrawComponent("Terra.Studio.InstantiateStudioObject"), AliasDrawer("Instantiate")]
    public class InstantiateStudioObject : BaseBehaviour
    {
        public override string ComponentName => nameof(InstantiateStudioObject);
        public override bool CanPreview => true;
        protected override bool CanBroadcast => false;
        protected override bool CanListen => true;
        //use
        [SerializeField] private Atom.InstantiateOnData data = new();
        //Do not use
        //[SerializeField] private Atom.StartOn spawnWhen = new();
        //[SerializeField] private SpawnWhere spawnWhere;
        //[SerializeField] private uint rounds;
        //[SerializeField] private bool repeatForever;
        //[SerializeField] private uint howMany;
        [SerializeField] private Atom.PlaySfx playSFX = new();
        [SerializeField] private Atom.PlayVfx playVFX = new();
        //[SerializeField] private bool canBroadcast;
        //[SerializeField] private string broadcast;
        //[SerializeField] private string condition;

        protected override void Awake()
        {
            base.Awake();
            data.Setup(gameObject, this);
            //spawnWhen.Setup<InstantiateOn>(gameObject, ComponentName, OnListenerUpdated, spawnWhen.data.startIndex == 1);
            playSFX.Setup<InstantiateStudioObject>(gameObject);
            playVFX.Setup<InstantiateStudioObject>(gameObject);
        }

        public override (string type, string data) Export()
        {
            var virtualEntities = new VirtualEntity[transform.childCount];
            for (int i = 0; i < virtualEntities.Length; i++)
            {
                virtualEntities[i] = EditorOp.Resolve<SceneDataHandler>().GetVirtualEntity(transform.GetChild(0).gameObject, i, true);
                virtualEntities[i].shouldLoadAssetAtRuntime = false;
            }
            var components = new EntityBasedComponent[0];
            var attachedBehaviours = GetComponents<BaseBehaviour>();
            if (attachedBehaviours != null && attachedBehaviours.Length > 0)
            {
                components = attachedBehaviours.
                Where(x => x.ComponentName != ComponentName).
                Select(y =>
                {
                    var export = y.Export();
                    return new EntityBasedComponent()
                    {
                        type = export.type,
                        data = export.data
                    };
                }).
                ToArray();
            }
            var comp = new InstantiateStudioObjectComponent()
            {
                //IsConditionAvailable = true,
                ////ConditionType = EditorOp.Resolve<DataProvider>().GetEnumValue(spawnWhen),
                //ConditionData = condition,
                //canPlaySFX = false,
                //canPlayVFX = false,
                //childrenEntities = virtualEntities,
                //componentsOnSelf = components,
                ////instantiateOn = spawnWhen,
                //spawnWhere = spawnWhere,
                //rounds = rounds,
                //canRepeatForver = repeatForever,
                //duplicatesToSpawn = howMany,
                //IsBroadcastable = canBroadcast,
                //Broadcast = broadcast
            };
            var type = EditorOp.Resolve<DataProvider>().GetCovariance(this);
            var data = JsonConvert.SerializeObject(comp);
            return (type, data);
        }

        public override void Import(EntityBasedComponent data)
        {
            var component = JsonConvert.DeserializeObject<InstantiateStudioObjectComponent>(data.data);
            EditorOp.Resolve<SceneDataHandler>().OnSceneSetupDone += () =>
            {
                ImportPostSceneSetup(component);
            };
        }

        private void ImportPostSceneSetup(InstantiateStudioObjectComponent component)
        {
            var componentsOnSelf = component.componentsOnSelf;
            if (componentsOnSelf != null && componentsOnSelf.Length > 0)
            {
                EditorOp.Resolve<SceneDataHandler>().AttachComponents(gameObject, component.componentsOnSelf);
            }
            if (component.childrenEntities != null && component.childrenEntities.Length > 0)
            {
                EditorOp.Resolve<SceneDataHandler>().HandleChildren(gameObject, component.childrenEntities);
            }
        }

        public override BehaviourPreviewUI.PreviewData GetPreviewData()
        {
            //var properties = new Dictionary<string, object>[1];
            //properties[0] = new()
            //{
            //    { "Spawn Where", spawnWhere.ToString() },
            //    { "Condition", condition }
            //};
            //if (playSFX.data.canPlay)
            //{
            //    properties[0].Add(BehaviourPreview.Constants.SFX_PREVIEW_NAME, playSFX.data.clipName);
            //}
            //if (playVFX.data.canPlay)
            //{
            //    properties[0].Add(BehaviourPreview.Constants.VFX_PREVIEW_NAME, playVFX.data.clipName);
            //}
            //var startOnName = spawnWhen.ToString();
            //var previewData = new BehaviourPreviewUI.PreviewData()
            //{
            //    DisplayName = GetDisplayName(),
            //    EventName = startOnName,
            //    Properties = properties,
            //    Broadcast = new string[] { broadcast },
            //    Listen = condition
            //};
            //return previewData;
            return base.GetPreviewData();
        }
    }
}