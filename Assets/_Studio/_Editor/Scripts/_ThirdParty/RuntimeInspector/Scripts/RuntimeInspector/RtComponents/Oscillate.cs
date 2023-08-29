using Newtonsoft.Json;
using PlayShifu.Terra;
using Terra.Studio;
using UnityEngine;


namespace RuntimeInspectorNamespace
{
    [EditorDrawComponent("Terra.Studio.Oscillate")]
    public class Oscillate : MonoBehaviour, IComponent
    {
        [HideInInspector]
        public OscillateComponent Component;

        public Atom.StartOn startOn = new Atom.StartOn();
        public Vector3 fromPoint;
        public Vector3 toPoint;
        public StartOn start;
        public float Speed = 1f;
        public bool Loop = false;

        private void Awake()
        {
            fromPoint = transform.localPosition;
            Component.fromPoint = fromPoint;
        }
        
        public void Start()
        {
            startOn.Setup(gameObject);
        }

        public (string type, string data) Export()
        {
            var type = EditorOp.Resolve<DataProvider>().GetCovariance(this);

            Component.fromPoint = fromPoint;
            Component.toPoint = toPoint;
            Component.ConditionType = startOn.data.startName;
            Component.ConditionData = startOn.data.listenName;
            Component.BroadcastListen = string.IsNullOrEmpty(startOn.data.listenName) ? null : startOn.data.listenName;
            
            Component.loop = Loop;
            Component.speed = Speed;
            Component.IsConditionAvailable = GetStartEvent() != "";
            gameObject.TrySetTrigger(false, true);
            var data = JsonConvert.SerializeObject(Component);
            return (type, data);
        }

        public string GetStartEvent()
        {
            var eventName = EditorOp.Resolve<DataProvider>().GetEnumValue(start);
            return eventName;
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
                start = (StartOn)result;
            }
        }

        private void OnDestroy()
        {
            if (gameObject.TryGetComponent(out Collider collider) && !gameObject.TryGetComponent(out MeshRenderer _))
            {
                Destroy(collider);
            }
        }
    }
}