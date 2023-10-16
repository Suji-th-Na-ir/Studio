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
        [AliasDrawer("Speed")] public float speed;
        [AliasDrawer("Repeat")] public Atom.Repeat repeat = new();
        public Atom.PlaySfx PlaySFX = new();
        public Atom.PlayVfx PlayVFX = new();

        public override string ComponentName => nameof(Translate);
        protected override bool CanBroadcast => true;
        protected override bool CanListen => true;
        protected override string[] BroadcasterRefs => new string[]
        {
            repeat.broadcast
        };
        protected override string[] ListenerRefs => new string[]
        {
            StartOn.data.listenName
        };

        protected override void Awake()
        {
            base.Awake();
            Type.Setup(gameObject, this);
            repeat.Setup(gameObject, this);
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
                GetLastValue = () => { return Type.LastVector3; },
                GetRecentValue = () => { return Type.recordedVector3.Get(); },
                OnGhostModeToggled = (state) =>
                {
                    if (state)
                    {
                        SetLastValue();
                    }
                },
                IsGhostInteractedInLastRecord = true,
                GhostTo = gameObject
            };
            SetLastValue();
        }

        public override (string type, string data) Export()
        {
            var comp = new TranslateComponent
            {
                translateType = (RepeatDirectionType)repeat.repeatType,
                speed = speed,
                pauseFor = (repeat.repeat <= 1 && !repeat.repeatForever) ? 0 : repeat.pauseFor,
                repeatFor = repeat.repeatForever ? int.MaxValue : repeat.repeat,
                targetPosition = (Vector3)Type.recordedVector3.Get(),
                startPosition = transform.position,
                IsConditionAvailable = true,
                ConditionType = GetStartEvent(),
                ConditionData = GetStartCondition(),
                broadcastAt = repeat.broadcastAt,
                IsBroadcastable = !string.IsNullOrEmpty(repeat.broadcast),
                Broadcast = repeat.broadcast,
                canPlaySFX = PlaySFX.data.canPlay,
                canPlayVFX = PlayVFX.data.canPlay,
                sfxName = string.IsNullOrEmpty(PlaySFX.data.clipName) ? null : PlaySFX.data.clipName,
                vfxName = string.IsNullOrEmpty(PlayVFX.data.clipName) ? null : PlayVFX.data.clipName,
                sfxIndex = PlaySFX.data.clipIndex,
                vfxIndex = PlayVFX.data.clipIndex,
                listen = Listen.Always,
            };

            //ModifyDataAsPerGiven(ref comp);
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

        public override void OnBroadcastStringUpdated(string newString, string oldString)
        {
            if (repeat.broadcastAt != BroadcastAt.Never)
            {
                EditorOp.Resolve<UILogicDisplayProcessor>().UpdateBroadcastString(newString, oldString, DisplayDock);
            }
            else if (repeat.broadcastAt == BroadcastAt.Never && newString == string.Empty)
            {
                EditorOp.Resolve<UILogicDisplayProcessor>().UpdateBroadcastString(newString, oldString, DisplayDock);
            }
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
            repeat.repeatType = comp.translateType;
            speed = comp.speed;
            repeat.pauseFor = comp.pauseFor;
            repeat.repeatForever = comp.repeatFor == int.MaxValue;
            Type.recordedVector3.Set(comp.targetPosition);
            repeat.repeat = comp.repeatFor == int.MaxValue? 1 :comp.repeatFor;
            repeat.broadcast = comp.Broadcast;
            repeat.broadcastAt = comp.broadcastAt;
            StartOn.data.listenName = comp.ConditionData;
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
                listenString = StartOn.data.listenName;
            }
            ImportVisualisation(repeat.broadcast, listenString);
        }

        private Vector3[] GetCurrentOffsetInWorld()
        {
            var pos = transform.position;
            var localOffset = (Vector3)Type.recordedVector3.Get();
            if (transform.parent != null)
            {
                localOffset = transform.TransformVector(localOffset);
            }
            pos += localOffset;
            return new Vector3[] { pos };
        }

        private void OnGhostDataModified(object data)
        {
            var vector3 = (Vector3)data;
            var delta = vector3 - transform.position;
            if (transform.parent != null)
            {
                delta = transform.InverseTransformVector(delta);
            }
            if (delta != (Vector3)Type.recordedVector3.Get())
            {
                Type.recordedVector3.Set(delta);
            }
            GhostDescription.IsGhostInteractedInLastRecord = true;
        }

        private void SetLastValue()
        {
            if (GhostDescription.IsGhostInteractedInLastRecord)
            {
                Type.LastVector3 = (Vector3)Type.recordedVector3.Get();
            }
            GhostDescription.IsGhostInteractedInLastRecord = false;
        }
    }
}
