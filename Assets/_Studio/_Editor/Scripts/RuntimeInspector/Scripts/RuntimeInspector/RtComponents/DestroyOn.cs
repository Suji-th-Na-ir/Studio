using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using PlayShifu.Terra;
using RuntimeInspectorNamespace;
using Terra.Studio;
using Terra.Studio.RTEditor;
using UnityEditor;
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
        public string Broadcast = "";
        public string BroadcastListen = "";
        public Atom.PlaySFX PlaySfx = Atom.PlaySFX.Off;
        public Atom.PlayVFX PlayVFX = Atom.PlayVFX.Off;
        
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
            var state = GetComponent<InspectorStateManager>();
            Debug.Log("state "+state);
            DestroyOnComponent destroyOn = new();
            
            destroyOn.IsConditionAvailable = true;
            destroyOn.ConditionType = GetStartEvent();
            destroyOn.ConditionData = GetStartCondition();
            destroyOn.IsBroadcastable = Broadcast != "";
            destroyOn.Broadcast = Broadcast == "" ? null : Broadcast.ToString();
            destroyOn.BroadcastListen = BroadcastListen == "" ? null : BroadcastListen.ToString();
            destroyOn.canPlaySFX = state.GetItem<bool>("sfx_toggle");
            destroyOn.canPlayVFX = state.GetItem<bool>("vfx_toggle");
                
            destroyOn.sfxName = !state.GetItem<bool>("sfx_toggle")
                ? null
                : Helper.GetSfxClipNameByIndex(state.GetItem<int>("sfx_dropdown"));
                
            destroyOn.vfxName = !state.GetItem<bool>("vfx_toggle")
                ? null
                : Helper.GetVfxClipNameByIndex(state.GetItem<int>("vfx_dropdown"));
                destroyOn.sfxIndex = state.GetItem<int>("sfx_dropdown");
                destroyOn.vfxIndex = state.GetItem<int>("vfx_dropdown");
            
            var type = EditorOp.Resolve<DataProvider>().GetCovariance(this);
            var data = JsonConvert.SerializeObject(destroyOn);
            return (type, data);
        }

        public void Import(EntityBasedComponent cdata)
        {
            var state = GetComponent<InspectorStateManager>();

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
            
            state.SetItem("sfx_toggle", cc.canPlaySFX);
            state.SetItem("vfx_toggle", cc.canPlayVFX);
            state.SetItem("sfx_dropdown", cc.sfxIndex);
            state.SetItem("vfx_dropdown", cc.vfxIndex);
        }
    }
}
