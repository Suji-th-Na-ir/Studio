using UnityEngine;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Terra.Studio
{
    [EditorDrawComponent("Terra.Studio.Teleport"), AliasDrawer("Teleport Player")]
    public class Teleport : BaseBehaviour
    {
        [AliasDrawer("Teleport\nTo")]
        public Atom.RecordedVector3 teleportTo = new();
        public Atom.PlaySfx playSFX = new();
        public Atom.PlayVfx playVFX = new();
        public Atom.Broadcast broadcastData = new();

        public override string ComponentName => nameof(Teleport);
        public override bool CanPreview => true;
        public override Atom.RecordedVector3 RecordedVector3 => teleportTo;
        protected override bool CanBroadcast => true;
        protected override bool CanListen => false;
        protected override string[] BroadcasterRefs => new string[]
        {
            broadcastData.broadcast
        };
        protected override Atom.PlaySfx[] Sfxes => new Atom.PlaySfx[]
        {
            playSFX
        };
        protected override Atom.PlayVfx[] Vfxes => new Atom.PlayVfx[]
        {
            playVFX
        };

        protected override void Awake()
        {
            base.Awake();
            playSFX.Setup<Teleport>(gameObject);
            playVFX.Setup<Teleport>(gameObject);
            broadcastData.Setup(gameObject, this);
            teleportTo.Setup(this);
            SetupTeleportPosition();
            SetupGhostDescription();
        }

        private void SetupTeleportPosition()
        {
            var currentPos = transform.position;
            currentPos.y += 1f;
            currentPos.z += 1f;
            teleportTo.Set(currentPos);
        }

        private void SetupGhostDescription()
        {
            GhostDescription = new()
            {
                OnGhostInteracted = OnGhostDataModified,
                SelectionGhostsTRS = () =>
                {
                    return new Vector3[] { (Vector3)GhostDescription.GetRecentValue.Invoke(), Quaternion.identity.eulerAngles, Vector3.one };
                },
                ToggleRecordMode = () =>
                {
                    EditorOp.Resolve<Recorder>().TrackPosition_Multiselect(this, true);
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
                GetLastValue = () => { return teleportTo.LastVector3; },
                GetRecentValue = () => { return teleportTo.Get(); },
                OnGhostModeToggled = (state) =>
                {
                    if (state)
                    {
                        SetLastValue();
                    }
                },
                IsGhostInteractedInLastRecord = true,
                GhostTo = EditorOp.Load<GameObject>("Prefabs/PlayerModel")
            };
            SetLastValue();
        }

        public override (string type, string data) Export()
        {
            var data = new TeleportComponent()
            {
                teleportTo = (Vector3)teleportTo.Get(),
                FXData = GetFXData(),
                IsBroadcastable = !string.IsNullOrEmpty(broadcastData.broadcast),
                Broadcast = broadcastData.broadcast,
                IsConditionAvailable = true,
                ConditionType = "Terra.Studio.TriggerAction",
                ConditionData = "Player",
                Listen = Listen.Always
            };
            var type = EditorOp.Resolve<DataProvider>().GetCovariance(this);
            var json = JsonConvert.SerializeObject(data);
            return (type, json);
        }

        public override void Import(EntityBasedComponent data)
        {
            var obj = JsonConvert.DeserializeObject<TeleportComponent>(data.data);
            teleportTo.Set(obj.teleportTo);
            MapSFXAndVFXData(obj.FXData);
            broadcastData.broadcast = obj.Broadcast;
            ImportVisualisation(broadcastData.broadcast, null);
        }

        private void SetLastValue()
        {
            if (GhostDescription.IsGhostInteractedInLastRecord)
            {
                var lastValue = GhostDescription.GetRecentValue.Invoke();
                teleportTo.LastVector3 = (Vector3)lastValue;
            }
            GhostDescription.IsGhostInteractedInLastRecord = false;
        }

        private void OnGhostDataModified(object data)
        {
            GhostDescription.IsGhostInteractedInLastRecord = true;
            teleportTo.Set(data);
        }

        public override BehaviourPreviewUI.PreviewData GetPreviewData()
        {
            var properties = new Dictionary<string, object>[1];
            properties[0] = new();
            if (playSFX.data.canPlay)
            {
                properties[0].Add(BehaviourPreview.Constants.SFX_PREVIEW_NAME, playSFX.data.clipName);
            }
            if (playVFX.data.canPlay)
            {
                properties[0].Add(BehaviourPreview.Constants.VFX_PREVIEW_NAME, playVFX.data.clipName);
            }
            var broadcasts = new string[] { broadcastData.broadcast };
            var previewData = new BehaviourPreviewUI.PreviewData()
            {
                DisplayName = GetDisplayName(),
                EventName = StartOn.OnPlayerCollide.ToString(),
                Properties = properties,
                Broadcast = broadcasts
            };
            return previewData;
        }
    }
}
