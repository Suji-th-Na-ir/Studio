using Newtonsoft.Json;
using Terra.Studio;
using Terra.Studio.RTEditor;
using UnityEngine;

namespace RuntimeInspectorNamespace
{
    public enum DestroyOnEventType
    {
        None,
        OnPlayerCollide,
        OnClick,
        BroadcastListen
    }

    [EditorDrawComponent("Terra.Studio.DestroyOn")]
    public class DestroyOn : MonoBehaviour, IComponent
    {
        public DestroyOnEventType Start = DestroyOnEventType.None;
        public Atom.PlaySfx PlaySFX = new Atom.PlaySfx();
        public Atom.PlayVfx PlayVFX = new Atom.PlayVfx();
        public string Broadcast = "";
        public string BroadcastListen = "";

        public string GetStartEvent()
        {
            if (Start == DestroyOnEventType.OnPlayerCollide)
                return "Terra.Studio.TriggerAction";

            if (Start == DestroyOnEventType.OnClick)
                return "Terra.Studio.MouseAction";

            if (Start == DestroyOnEventType.BroadcastListen)
                return "Terra.Studio.Listener";

            return "";
        }

        public string GetStartCondition()
        {
            if (Start == DestroyOnEventType.OnPlayerCollide)
                return "Player";

            if (Start == DestroyOnEventType.OnClick)
                return "OnClick";

            if (Start == DestroyOnEventType.BroadcastListen)
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
                destroyOn.IsBroadcastable = Broadcast != "";
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
                Start = DestroyOnEventType.OnPlayerCollide;
            else if (cc.ConditionType == "Terra.Studio.MouseAction")
                Start = DestroyOnEventType.OnClick;
            else if (cc.ConditionType == "Terra.Studio.Listener")
                Start = DestroyOnEventType.BroadcastListen;
            else
                Start = DestroyOnEventType.None;
            
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
