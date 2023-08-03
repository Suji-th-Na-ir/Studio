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
        public StartOn Start = StartOn.None;
        public Atom.PlaySfx PlaySFX = new Atom.PlaySfx();
        public Atom.PlayVfx PlayVFX = new Atom.PlayVfx();
        public string Broadcast = null;
        public string BroadcastListen = null;

        public string GetStartEvent()
        {
            if (Start == StartOn.OnPlayerCollide)
                return "Terra.Studio.TriggerAction";

            if (Start == StartOn.OnClick)
                return "Terra.Studio.MouseAction";

            if (Start == StartOn.BroadcastListen)
                return "Terra.Studio.Listener";

            return "";
        }

        public string GetStartCondition()
        {
            if (Start == StartOn.OnPlayerCollide)
                return "Player";

            if (Start == StartOn.OnClick)
                return "OnClick";

            if (Start == StartOn.BroadcastListen)
                return BroadcastListen.ToString();

            return "";
        }

        public (string type, string data) Export()
        {
            DestroyOnComponent destroyOn = new();
            {
                destroyOn.IsConditionAvailable = true;
                destroyOn.ConditionType = GetStartEvent();
                destroyOn.ConditionData = GetStartCondition();
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

            if (cc.ConditionType == "Terra.Studio.TriggerAction")
                Start = StartOn.OnPlayerCollide;
            else if (cc.ConditionType == "Terra.Studio.MouseAction")
                Start = StartOn.OnClick;
            else if (cc.ConditionType == "Terra.Studio.Listener")
                Start = StartOn.BroadcastListen;
            else
                Start = StartOn.None;

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
