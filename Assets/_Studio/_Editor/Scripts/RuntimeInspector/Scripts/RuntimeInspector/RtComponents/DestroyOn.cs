using Newtonsoft.Json;
using PlayShifu.Terra;
using Terra.Studio;
using Terra.Studio.RTEditor;
using UnityEngine;

namespace RuntimeInspectorNamespace
{
    [EditorDrawComponent("Terra.Studio.DestroyOn")]
    public class DestroyOn : MonoBehaviour, IComponent
    {
        public enum DestroyOnEnum
        {
            [EditorEnumField("Terra.Studio.TriggerAction", "Player")]
            OnPlayerCollide,
            [EditorEnumField("Terra.Studio.MouseAction", "OnClick")]
            OnClick,
            [EditorEnumField("Terra.Studio.Listener")]
            BroadcastListen

        }
        public DestroyOnEnum Start = DestroyOnEnum.OnPlayerCollide;
        public Atom.PlaySfx PlaySFX = new Atom.PlaySfx();
        public Atom.PlayVfx PlayVFX = new Atom.PlayVfx();
        public string Broadcast = null;
        public string BroadcastListen = null;

        public string GetStartEvent()
        {
            var eventName = EditorOp.Resolve<DataProvider>().GetEnumValue(Start);
            return eventName;
        }

        public string GetCondition()
        {
            if (Start == DestroyOnEnum.BroadcastListen)
            {
                return BroadcastListen;
            }
            return EditorOp.Resolve<DataProvider>().GetEnumConditionDataValue(Start);
        }

        public (string type, string data) Export()
        {
            DestroyOnComponent destroyOn = new();
            {
                destroyOn.IsConditionAvailable = true;
                destroyOn.ConditionType = GetStartEvent();
                destroyOn.ConditionData = GetCondition();
                destroyOn.IsBroadcastable = !string.IsNullOrEmpty(Broadcast);
                destroyOn.Broadcast = string.IsNullOrEmpty(Broadcast) ? null : Broadcast;
                destroyOn.BroadcastListen = string.IsNullOrEmpty(BroadcastListen) ? null : BroadcastListen;

                destroyOn.canPlaySFX = PlaySFX.canPlay;
                destroyOn.canPlayVFX = PlayVFX.canPlay;

                destroyOn.sfxName = Helper.GetSfxClipNameByIndex(PlaySFX.clipIndex);
                destroyOn.vfxName = Helper.GetVfxClipNameByIndex(PlayVFX.clipIndex);

                destroyOn.sfxIndex = PlaySFX.clipIndex;
                destroyOn.vfxIndex = PlayVFX.clipIndex;
            }

            var type = EditorOp.Resolve<DataProvider>().GetCovariance(this);
            var data = JsonConvert.SerializeObject(destroyOn);
            return (type, data);
        }

        public void Import(EntityBasedComponent cdata)
        {
            DestroyOnComponent cc = JsonConvert.DeserializeObject<DestroyOnComponent>($"{cdata.data}");

            if (EditorOp.Resolve<DataProvider>().TryGetEnum(cc.ConditionType, typeof(DestroyOnEnum), out object result))
            {
                Start = (DestroyOnEnum)result;
            }

            Broadcast = cc.Broadcast;
            BroadcastListen = cc.BroadcastListen;

            PlaySFX.canPlay = cc.canPlaySFX;
            PlaySFX.clipIndex = cc.sfxIndex;
            PlaySFX.clipName = cc.sfxName;
            PlayVFX.canPlay = cc.canPlayVFX;
            PlayVFX.clipIndex = cc.vfxIndex;
            PlayVFX.clipName = cc.vfxName;
        }
    }
}
