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
        [AliasDrawer("OscillateWhen")]
        public Atom.StartOn startOn = new();
        public Vector3 fromPoint;
        public Vector3 toPoint;
        public float Speed = 1f;
        public bool Loop = false;

        private void Awake()
        {
            startOn.Setup(gameObject, Helper.GetEnumValuesAsStrings<StartOn>(), Helper.GetEnumWithAliasNames<StartOn>(), GetType().Name,startOn.data.startIndex==3);
            fromPoint = transform.localPosition;
            Component.fromPoint = fromPoint;
        }

        public (string type, string data) Export()
        {
            var type = EditorOp.Resolve<DataProvider>().GetCovariance(this);
            Component.fromPoint = fromPoint;
            Component.toPoint = toPoint;
            Component.loop = Loop;
            Component.speed = Speed;
            Component.IsConditionAvailable = !string.IsNullOrEmpty(GetStartEvent());
            Component.ConditionType = GetStartEvent();
            Component.ConditionData = GetStartCondition();
            gameObject.TrySetTrigger(false, true);
            var data = JsonConvert.SerializeObject(Component);
            return (type, data);
        }

        public string GetStartEvent()
        {
            int index = startOn.data.startIndex;
            var value = (StartOn)index;
            var eventName = EditorOp.Resolve<DataProvider>().GetEnumValue(value);
            return eventName;
        }


        public string GetStartCondition()
        {
            int index = startOn.data.startIndex;
            var value = (StartOn)index;
            string inputString = value.ToString();
            if (inputString.ToLower().Contains("listen"))
            {
                return startOn.data.listenName;
            }
            else
            {
                return EditorOp.Resolve<DataProvider>().GetEnumConditionDataValue(value);
            }
        }

        public void Import(EntityBasedComponent cdata)
        {
            OscillateComponent comp = JsonConvert.DeserializeObject<OscillateComponent>(cdata.data);
            fromPoint = comp.fromPoint;
            toPoint = comp.toPoint;
            Speed = comp.speed;
            Loop = comp.loop;
            if (EditorOp.Resolve<DataProvider>().TryGetEnum(comp.ConditionType, typeof(StartOn), out object result))
            {
                var res = (StartOn)result;
                if (res == StartOn.OnPlayerCollide)
                {
                    if (comp.ConditionData.Equals("Player"))
                    {
                        startOn.data.startIndex = (int)res;
                    }
                    else
                    {
                        startOn.data.startIndex = (int)StartOn.OnObjectCollide;
                    }
                }
                else
                {
                    startOn.data.startIndex = (int)(StartOn)result;
                }
                startOn.data.startName = res.ToString();
            }
            startOn.data.listenName = comp.ConditionData;
            var listenString = "";
            if (startOn.data.startIndex == 3)
                listenString = startOn.data.listenName;
            EditorOp.Resolve<UILogicDisplayProcessor>().ImportVisualisation(gameObject, GetType().Name, null, listenString);
        }
    }
}