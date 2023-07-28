using System.Collections;
using System.Collections.Generic;
using RuntimeInspectorNamespace;
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
    
    public class DestroyOn : MonoBehaviour, IComponent
    {
        public DestroyOnEventType Start = DestroyOnEventType.None;
        public Atom.BroadCast Broadcast = Atom.BroadCast.None;
        public Atom.BroadCastListen BroadcastListen = Atom.BroadCastListen.None;
        public void ExportData()
        {
            
        }
        
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
    }
}
