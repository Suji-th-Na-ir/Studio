using System;
using System.Collections;
using System.Collections.Generic;
using RuntimeInspectorNamespace;
using Terra.Studio.RTEditor;
using UnityEngine;

namespace RuntimeInspectorNamespace
{
    public enum CollectableEventType
    {
        OnPlayerCollide,
        OnClick
    }

    public class Collectible : MonoBehaviour, IComponent
    {
        public CollectableEventType Start = CollectableEventType.OnPlayerCollide;
        public Atom.PlaySFX PlaySfx = Atom.PlaySFX.Off;
        public Atom.PlayVFX PlayVFX = Atom.PlayVFX.Off;
        public bool ShowScoreUI = false;
        public int ScoreValue = 0;
        public Atom.BroadCast Broadcast = Atom.BroadCast.None;

        public void ExportData()
        {
            Debug.Log("name "+gameObject.name);
            Debug.Log("start type "+Start.ToString());
            Debug.Log("Broadcast "+Broadcast.ToString());
        }
        
        public string GetStartEvent()
        {
            if (Start == CollectableEventType.OnPlayerCollide)
                return "Terra.Studio.TriggerAction";
            
            if (Start == CollectableEventType.OnClick)
                return "Terra.Studio.MouseAction";

            return "";
        }
        
        public string GetStartCondition()
        {
            if (Start == CollectableEventType.OnPlayerCollide)
                return "Player";
            
            if (Start == CollectableEventType.OnClick)
                return "OnClick";

            return "";
        }
    }
}
