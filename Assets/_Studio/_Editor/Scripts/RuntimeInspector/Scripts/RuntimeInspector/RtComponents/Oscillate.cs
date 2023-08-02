using Newtonsoft.Json;
using Terra.Studio;
using UnityEngine;


namespace RuntimeInspectorNamespace
{
    public enum OscillateEventType
    {
        OnPlayerCollide,
        OnClick,
        OnBroadcastListen
    }
    
    [EditorDrawComponent("Terra.Studio.Oscillate")]
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
            var type = EditorOp.Resolve<DataProvider>().GetCovariance(this);
            
            Component.fromPoint = fromPoint;
            Component.toPoint = toPoint;
            Component.ConditionType = GetStartEvent();
            Component.ConditionData = BroadcastListen;
            Component.BroadcastListen = BroadcastListen;
            Component.loop = Loop;
            Component.speed = Speed;
            Component.IsConditionAvailable = GetStartEvent() != "";
            

            var data = JsonConvert.SerializeObject(Component, Formatting.None, new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
            return (type, data);
        }

        public void Import(EntityBasedComponent cdata)
        {   
            // var state = GetComponent<InspectorStateManager>();
            OscillateComponent cc = JsonConvert.DeserializeObject<OscillateComponent>($"{cdata.data}");
            fromPoint = cc.fromPoint;
            toPoint = cc.toPoint;
            Speed = cc.speed;
            Loop = cc.loop;

            if (cc.ConditionType == "Terra.Studio.TriggerAction")
                Start = OscillateEventType.OnPlayerCollide;
            else if (cc.ConditionType == "Terra.Studio.MouseAction")
                Start = OscillateEventType.OnClick;
            else if (cc.ConditionType == "Terra.Studio.Listener")
                Start = OscillateEventType.OnBroadcastListen;

            BroadcastListen = cc.BroadcastListen;
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
    }
}