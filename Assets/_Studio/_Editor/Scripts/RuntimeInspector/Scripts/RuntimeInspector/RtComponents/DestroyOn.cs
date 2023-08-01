using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using RuntimeInspectorNamespace;
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
            DestroyOnComponent destroyOn = new()
            {
                IsConditionAvailable = true,
                ConditionType = GetStartEvent(),
                ConditionData = GetStartCondition(),
                IsBroadcastable = Broadcast != "",
                Broadcast = Broadcast == "" ? null : Broadcast.ToString(),
                canPlaySFX = state.GetItem<bool>("sfx_toggle"),
                canPlayVFX = state.GetItem<bool>("vfx_toggle"),
                sfxName = !state.GetItem<bool>("sfx_toggle") ? null : PlaySFXField.GetSfxClipName(state.GetItem<int>("sfx_dropdown")),
                vfxName = !state.GetItem<bool>("vfx_toggle") ? null : PlayVFXField.GetVfxClipName(state.GetItem<int>("vfx_dropdown"))
            };
            var type = EditorOp.Resolve<DataProvider>().GetCovariance(this);
            var data = JsonConvert.SerializeObject(destroyOn);
            return (type, data);
        }

        public void Import(EntityBasedComponent data)
        {
            
        }
    }
}
