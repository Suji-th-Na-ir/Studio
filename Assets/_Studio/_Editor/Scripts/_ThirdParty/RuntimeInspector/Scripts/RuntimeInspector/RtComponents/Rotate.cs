using UnityEngine;
using Terra.Studio;
using Newtonsoft.Json;
using PlayShifu.Terra;
using System.Collections.Generic;

namespace RuntimeInspectorNamespace
{
    [EditorDrawComponent("Terra.Studio.Rotate")]
    public class Rotate : BaseBehaviour
    {
        [AliasDrawer("RotateWhen")]
        public Atom.StartOn StartOn = new();
        public Atom.Rotate Type = new();
        public Atom.PlaySfx PlaySFX = new();
        public Atom.PlayVfx PlayVFX = new();

        public override string ComponentName => nameof(Rotate);
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
                GetLastValue = () => { return Type.data.LastVector3; },
                GetRecentValue = () => { return Type.data.vector3.Get(); },
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
            Type.data.vector3.OnPerAxisValueModified = () =>
            {
                Type.data.ghostLastRecordedRotation = transform.eulerAngles + (Vector3)Type.data.vector3.Get();
            };
            Type.data.vector3.OnValueReset = () =>
            {
                Type.data.vector3.OnPerAxisValueModified?.Invoke();
                Type.ForceRefreshData?.Invoke();
            };
        }

        public override (string type, string data) Export()
        {
            var targetVector = (Vector3)Type.data.vector3.Get();
            var comp = new RotateComponent
            {
                direction = Type.data.direction,
                rotationType = (RotationType)Type.data.rotateType,
                repeatType = GetRepeatType(Type.data.repeat),
                speed = Type.data.speed,
                rotateTo = targetVector,
                pauseFor = Type.data.pauseBetween,
                repeatFor = Type.data.repeat,
                IsConditionAvailable = true,
                ConditionType = GetStartEvent(),
                ConditionData = GetStartCondition(),
                broadcastAt = Type.data.broadcastAt,
                IsBroadcastable = !string.IsNullOrEmpty(Type.data.Broadcast),
                Broadcast = string.IsNullOrEmpty(Type.data.Broadcast) ? null : Type.data.Broadcast,
                canPlaySFX = PlaySFX.data.canPlay,
                canPlayVFX = PlayVFX.data.canPlay,
                sfxName = string.IsNullOrEmpty(PlaySFX.data.clipName) ? null : PlaySFX.data.clipName,
                vfxName = string.IsNullOrEmpty(PlayVFX.data.clipName) ? null : PlayVFX.data.clipName,
                sfxIndex = PlaySFX.data.clipIndex,
                vfxIndex = PlayVFX.data.clipIndex,
                listen = Listen.Always,
                ghostLastRotation = Type.data.ghostLastRecordedRotation
            };
            ModifyDataAsPerSelected(ref comp);
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

        private RepeatType GetRepeatType(float _value)
        {
            if (_value == 0) return RepeatType.Forever;
            else return RepeatType.XTimes;
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
            Type.data.direction = comp.direction;
            Type.data.rotateType = (int)comp.rotationType;
            Type.data.speed = comp.speed;
            Type.data.vector3.Set(comp.rotateTo);
            Type.data.pauseBetween = comp.pauseFor;
            Type.data.repeat = comp.repeatFor;
            Type.data.broadcastAt = comp.broadcastAt;
            Type.data.Broadcast = comp.Broadcast;
            Type.data.listen = comp.listen;
            Type.data.ghostLastRecordedRotation = comp.ghostLastRotation;
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
            ImportVisualisation(Type.data.Broadcast, listenString);
        }

        private void ModifyDataAsPerSelected(ref RotateComponent component)
        {
            switch (component.rotationType)
            {
                case RotationType.RotateForever:
                case RotationType.OscillateForever:
                case RotationType.IncrementallyRotateForever:
                    component.repeatFor = int.MaxValue;
                    break;
            }
        }

        private Vector3[] GetSpawnTRS()
        {
            var position = transform.position;
            var rotation = Type.data.ghostLastRecordedRotation == Atom.RecordedVector3.INFINITY ||
                Type.data.ghostLastRecordedRotation == Vector3.zero ?
                gameObject.transform.rotation.eulerAngles :
                Type.data.ghostLastRecordedRotation;
            return new Vector3[] { position, rotation };
        }

        private void OnGhostDataModified(object data)
        {
            GhostDescription.IsGhostInteractedInLastRecord = true;
            var vector3 = (Vector3)data;
            vector3 = Quaternion.Euler(vector3).eulerAngles;
            Type.data.ghostLastRecordedRotation = vector3;
            var delta = vector3 - transform.eulerAngles;
            delta = delta.GetAbsEulerAngle();
            Type.data.vector3.Set(delta);
            Type.ForceRefreshData?.Invoke();
        }

        private void SetLastValue()
        {
            if (GhostDescription.IsGhostInteractedInLastRecord)
            {
                Type.data.LastVector3 = (Vector3)Type.data.vector3.Get();
            }
            GhostDescription.IsGhostInteractedInLastRecord = false;
        }
    }
}

