using UnityEngine;
using Newtonsoft.Json;
using PlayShifu.Terra;
using System.Collections.Generic;

namespace Terra.Studio
{
    [EditorDrawComponent("Terra.Studio.Rotate")]
    public class Rotate : BaseBehaviour
    {
        [AliasDrawer("RotateWhen")]
        public Atom.StartOn StartOn = new();
        public Atom.Rotate Type = new();
        [AliasDrawer("Speed")] public float speed = 20f;
        [AliasDrawer("Direction")]
        public Direction direction;
        [AliasDrawer("Repeat")] public Atom.Repeat repeat = new();
        public Atom.PlaySfx PlaySFX = new();
        public Atom.PlayVfx PlayVFX = new();
        public override Atom.RecordedVector3 RecordedVector3 { get { return Type.vector3; } }

        public override string ComponentName => nameof(Rotate);
        public override bool CanPreview => true;
        protected override bool CanBroadcast => true;
        protected override bool CanListen => true;
        protected override bool UpdateListenOnEnable => StartOn.data.startIndex == 4;
        protected override string[] BroadcasterRefs => new string[]
        {
          repeat.broadcastData.broadcast
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
                direction = repeat.repeatType == RepeatDirectionType.PingPong ? Direction.Clockwise : direction,
                repeatType = repeat.repeatType,
                speed = speed,
                rotateTo = targetVector,
                pauseFor = repeat.pauseFor,
                repeatFor = repeat.repeat,
                repeatForever = repeat.repeatForever,
                IsConditionAvailable = true,
                ConditionType = GetStartEvent(),
                ConditionData = GetStartCondition(),
                broadcastAt = repeat.broadcastAt,
                IsBroadcastable = !string.IsNullOrEmpty(repeat.broadcastData.broadcast),
                Broadcast = string.IsNullOrEmpty(repeat.broadcastData.broadcast) ? null : repeat.broadcastData.broadcast,
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
            direction = comp.direction;
            speed = comp.speed;
            Type.vector3.Set(comp.rotateTo);
            repeat.pauseFor = comp.pauseFor;
            repeat.repeat = comp.repeatFor;
            repeat.repeatForever = comp.repeatForever;
            repeat.broadcastAt = comp.broadcastAt;
            repeat.broadcastData.broadcast = comp.Broadcast;
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
            ImportVisualisation(repeat.broadcastData.broadcast, listenString);
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

        public override BehaviourPreviewUI.PreviewData GetPreviewData()
        {
            var properties = new Dictionary<string, object>[1];
            var repeatString = repeat.repeatForever ? "Forever" : repeat.repeat.ToString();
            properties[0] = new()
            {
                { "Speed", speed },
                { "Repeat", repeatString },
                { "Pause", repeat.pauseFor }
            };
            if (PlaySFX.data.canPlay)
            {
                properties[0].Add(BehaviourPreview.Constants.SFX_PREVIEW_NAME, PlaySFX.data.clipName);
            }
            if (PlayVFX.data.canPlay)
            {
                properties[0].Add(BehaviourPreview.Constants.VFX_PREVIEW_NAME, PlayVFX.data.clipName);
            }
            var startOnIndex = StartOn.data.startIndex;
            var startOnName = (StartOn)startOnIndex;
            var previewData = new BehaviourPreviewUI.PreviewData()
            {
                DisplayName = GetDisplayName(),
                EventName = startOnName.ToString(),
                Properties = properties,
                Broadcast = new string[] { repeat.broadcastData.broadcast },
                Listen = StartOn.data.listenName
            };
            return previewData;
        }
    }
}
