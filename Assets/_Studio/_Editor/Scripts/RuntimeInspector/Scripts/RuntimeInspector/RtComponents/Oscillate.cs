using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Terra.Studio;
using RuntimeInspectorNamespace;
using Newtonsoft.Json;
using Terra.Studio.RTEditor;

namespace RuntimeInspectorNamespace
{
    public enum OscillateEventType
    {
        OnPlayerCollide,
        OnClick,
        OnBroadcastListen
    }
    
    public class Oscillate : MonoBehaviour, IComponent
    {
        [HideInInspector]
        public Terra.Studio.OscillateComponent Component;

        public Vector3 fromPoint;
        public Vector3 toPoint;
        public OscillateEventType Start;
        public string BroadcastListen = "";
        public float Speed = 1f;
        public bool Loop = false;
        
        private void Awake()
        {
            fromPoint = transform.position;
            Component.fromPoint = transform.position;
        }

        public (string type, string data) Export()
        {
            var type = "Terra.Studio.Oscillate";
            
            Component.fromPoint = fromPoint;
            Component.toPoint = toPoint;
            Component.ConditionType = GetStartEvent();
            Component.ConditionData = BroadcastListen;
            Component.loop = Loop;
            Component.speed = Speed;
            Component.IsConditionAvailable = GetStartEvent() != "";
            
            var data = JsonConvert.SerializeObject(Component);
            return (type, data);
        }
        
        public string GetStartEvent()
        {
            if (Start == OscillateEventType.OnPlayerCollide)
                return "Terra.Studio.TriggerAction";

            if (Start == OscillateEventType.OnClick)
                return "Terra.Studio.MouseAction";
            
            if (Start == OscillateEventType.OnBroadcastListen)
                return "Terra.Studio.Listener";

            return "";
        }

        public void ExportData()
        {
            var resp = EditorOp.Resolve<SelectionHandler>().GetSceneData(this);
            SystemOp.Resolve<CrossSceneDataHolder>().Set(resp);
        }
    }
}