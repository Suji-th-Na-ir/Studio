using Newtonsoft.Json;
using Terra.Studio;
using Terra.Studio.RTEditor;
using UnityEngine;

namespace RuntimeInspectorNamespace
{
    [EditorDrawComponent("Terra.Studio.DestroyOn")]
    public class DestroyOn : MonoBehaviour, IComponent
    {
        public GlobalEnums.StartOn Start = GlobalEnums.StartOn.None;
        public Atom.PlaySfx PlaySFX = new Atom.PlaySfx();
        public Atom.PlayVfx PlayVFX = new Atom.PlayVfx();
        public string Broadcast = "";
        public string BroadcastListen = "";

        public string GetStartEvent()
        {
            if (Start == GlobalEnums.StartOn.OnPlayerCollide)
                return "Terra.Studio.TriggerAction";

            if (Start == GlobalEnums.StartOn.OnClick)
                return "Terra.Studio.MouseAction";

            if (Start == GlobalEnums.StartOn.BroadcastListen)
                return "Terra.Studio.Listener";

            return "";
        }

        public string GetStartCondition()
        {
            if (Start == GlobalEnums.StartOn.OnPlayerCollide)
                return "Player";

            if (Start == GlobalEnums.StartOn.OnClick)
                return "OnClick";

            if (Start == GlobalEnums.StartOn.BroadcastListen)
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
                destroyOn.BroadcastListen = BroadcastListen == "" ? null : BroadcastListen;
                
                destroyOn.canPlaySFX = PlaySFX.canPlay;
                destroyOn.canPlayVFX = PlayVFX.canPlay;
                
                destroyOn.sfxName = PlaySFX.clipName;
                destroyOn.vfxName = PlayVFX.clipName;
                
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
                Start = GlobalEnums.StartOn.OnPlayerCollide;
            else if (cc.ConditionType == "Terra.Studio.MouseAction")
                Start = GlobalEnums.StartOn.OnClick;
            else if (cc.ConditionType == "Terra.Studio.Listener")
                Start = GlobalEnums.StartOn.BroadcastListen;
            else
                Start = GlobalEnums.StartOn.None;
            
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
