using Newtonsoft.Json;
using Terra.Studio;
using UnityEngine;


namespace RuntimeInspectorNamespace
{
    [EditorDrawComponent("Terra.Studio.Oscillate")]
    public class Oscillate : MonoBehaviour, IComponent
    {
        [HideInInspector]
        public OscillateComponent Component;

        public Vector3 fromPoint;
        public Vector3 toPoint;
        public StartOn Start;
        public string BroadcastListen = null;
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
            Component.ConditionData = GetStartCondition();
            Component.BroadcastListen = string.IsNullOrEmpty(BroadcastListen) ? null : BroadcastListen;
            Component.loop = Loop;
            Component.speed = Speed;
            Component.IsConditionAvailable = GetStartEvent() != "";

            var data = JsonConvert.SerializeObject(Component);
            return (type, data);
        }

        public void Import(EntityBasedComponent cdata)
        {
            OscillateComponent cc = JsonConvert.DeserializeObject<OscillateComponent>($"{cdata.data}");
            fromPoint = cc.fromPoint;
            toPoint = cc.toPoint;
            Speed = cc.speed;
            Loop = cc.loop;

            if (EditorOp.Resolve<DataProvider>().TryGetEnum(cc.ConditionType, typeof(StartOn), out object result))
            {
                Start = (StartOn)result;
            }

            BroadcastListen = cc.BroadcastListen;
        }

        public string GetStartEvent()
        {
            var eventName = EditorOp.Resolve<DataProvider>().GetEnumValue(Start);
            return eventName;
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
    }
}