using UnityEngine;
using System.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Terra.Studio
{
    [EditorDrawComponent("Terra.Studio.InstantiateStudioObject"), AliasDrawer("Instantiate")]
    public class InstantiateStudioObject : BaseBehaviour
    {
        public Atom.InstantiateOnData instantiateData = new();
        [SerializeField] private Atom.PlaySfx playSFX = new();
        [SerializeField] private Atom.PlayVfx playVFX = new();
        [SerializeField] private Atom.Broadcast broadcast = new();

        public override string ComponentName => nameof(InstantiateStudioObject);
        public override bool CanPreview => true;
        protected override bool CanBroadcast => false;
        protected override bool CanListen => true;
        protected override string[] BroadcasterRefs => new string[]
        {
            broadcast.broadcast
        };
        protected override string[] ListenerRefs => new string[]
        {
            instantiateData.spawnWhen.data.listenName
        };

        protected override void Awake()
        {
            base.Awake();
            instantiateData.Setup(gameObject, this);
            playSFX.Setup<InstantiateStudioObject>(gameObject);
            playVFX.Setup<InstantiateStudioObject>(gameObject);
            broadcast.Setup(gameObject, this);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            instantiateData.OnRecordToggled += ToggleRecorder;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            instantiateData.OnRecordToggled -= ToggleRecorder;
        }

        private void ToggleRecorder(bool status)
        {
            EditorOp.Resolve<Recorder>().ToggleInstantiateRecorder(status, instantiateData.trs, instantiateData.UpdateTRS);
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
            var trsData = new InstantiateStudioObjectComponent.TRS()
            {
                Position = instantiateData.trs[0],
                Rotation = instantiateData.trs[1],
                Scale = instantiateData.trs[2],
            };
            var comp = new InstantiateStudioObjectComponent()
            {
                IsConditionAvailable = true,
                ConditionType = EditorOp.Resolve<DataProvider>().GetEnumValue(instantiateData.instantiateOn),
                ConditionData = GetCondition(),
                canPlaySFX = false,
                canPlayVFX = false,
                childrenEntities = virtualEntities,
                componentsOnSelf = components,
                instantiateOn = instantiateData.instantiateOn,
                spawnWhere = instantiateData.spawnWhere,
                rounds = instantiateData.rounds,
                canRepeatForver = instantiateData.repeatForever,
                duplicatesToSpawn = instantiateData.howMany,
                IsBroadcastable = !string.IsNullOrEmpty(broadcast.broadcast),
                Broadcast = broadcast.broadcast,
                trs = trsData
            };
            var type = EditorOp.Resolve<DataProvider>().GetCovariance(this);
            var data = JsonConvert.SerializeObject(comp);
            Debug.Log($"Type: {type}");
            Debug.Log($"Data: {data}");
            return (type, data);
        }

        private string GetCondition()
        {
            return instantiateData.instantiateOn switch
            {
                InstantiateOn.BroadcastListen => instantiateData.spawnWhen.data.listenName,
                InstantiateOn.EveryXSeconds => instantiateData.interval.ToString(),
                _ => "Start",
            };
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
            var properties = new Dictionary<string, object>[1];
            var instantiateOn = (InstantiateOn)instantiateData.spawnWhen.data.startIndex;
            var startName = instantiateOn.GetStringValue();
            if (instantiateOn == InstantiateOn.EveryXSeconds)
            {
                startName = startName.Replace("x", $"{instantiateData.interval}");
            }
            properties[0] = new()
            {
                { "Spawn Where", instantiateData.spawnWhere.ToString() },
                { "Spawn When", startName }
            };
            if (instantiateOn == InstantiateOn.EveryXSeconds)
            {
                if (instantiateData.repeatForever)
                {
                    properties[0].Add("Rounds", "Repeat Forever");
                }
                else
                {
                    properties[0].Add("Rounds", $"{instantiateData.rounds}");
                }
            }
            if (playSFX.data.canPlay)
            {
                properties[0].Add(BehaviourPreview.Constants.SFX_PREVIEW_NAME, playSFX.data.clipName);
            }
            if (playVFX.data.canPlay)
            {
                properties[0].Add(BehaviourPreview.Constants.VFX_PREVIEW_NAME, playVFX.data.clipName);
            }
            var previewData = new BehaviourPreviewUI.PreviewData()
            {
                DisplayName = GetDisplayName(),
                EventName = startName,
                Properties = properties,
                Broadcast = new string[] { broadcast.broadcast },
                Listen = instantiateData.spawnWhen.data.listenName
            };
            return previewData;
        }
    }
}