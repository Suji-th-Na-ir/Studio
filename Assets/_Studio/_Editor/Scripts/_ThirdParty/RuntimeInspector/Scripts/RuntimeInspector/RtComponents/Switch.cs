using UnityEngine;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Terra.Studio
{
    [EditorDrawComponent("Terra.Studio.Switch")]
    public class Switch : BaseBehaviour
    {
        public enum StartOn
        {
            [EditorEnumField("Terra.Studio.MouseAction", "OnClick"), AliasDrawer("Clicked")]
            OnClick,
            [EditorEnumField("Terra.Studio.TriggerAction", "Player"), AliasDrawer("Player Touches")]
            OnPlayerCollide,
            [EditorEnumField("Terra.Studio.TriggerAction", "Other"), AliasDrawer("Other Object Touches")]
            OnObjectCollide
        }

        [AliasDrawer("Switch\nWhen")] public StartOn switchWhen;
        [AliasDrawer("Default")] public SwitchState defaultState;
        [Header("When Switch is \"On\"")]
        public Atom.Broadcast broadcastWhenOnData = new();
        [AliasDrawer("Play SFX")] public Atom.PlaySfx playSFXWhenOn = new();
        [AliasDrawer("Play VFX")] public Atom.PlayVfx playVFXWhenOn = new();
        [Header("When Switch is \"Off\"")]
        public Atom.Broadcast broadcastWhenOffData = new();
        [AliasDrawer("Play SFX")] public Atom.PlaySfx playSFXWhenOff = new();
        [AliasDrawer("Play VFX")] public Atom.PlayVfx playVFXWhenOff = new();

        public override string ComponentName => nameof(Switch);
        public override bool CanPreview => true;
        protected override bool CanBroadcast => true;
        protected override bool CanListen => false;
        protected override string[] BroadcasterRefs => new string[]
        {
            broadcastWhenOnData.broadcast,
            broadcastWhenOffData.broadcast
        };
        protected override Atom.PlaySfx[] Sfxes => new Atom.PlaySfx[]
        {
            playSFXWhenOn,
            playSFXWhenOff
        };
        protected override Atom.PlayVfx[] Vfxes => new Atom.PlayVfx[]
        {
            playVFXWhenOn,
            playVFXWhenOff
        };

        protected override void Awake()
        {
            base.Awake();
            broadcastWhenOffData.Setup(gameObject, this);
            broadcastWhenOnData.Setup(gameObject, this);
            playSFXWhenOn.Setup<Switch>(gameObject, nameof(playSFXWhenOn));
            playVFXWhenOn.Setup<Switch>(gameObject, nameof(playVFXWhenOn));
            playSFXWhenOff.Setup<Switch>(gameObject, nameof(playSFXWhenOff));
            playVFXWhenOff.Setup<Switch>(gameObject, nameof(playVFXWhenOff));
        }

        public override (string type, string data) Export()
        {
            var data = new SwitchComponent()
            {
                IsConditionAvailable = true,
                ConditionType = EditorOp.Resolve<DataProvider>().GetEnumValue(switchWhen),
                ConditionData = EditorOp.Resolve<DataProvider>().GetEnumConditionDataValue(switchWhen),
                currentState = defaultState,
                onStateData = GetSwitchComponentData(SwitchState.On, broadcastWhenOnData.broadcast),
                offStateData = GetSwitchComponentData(SwitchState.Off, broadcastWhenOffData.broadcast),
                Listen = Listen.Always,
                FXData = GetFXData()
            };
            var type = EditorOp.Resolve<DataProvider>().GetCovariance(this);
            var json = JsonConvert.SerializeObject(data);
            return (type, json);
        }

        public override void Import(EntityBasedComponent data)
        {
            var component = JsonConvert.DeserializeObject<SwitchComponent>(data.data);
            defaultState = component.currentState;
            switchWhen = GetStartOn(component.ConditionType, component.ConditionData);
            broadcastWhenOnData.broadcast = GetBroadcastData(component.onStateData);
            broadcastWhenOffData.broadcast = GetBroadcastData(component.offStateData);
            MapSFXAndVFXData(component.FXData);
            ImportVisualisation(broadcastWhenOnData.broadcast, null);
            ImportVisualisation(broadcastWhenOffData.broadcast, null);
        }

        private SwitchComponentData GetSwitchComponentData(SwitchState state, string broadcast)
        {
            return new SwitchComponentData()
            {
                state = state,
                isBroadcastable = !string.IsNullOrEmpty(broadcast),
                broadcast = broadcast
            };
        }

        private string GetBroadcastData(SwitchComponentData data)
        {
            return data.broadcast;
        }

        private StartOn GetStartOn(string type, string data)
        {
            if (type.Equals(EditorOp.Resolve<DataProvider>().GetEnumValue(StartOn.OnClick)))
            {
                return StartOn.OnClick;
            }
            else if (data.Equals("Player"))
            {
                return StartOn.OnPlayerCollide;
            }
            else
            {
                return StartOn.OnObjectCollide;
            }
        }

        public override BehaviourPreviewUI.PreviewData GetPreviewData()
        {
            var properties = new Dictionary<string, object>[] { new(), new() };
            var broadcastValues = new string[2];
            if (defaultState == SwitchState.Off)
            {
                broadcastValues[0] = broadcastWhenOffData.broadcast;
                broadcastValues[1] = broadcastWhenOnData.broadcast;
                if (playSFXWhenOff.data.canPlay)
                {
                    properties[0].Add(BehaviourPreview.Constants.SFX_PREVIEW_NAME, playSFXWhenOff.data.clipName);
                }
                if (playVFXWhenOff.data.canPlay)
                {
                    properties[0].Add(BehaviourPreview.Constants.VFX_PREVIEW_NAME, playVFXWhenOff.data.clipName);
                }
                if (playSFXWhenOn.data.canPlay)
                {
                    properties[1].Add(BehaviourPreview.Constants.SFX_PREVIEW_NAME, playSFXWhenOn.data.clipName);
                }
                if (playVFXWhenOn.data.canPlay)
                {
                    properties[1].Add(BehaviourPreview.Constants.VFX_PREVIEW_NAME, playVFXWhenOn.data.clipName);
                }
            }
            else
            {
                broadcastValues[0] = broadcastWhenOnData.broadcast;
                broadcastValues[1] = broadcastWhenOffData.broadcast;
                if (playSFXWhenOn.data.canPlay)
                {
                    properties[0].Add(BehaviourPreview.Constants.SFX_PREVIEW_NAME, playSFXWhenOn.data.clipName);
                }
                if (playVFXWhenOn.data.canPlay)
                {
                    properties[0].Add(BehaviourPreview.Constants.VFX_PREVIEW_NAME, playVFXWhenOn.data.clipName);
                }
                if (playSFXWhenOff.data.canPlay)
                {
                    properties[1].Add(BehaviourPreview.Constants.SFX_PREVIEW_NAME, playSFXWhenOff.data.clipName);
                }
                if (playVFXWhenOff.data.canPlay)
                {
                    properties[1].Add(BehaviourPreview.Constants.VFX_PREVIEW_NAME, playVFXWhenOff.data.clipName);
                }
            }
            var previewData = new BehaviourPreviewUI.PreviewData()
            {
                DisplayName = GetDisplayName(),
                EventName = switchWhen.ToString(),
                Broadcast = broadcastValues,
                Properties = properties
            };
            return previewData;
        }
    }
}
