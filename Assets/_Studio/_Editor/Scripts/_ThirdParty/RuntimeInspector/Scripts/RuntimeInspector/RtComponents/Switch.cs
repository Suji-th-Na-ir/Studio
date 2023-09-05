using System;
using UnityEngine;
using Newtonsoft.Json;
using RuntimeInspectorNamespace;

namespace Terra.Studio
{
    [EditorDrawComponent("Terra.Studio.Switch")]
    public class Switch : MonoBehaviour, IComponent
    {
        public enum StartOn
        {
            [EditorEnumField("Terra.Studio.MouseAction", "OnClick")]
            OnClick,
            [EditorEnumField("Terra.Studio.TriggerAction", "Player")]
            OnPlayerCollide,
            [EditorEnumField("Terra.Studio.TriggerAction", "Any")]
            OnObjectCollide
        }

        public StartOn switchWhen;
        public SwitchState defaultState;
        [Header("When Switch is \"On\"")]
        [AliasDrawer("Broadcast")] public Atom.Broadcast broadcastWhenOn = new();
        [AliasDrawer("Play SFX")] public Atom.PlaySfx playSFXWhenOn = new();
        [AliasDrawer("Play VFX")] public Atom.PlayVfx playVFXWhenOn = new();
        [Header("When Switch is \"Off\"")]
        [AliasDrawer("Broadcast")] public Atom.Broadcast broadcastWhenOff = new();
        [AliasDrawer("Play SFX")] public Atom.PlaySfx playSFXWhenOff = new();
        [AliasDrawer("Play VFX")] public Atom.PlayVfx playVFXWhenOff = new();

        private string guid;

        private void Awake()
        {
            guid = Guid.NewGuid().ToString("N");
        }

        public (string type, string data) Export()
        {
            var data = new SwitchComponent()
            {
                IsConditionAvailable = true,
                ConditionType = EditorOp.Resolve<DataProvider>().GetEnumValue(switchWhen),
                ConditionData = EditorOp.Resolve<DataProvider>().GetEnumConditionDataValue(switchWhen),
                currentState = defaultState,
                onStateData = GetSwitchComponentData(SwitchState.On, playSFXWhenOn.data, playVFXWhenOn.data, broadcastWhenOn.data),
                offStateData = GetSwitchComponentData(SwitchState.Off, playSFXWhenOff.data, playVFXWhenOff.data, broadcastWhenOff.data),
                listen = Listen.Always
            };
            var type = EditorOp.Resolve<DataProvider>().GetCovariance(this);
            var json = JsonConvert.SerializeObject(data);
            return (type, json);
        }

        public void Import(EntityBasedComponent data)
        {
            var component = JsonConvert.DeserializeObject<SwitchComponent>(data.data);
            defaultState = component.currentState;
            switchWhen = GetStartOn(component.ConditionType, component.ConditionData);
            broadcastWhenOn.data = GetBroadcastData(component.onStateData);
            broadcastWhenOff.data = GetBroadcastData(component.offStateData);
            playSFXWhenOn.data = GetPlaySFXData(component.onStateData);
            playSFXWhenOff.data = GetPlaySFXData(component.offStateData);
            playVFXWhenOn.data = GetPlayVFXData(component.onStateData);
            playVFXWhenOff.data = GetPlayVFXData(component.offStateData);
        }

        private SwitchComponentData GetSwitchComponentData(SwitchState state, PlaySFXData playSFXData, PlayVFXData playVFXData, BroadcastData broadcast)
        {
            return new SwitchComponentData()
            {
                state = SwitchState.On,
                canPlaySFX = playSFXData.canPlay,
                sfxName = playSFXData.clipName,
                sfxIndex = playSFXData.clipIndex,
                canPlayVFX = playVFXData.canPlay,
                vfxName = playVFXData.clipName,
                vfxIndex = playVFXData.clipIndex,
                isBroadcastable = !string.IsNullOrEmpty(broadcast.broadcastName),
                broadcast = broadcast.broadcastName,
                broadcastIndex = broadcast.broadcastTypeIndex
            };
        }

        private BroadcastData GetBroadcastData(SwitchComponentData data)
        {
            return new BroadcastData()
            {
                id = guid,
                broadcastName = data.broadcast,
                broadcastTypeIndex = data.broadcastIndex
            };
        }

        private PlaySFXData GetPlaySFXData(SwitchComponentData data)
        {
            return new PlaySFXData()
            {
                canPlay = data.canPlaySFX,
                clipIndex = data.sfxIndex,
                clipName = data.sfxName
            };
        }

        private PlayVFXData GetPlayVFXData(SwitchComponentData data)
        {
            return new PlayVFXData()
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
    }
}
