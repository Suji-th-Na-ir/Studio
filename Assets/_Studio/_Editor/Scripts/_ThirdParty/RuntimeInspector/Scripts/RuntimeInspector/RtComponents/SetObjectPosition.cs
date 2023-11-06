using System;
using UnityEngine;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Terra.Studio
{
    [EditorDrawComponent("Terra.Studio.SetObjectPosition"), AliasDrawer("Teleport Self")]
    public class SetObjectPosition : BaseBehaviour
    {
        public enum StartOptions
        {
            [EditorEnumField("Terra.Studio.MouseAction", "OnClick"), AliasDrawer("Clicked")]
            OnClick,
            [EditorEnumField("Terra.Studio.TriggerAction", "Player"), AliasDrawer("Player Touches")]
            OnPlayerCollide,
            [EditorEnumField("Terra.Studio.TriggerAction", "Other"), AliasDrawer("Other Object Touches")]
            OnObjectCollide,
            [EditorEnumField("Terra.Studio.Listener"), AliasDrawer("Broadcast Listened")]
            BroadcastListen
        }

        [AliasDrawer("TeleportWhen")]
        public Atom.StartOn startOn = new();
        [AliasDrawer("Target\nPosition")]
        public Atom.RecordedVector3 targetPosition = new();
        public Atom.PlaySfx playSFX = new();
        public Atom.PlayVfx playVFX = new();
        public Atom.Broadcast broadcastData = new();

        public override string ComponentName => nameof(SetObjectPosition);
        public override bool CanPreview => true;
        public override Atom.RecordedVector3 RecordedVector3 => targetPosition;

        protected override bool CanBroadcast => true;
        protected override bool CanListen => true;
        protected override string[] BroadcasterRefs => new string[]
        {
            broadcastData.broadcast
        };
        protected override string[] ListenerRefs => new string[]
        {
            startOn.data.listenName
        };

        protected override void Awake()
        {
            base.Awake();
            startOn.Setup<StartOptions>(gameObject, ComponentName, OnListenerUpdated, startOn.data.startIndex == 3);
            playSFX.Setup<SetObjectPosition>(gameObject);
            playVFX.Setup<SetObjectPosition>(gameObject);
            broadcastData.Setup(gameObject, this);
            targetPosition.Setup(this);
            SetupTargetPosition();
            SetupGhostDescription();
        }

        private void SetupGhostDescription()
        {
            GhostDescription = new()
            {
                OnGhostInteracted = OnGhostDataModified,
                SelectionGhostsTRS = () =>
                {
                    return new Vector3[] { (Vector3)GhostDescription.GetRecentValue.Invoke(), transform.localRotation.eulerAngles, Vector3.one };
                },
                ToggleRecordMode = () =>
                {
                    EditorOp.Resolve<Recorder>().TrackPosition_Multiselect(this,true);
                },
                ShowSelectionGhost = () =>
                {
                    EditorOp.Resolve<Recorder>().ShowSelectionGhost_Position(this, true);
                },
                HideSelectionGhost = () =>
                {
                    EditorOp.Resolve<Recorder>().ShowSelectionGhost_Position(this, false);
                },
                UpdateSlectionGhostTRS = () =>
                {
                    EditorOp.Resolve<Recorder>().UpdateTRS_Multiselect(this);
                },
                GetLastValue = () => { return targetPosition.LastVector3; },
                GetRecentValue = () => { return targetPosition.Get(); },
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

        private void SetupTargetPosition()
        {
            var newTarget = transform.position;
            newTarget.z += 1f;
            targetPosition.Set(newTarget);
        }

        public override (string type, string data) Export()
        {
            var data = new SetObjectPositionComponent()
            {
                IsConditionAvailable = true,
                ConditionType = GetConditionType(),
                ConditionData = GetConditionData(),
                IsBroadcastable = !string.IsNullOrEmpty(broadcastData.broadcast),
                Broadcast = broadcastData.broadcast,
                targetPosition = (Vector3)GhostDescription.GetRecentValue.Invoke(),
                startIndex = startOn.data.startIndex,
                canPlaySFX = playSFX.data.canPlay,
                sfxIndex = playSFX.data.clipIndex,
                sfxName = playSFX.data.clipName,
                canPlayVFX = playVFX.data.canPlay,
                vfxIndex = playVFX.data.clipIndex,
                vfxName = playVFX.data.clipName,
            };
            var type = EditorOp.Resolve<DataProvider>().GetCovariance(this);
            var json = JsonConvert.SerializeObject(data);
            return (type, json);
        }

        private string GetConditionType()
        {
            var type = GetEnum();
            var value = EditorOp.Resolve<DataProvider>().GetEnumValue(type);
            return value;
        }

        private string GetConditionData()
        {
            var type = GetEnum();
            if (type == StartOptions.BroadcastListen)
            {
                return startOn.data.listenName;
            }
            var value = EditorOp.Resolve<DataProvider>().GetEnumConditionDataValue(type);
            return value;
        }

        private StartOptions GetEnum()
        {
            var selectedType = startOn.data.startName;
            var start = (StartOptions)Enum.Parse(typeof(StartOptions), selectedType);
            return start;
        }

        public override void Import(EntityBasedComponent data)
        {
            var obj = JsonConvert.DeserializeObject<SetObjectPositionComponent>(data.data);
            broadcastData.broadcast = obj.Broadcast;
            targetPosition.Set(obj.targetPosition);
            AssignStartOnData(obj);
            AssignSFXandVFXData(obj);
            var listenString = "";
            if (startOn.data.startIndex == 3)
            {
                listenString = startOn.data.listenName;
            }
            ImportVisualisation(broadcastData.broadcast, listenString);
        }

        private void AssignStartOnData(SetObjectPositionComponent comp)
        {
            if (EditorOp.Resolve<DataProvider>().TryGetEnum(comp.ConditionType, typeof(StartOptions), out var result))
            {
                var startOn = (StartOptions)result;
                var startName = startOn.ToString();
                if (startOn == StartOptions.OnPlayerCollide || startOn == StartOptions.OnObjectCollide)
                {
                    if (comp.ConditionData.Equals("Player"))
                    {
                        startName = StartOptions.OnPlayerCollide.ToString();
                    }
                    else
                    {
                        startName = StartOptions.OnObjectCollide.ToString();
                    }
                }
                this.startOn.data.startName = startName;
            }
            startOn.data.startIndex = comp.startIndex;
            startOn.data.listenName = comp.ConditionData;
        }

        private void AssignSFXandVFXData(SetObjectPositionComponent comp)
        {
            playSFX.data.canPlay = comp.canPlaySFX;
            playSFX.data.clipIndex = comp.sfxIndex;
            playSFX.data.clipName = comp.sfxName;
            playVFX.data.canPlay = comp.canPlayVFX;
            playVFX.data.clipIndex = comp.vfxIndex;
            playVFX.data.clipName = comp.vfxName;
        }

        private void SetLastValue()
        {
            if (GhostDescription.IsGhostInteractedInLastRecord)
            {
                var lastValue = GhostDescription.GetRecentValue.Invoke();
                targetPosition.LastVector3 = (Vector3)lastValue;
            }
            GhostDescription.IsGhostInteractedInLastRecord = false;
        }

        private void OnGhostDataModified(object data)
        {
            GhostDescription.IsGhostInteractedInLastRecord = true;
            targetPosition.Set(data);
        }

        public override BehaviourPreviewUI.PreviewData GetPreviewData()
        {
            var properties = new Dictionary<string, object>[1];
            var broadcastValues = new string[] { broadcastData.broadcast };
            properties[0] = new();
            if (playSFX.data.canPlay)
            {
                properties[0].Add(BehaviourPreview.Constants.SFX_PREVIEW_NAME, playSFX.data.clipName);
            }
            if (playVFX.data.canPlay)
            {
                properties[0].Add(BehaviourPreview.Constants.VFX_PREVIEW_NAME, playVFX.data.clipName);
            }
            var index = startOn.data.startIndex;
            var enumValue = (StartOptions)index;
            var name = enumValue.ToString();
            var listenTo = string.Empty;
            if (enumValue == StartOptions.BroadcastListen)
            {
                listenTo = startOn.data.listenName;
            }
            var previewData = new BehaviourPreviewUI.PreviewData()
            {
                DisplayName = GetDisplayName(),
                Broadcast = broadcastValues,
                Properties = properties,
                EventName = name,
                Listen = listenTo
            };
            return previewData;
        }
    }
}