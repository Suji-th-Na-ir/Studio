using Newtonsoft.Json;
using PlayShifu.Terra;
using Terra.Studio;
using UnityEngine;
using UnityEngine.Serialization;

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
        public DestroyOnEnum start = DestroyOnEnum.OnPlayerCollide;
        public Atom.PlaySfx PlaySFX = new Atom.PlaySfx();
        public Atom.PlayVfx PlayVFX = new Atom.PlayVfx();
        public string Broadcast = null;
        public string BroadcastListen = null;
        
        public void Start()
        {
            PlaySFX.Setup(gameObject);
            PlayVFX.Setup(gameObject);
        }

        public string GetStartEvent()
        {
            var eventName = EditorOp.Resolve<DataProvider>().GetEnumValue(start);
            return eventName;
        }

        public string GetCondition()
        {
            if (start == DestroyOnEnum.BroadcastListen)
            {
                return BroadcastListen;
            }
            return EditorOp.Resolve<DataProvider>().GetEnumConditionDataValue(start);
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

                destroyOn.canPlaySFX = PlaySFX.data.canPlay;
                destroyOn.canPlayVFX = PlayVFX.data.canPlay;

                destroyOn.sfxName = Helper.GetSfxClipNameByIndex(PlaySFX.data.clipIndex);
                destroyOn.vfxName = Helper.GetVfxClipNameByIndex(PlayVFX.data.clipIndex);

                destroyOn.sfxIndex = PlaySFX.data.clipIndex;
                destroyOn.vfxIndex = PlayVFX.data.clipIndex;
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
                start = (DestroyOnEnum)result;
            }

            Broadcast = cc.Broadcast;
            BroadcastListen = cc.BroadcastListen;

            PlaySFX.data.canPlay = cc.canPlaySFX;
            PlaySFX.data.clipIndex = cc.sfxIndex;
            PlaySFX.data.clipName = cc.sfxName;
            PlayVFX.data.canPlay = cc.canPlayVFX;
            PlayVFX.data.clipIndex = cc.vfxIndex;
            PlayVFX.data.clipName = cc.vfxName;
        }
    }
}
