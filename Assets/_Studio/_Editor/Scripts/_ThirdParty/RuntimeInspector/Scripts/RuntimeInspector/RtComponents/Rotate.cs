using UnityEngine;
using Terra.Studio;
using Newtonsoft.Json;
using PlayShifu.Terra;

namespace RuntimeInspectorNamespace
{
    [EditorDrawComponent("Terra.Studio.Rotate")]
    public class Rotate : BaseBehaviour
    {
        [AliasDrawer("RotateWhen")]
        public Atom.StartOn StartOn = new();
        public Atom.Rotate Type = new();
        [AliasDrawer("Repeat")] public Atom.Repeat repeat = new();
        public Atom.PlaySfx PlaySFX = new();
        public Atom.PlayVfx PlayVFX = new();

        public override string ComponentName => nameof(Rotate);
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
            PlaySFX.Setup<Rotate>(gameObject);
            PlayVFX.Setup<Rotate>(gameObject);
            SetupGhostDescription();
        }

        private void SetupGhostDescription()
        {
            GhostDescription = new()
            {
                OnGhostInteracted = OnGhostDataModified,
                SpawnTRS = GetSpawnTRS,
                ToggleGhostMode = () =>
                {
                    EditorOp.Resolve<Recorder>().TrackRotation_ShowGhostOnMultiselect(this, true);
                },
                ShowVisualsOnMultiSelect = true,
                GetLastValue = () => { return Type.LastVector3; },
                GetRecentValue = () => { return Type.vector3.Get(); },
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
            Type.vector3.OnPerAxisValueModified = () =>
            {
                Type.ghostLastRecordedRotation = transform.eulerAngles + (Vector3)Type.vector3.Get();
            };
            Type.vector3.OnValueReset = () =>
            {
                Type.vector3.OnPerAxisValueModified?.Invoke();
                Type.ForceRefreshData?.Invoke();
            };
        }

        public override (string type, string data) Export()
        {
            var targetVector = (Vector3)Type.vector3.Get();
            var comp = new RotateComponent
            {
                direction = Type.direction,
                repeatType = repeat.repeatType,
                speed = Type.speed,
                rotateTo = targetVector,
                pauseFor = repeat.pauseFor,
                repeatFor = repeat.repeatForever ? int.MaxValue : repeat.repeat,
                IsConditionAvailable = true,
                ConditionType = GetStartEvent(),
                ConditionData = GetStartCondition(),
                broadcastAt = repeat.broadcastAt,
                IsBroadcastable = !string.IsNullOrEmpty(repeat.broadcast),
                Broadcast = string.IsNullOrEmpty(repeat.broadcast) ? null : repeat.broadcast,
                canPlaySFX = PlaySFX.data.canPlay,
                canPlayVFX = PlayVFX.data.canPlay,
                sfxName = string.IsNullOrEmpty(PlaySFX.data.clipName) ? null : PlaySFX.data.clipName,
                vfxName = string.IsNullOrEmpty(PlayVFX.data.clipName) ? null : PlayVFX.data.clipName,
                sfxIndex = PlaySFX.data.clipIndex,
                vfxIndex = PlayVFX.data.clipIndex,
                listen = Listen.Always,
                ghostLastRotation = Type.ghostLastRecordedRotation
            };
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
            RotateComponent comp = JsonConvert.DeserializeObject<RotateComponent>(cdata.data);
            PlaySFX.data.canPlay = comp.canPlaySFX;
            PlaySFX.data.clipIndex = comp.sfxIndex;
            PlaySFX.data.clipName = comp.sfxName;
            PlayVFX.data.canPlay = comp.canPlayVFX;
            PlayVFX.data.clipIndex = comp.vfxIndex;
            PlayVFX.data.clipName = comp.vfxName;
            Type.direction = comp.direction;
            Type.speed = comp.speed;
            Type.vector3.Set(comp.rotateTo);
            repeat.pauseFor = comp.pauseFor;
            repeat.repeat = comp.repeatFor == int.MaxValue ? 1 : comp.repeatFor;
            repeat.repeatForever= comp.repeatFor == int.MaxValue;
            repeat.broadcastAt = comp.broadcastAt;
            repeat.broadcast = comp.Broadcast;
            repeat.repeatType = comp.repeatType;
            Type.ghostLastRecordedRotation = comp.ghostLastRotation;
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

        private Vector3[] GetSpawnTRS()
        {
            var position = transform.position;
            var rotation = Type.ghostLastRecordedRotation == Atom.RecordedVector3.INFINITY ||
                Type.ghostLastRecordedRotation == Vector3.zero ?
                gameObject.transform.rotation.eulerAngles :
                Type.ghostLastRecordedRotation;
            return new Vector3[] { position, rotation };
        }

        private void OnGhostDataModified(object data)
        {
            GhostDescription.IsGhostInteractedInLastRecord = true;
            var vector3 = (Vector3)data;
            vector3 = Quaternion.Euler(vector3).eulerAngles;
            Type.ghostLastRecordedRotation = vector3;
            var delta = vector3 - transform.eulerAngles;
            delta = delta.GetAbsEulerAngle();
            Type.vector3.Set(delta);
            Type.ForceRefreshData?.Invoke();
        }

        private void SetLastValue()
        {
            if (GhostDescription.IsGhostInteractedInLastRecord)
            {
                Type.LastVector3 = (Vector3)Type.vector3.Get();
            }
            GhostDescription.IsGhostInteractedInLastRecord = false;
        }
    }
}

