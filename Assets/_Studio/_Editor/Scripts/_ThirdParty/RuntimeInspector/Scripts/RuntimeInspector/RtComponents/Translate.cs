using UnityEngine;
using Terra.Studio;
using Newtonsoft.Json;
using PlayShifu.Terra;

namespace RuntimeInspectorNamespace
{
    [EditorDrawComponent("Terra.Studio.Translate"), AliasDrawer("Move")]
    public class Translate : BaseBehaviour
    {
        [AliasDrawer("MoveWhen")]
        public Atom.StartOn StartOn = new();
        public Atom.Translate Type = new();
        public Atom.PlaySfx PlaySFX = new();
        public Atom.PlayVfx PlayVFX = new();

        public override string ComponentName => nameof(Translate);
        protected override bool CanBroadcast => true;
        protected override bool CanListen => true;
        protected override string[] BroadcasterRefs => new string[]
        {
            Type.data.Broadcast
        };
        protected override string[] ListenerRefs => new string[]
        {
            StartOn.data.listenName
        };

        protected override void Awake()
        {
            base.Awake();
            Type.Setup(gameObject, this);
            StartOn.Setup<StartOn>(gameObject, ComponentName, OnListenerUpdated, StartOn.data.startIndex == 4);
            PlaySFX.Setup<Translate>(gameObject);
            PlayVFX.Setup<Translate>(gameObject);
            SetupGhostDescription();
        }

        private void SetupGhostDescription()
        {
            GhostDescription = new()
            {
                OnGhostInteracted = OnGhostDataModified,
                SpawnTRS = GetCurrentOffsetInWorld,
                ToggleGhostMode = () =>
                {
                    EditorOp.Resolve<Recorder>().TrackPosition_ShowGhostOnMultiselect(this, true);
                },
                ShowVisualsOnMultiSelect = true,
                GetLastValue = () => { return Type.data.LastVector3; },
                GetRecentValue = () => { return Type.data.moveBy; },
                OnGhostModeToggled = (state) =>
                {
                    if (state)
                    {
                        SetLastValue();
                    }
                },
                IsGhostInteractedInLastRecord = true
            };
            SetLastValue();
        }

        public override (string type, string data) Export()
        {
            var comp = new TranslateComponent
            {
                translateType = (TranslateType)Type.data.translateType,
                speed = Type.data.speed,
                pauseFor = Type.data.pauseFor,
                repeatFor = Type.data.repeat,
                targetPosition = Type.data.moveBy,
                startPosition = transform.position,
                IsConditionAvailable = true,
                ConditionType = GetStartEvent(),
                ConditionData = GetStartCondition(),
                broadcastAt = Type.data.broadcastAt,
                IsBroadcastable = !string.IsNullOrEmpty(Type.data.Broadcast),
                Broadcast = Type.data.Broadcast,
                canPlaySFX = PlaySFX.data.canPlay,
                canPlayVFX = PlayVFX.data.canPlay,
                sfxName = string.IsNullOrEmpty(PlaySFX.data.clipName) ? null : PlaySFX.data.clipName,
                vfxName = string.IsNullOrEmpty(PlayVFX.data.clipName) ? null : PlayVFX.data.clipName,
                sfxIndex = PlaySFX.data.clipIndex,
                vfxIndex = PlayVFX.data.clipIndex,
                listen = Listen.Always,
            };

            ModifyDataAsPerGiven(ref comp);
            gameObject.TrySetTrigger(false, true);
            string type = EditorOp.Resolve<DataProvider>().GetCovariance(this);
            var data = JsonConvert.SerializeObject(comp, Formatting.Indented);
            return (type, data);
        }

        public string GetStartEvent()
        {
            int index = StartOn.data.startIndex;
            var value = (StartOn)index;
            var eventName = EditorOp.Resolve<DataProvider>().GetEnumValue(value);
            return eventName;
        }


        public string GetStartCondition()
        {
            int index = StartOn.data.startIndex;
            var value = (StartOn)index;
            string inputString = value.ToString();
            if (inputString.ToLower().Contains("listen"))
            {
                return StartOn.data.listenName;
            }
            var data = EditorOp.Resolve<DataProvider>().GetEnumConditionDataValue(value);
            return data;
        }

        public override void Import(EntityBasedComponent cdata)
        {
            var comp = JsonConvert.DeserializeObject<TranslateComponent>(cdata.data);
            PlaySFX.data.canPlay = comp.canPlaySFX;
            PlaySFX.data.clipIndex = comp.sfxIndex;
            PlaySFX.data.clipName = comp.sfxName;
            PlayVFX.data.canPlay = comp.canPlayVFX;
            PlayVFX.data.clipIndex = comp.vfxIndex;
            PlayVFX.data.clipName = comp.vfxName;
            Type.data.translateType = (int)comp.translateType;
            Type.data.speed = comp.speed;
            Type.data.pauseFor = comp.pauseFor;
            Type.data.moveBy = comp.targetPosition;
            Type.data.repeat = comp.repeatFor;
            Type.data.Broadcast = comp.Broadcast;
            Type.data.broadcastAt = comp.broadcastAt;
            Type.data.listenTo = comp.ConditionData;
            Type.data.listen = comp.listen;
            if (EditorOp.Resolve<DataProvider>().TryGetEnum(comp.ConditionType, typeof(StartOn), out object result))
            {
                var res = (StartOn)result;
                if (res == Terra.Studio.StartOn.OnPlayerCollide)
                {
                    if (comp.ConditionData.Equals("Player"))
                    {
                        StartOn.data.startIndex = (int)res;
                    }
                    else
                    {
                        StartOn.data.startIndex = (int)Terra.Studio.StartOn.OnObjectCollide;
                    }
                }
                else
                {
                    StartOn.data.startIndex = (int)(StartOn)result;
                }
                StartOn.data.startName = res.ToString();
            }
            StartOn.data.listenName = comp.ConditionData;
            var listenString = "";
            if (StartOn.data.startIndex == 4)
            {
                listenString = Type.data.listenTo;
            }
            ImportVisualisation(Type.data.Broadcast, listenString);
        }

        private void ModifyDataAsPerGiven(ref TranslateComponent component)
        {
            switch (component.translateType)
            {
                case TranslateType.MoveForever:
                case TranslateType.MoveIncrementallyForever:
                case TranslateType.PingPongForever:
                    component.repeatFor = int.MaxValue;
                    break;
                default:
                    component.repeatFor = Type.data.repeat;
                    break;
            }
        }

        private void Update()
        {
            if ((Input.GetKey(KeyCode.LeftCommand) || Input.GetKey(KeyCode.RightCommand)) &&
                (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) &&
                Input.GetKeyDown(KeyCode.R))
            {
                var selections = EditorOp.Resolve<SelectionHandler>().GetSelectedObjects();
                if (selections.Count > 0 && selections[^1] == gameObject)
                {
                    GhostDescription.ToggleGhostMode?.Invoke();
                }
            }
        }

        private Vector3[] GetCurrentOffsetInWorld()
        {
            var pos = transform.localPosition + Type.data.moveBy;
            if (transform.parent != null)
            {
                pos = transform.TransformPoint(pos);
            }
            return new Vector3[] { pos };
        }

        private void OnGhostDataModified(object data)
        {
            var vector3 = (Vector3)data;
            if (transform.parent != null)
            {
                vector3 = transform.InverseTransformPoint(vector3);
            }
            var delta = vector3 - transform.localPosition;
            if (delta != Type.data.moveBy)
            {
                Type.data.moveBy = delta;
                Type.ForceRefreshData?.Invoke();
            }
            GhostDescription.IsGhostInteractedInLastRecord = true;
        }

        private void SetLastValue()
        {
            if (GhostDescription.IsGhostInteractedInLastRecord)
            {
                Type.data.LastVector3 = Type.data.moveBy;
            }
            GhostDescription.IsGhostInteractedInLastRecord = false;
        }
    }
}
