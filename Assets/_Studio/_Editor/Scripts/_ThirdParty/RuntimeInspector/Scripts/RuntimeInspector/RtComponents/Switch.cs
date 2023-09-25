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
        [AliasDrawer("Broadcast")] public string broadcastWhenOn;
        [AliasDrawer("Play SFX")] public Atom.PlaySfx playSFXWhenOn = new();
        [AliasDrawer("Play VFX")] public Atom.PlayVfx playVFXWhenOn = new();
        [Header("When Switch is \"Off\"")]
        [AliasDrawer("Broadcast")] public string broadcastWhenOff;
        [AliasDrawer("Play SFX")] public Atom.PlaySfx playSFXWhenOff = new();
        [AliasDrawer("Play VFX")] public Atom.PlayVfx playVFXWhenOff = new();

        public void Awake()
        {
            playSFXWhenOn.Setup<Switch>(gameObject, nameof(playSFXWhenOn));
            playVFXWhenOn.Setup<Switch>(gameObject, nameof(playVFXWhenOn));
            playSFXWhenOff.Setup<Switch>(gameObject, nameof(playSFXWhenOff));
            playVFXWhenOff.Setup<Switch>(gameObject, nameof(playVFXWhenOff));
            EditorOp.Resolve<UILogicDisplayProcessor>().UpdateBroadcastString(broadcastWhenOn, ""
                                 , new ComponentDisplayDock() { componentGameObject = gameObject, componentType = this.GetType().Name });
            EditorOp.Resolve<UILogicDisplayProcessor>().UpdateBroadcastString(broadcastWhenOff, ""
                                 , new ComponentDisplayDock() { componentGameObject = gameObject, componentType = this.GetType().Name });
        }

        public (string type, string data) Export()
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

        public void Import(EntityBasedComponent data)
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
            EditorOp.Resolve<UILogicDisplayProcessor>().ImportVisualisation(gameObject, GetType().Name, broadcastWhenOn, null);
            EditorOp.Resolve<UILogicDisplayProcessor>().ImportVisualisation(gameObject, GetType().Name, broadcastWhenOff, null);
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
    }
}
