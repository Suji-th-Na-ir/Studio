using System;
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
        [AliasDrawer("Broadcast"), OnValueChanged(UpdateBroadcast = true)] public string broadcastWhenOn;
        [AliasDrawer("Play SFX")] public Atom.PlaySfx playSFXWhenOn = new();
        [AliasDrawer("Play VFX")] public Atom.PlayVfx playVFXWhenOn = new();
        [Header("When Switch is \"Off\"")]
        [AliasDrawer("Broadcast"), OnValueChanged(UpdateBroadcast = true)] public string broadcastWhenOff;
        [AliasDrawer("Play SFX")] public Atom.PlaySfx playSFXWhenOff = new();
        [AliasDrawer("Play VFX")] public Atom.PlayVfx playVFXWhenOff = new();

        public override string ComponentName => nameof(Switch);
        protected override bool CanBroadcast => true;
        protected override bool CanListen => false;
        protected override string[] BroadcasterRefs => new string[]
        {
            broadcastWhenOn,
            broadcastWhenOff
        };

        protected override void Awake()
        {
            base.Awake();
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
                onStateData = GetSwitchComponentData(SwitchState.On, playSFXWhenOn.data, playVFXWhenOn.data, broadcastWhenOn),
                offStateData = GetSwitchComponentData(SwitchState.Off, playSFXWhenOff.data, playVFXWhenOff.data, broadcastWhenOff),
                listen = Listen.Always
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
            broadcastWhenOn = GetBroadcastData(component.onStateData);
            broadcastWhenOff = GetBroadcastData(component.offStateData);
            playSFXWhenOn.data = GetPlaySFXData(component.onStateData);
            playSFXWhenOff.data = GetPlaySFXData(component.offStateData);
            playVFXWhenOn.data = GetPlayVFXData(component.onStateData);
            playVFXWhenOff.data = GetPlayVFXData(component.offStateData);
            ImportVisualisation(broadcastWhenOn, null);
            ImportVisualisation(broadcastWhenOff, null);
        }

        private SwitchComponentData GetSwitchComponentData(SwitchState state, PlayFXData playSFXData, PlayFXData playVFXData, string broadcast)
        {
            return new SwitchComponentData()
            {
                state = state,
                canPlaySFX = playSFXData.canPlay,
                sfxName = playSFXData.clipName,
                sfxIndex = playSFXData.clipIndex,
                canPlayVFX = playVFXData.canPlay,
                vfxName = playVFXData.clipName,
                vfxIndex = playVFXData.clipIndex,
                isBroadcastable = !string.IsNullOrEmpty(broadcast),
                broadcast = broadcast
            };
        }

        private string GetBroadcastData(SwitchComponentData data)
        {
            return data.broadcast;
        }

        private PlayFXData GetPlaySFXData(SwitchComponentData data)
        {
            return new PlayFXData()
            {
                canPlay = data.canPlaySFX,
                clipIndex = data.sfxIndex,
                clipName = data.sfxName
            };
        }

        private PlayFXData GetPlayVFXData(SwitchComponentData data)
        {
            return new PlayFXData()
            {
                canPlay = data.canPlayVFX,
                clipIndex = data.vfxIndex,
                clipName = data.vfxName
            };
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
            var properties = new Dictionary<string, object>[2];
            var broadcastValues = new string[2];
            if (defaultState == SwitchState.Off)
            {
                broadcastValues[0] = broadcastWhenOff;
                broadcastValues[1] = broadcastWhenOn;
                properties[0] = new();
                if (playSFXWhenOff.data.canPlay)
                {
                    properties[0].Add(BehaviourPreview.Constants.SFX_PREVIEW_NAME, playSFXWhenOff.data.clipName);
                }
                if (playVFXWhenOff.data.canPlay)
                {
                    properties[0].Add(BehaviourPreview.Constants.VFX_PREVIEW_NAME, playVFXWhenOff.data.clipName);
                }
            }
            else
            {
                broadcastValues[0] = broadcastWhenOn;
                broadcastValues[1] = broadcastWhenOff;
                properties[1] = new();
                if (playSFXWhenOn.data.canPlay)
                {
                    properties[1].Add(BehaviourPreview.Constants.SFX_PREVIEW_NAME, playSFXWhenOn.data.clipName);
                }
                if (playVFXWhenOn.data.canPlay)
                {
                    properties[1].Add(BehaviourPreview.Constants.VFX_PREVIEW_NAME, playVFXWhenOn.data.clipName);
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
