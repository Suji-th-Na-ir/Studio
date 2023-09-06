using Newtonsoft.Json;
using PlayShifu.Terra;
using Terra.Studio;
using UnityEngine;
using System;

namespace RuntimeInspectorNamespace
{
    [EditorDrawComponent("Terra.Studio.Oscillate")]
    public class Oscillate : MonoBehaviour, IComponent
    {
        [HideInInspector]
        public OscillateComponent Component;

        public Atom.StartOn startOn = new();
        public Vector3 fromPoint;
        public Vector3 toPoint;
        public float Speed = 1f;
        public bool Loop = false;
        
        private void Awake()
        {
            startOn.Setup(gameObject, Helper.GetEnumValuesAsStrings<StartOn>(), this.GetType().Name);
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
            
            Component.IsConditionAvailable = GetStartEvent() != "";
            Component.ConditionType = GetStartEvent();
            Component.ConditionData = GetStartCondition();
            Component.BroadcastListen = string.IsNullOrEmpty(startOn.data.listenName) ? "None" : startOn.data.listenName;

            
            gameObject.TrySetTrigger(false, true);
            var data = JsonConvert.SerializeObject(Component);
            return (type, data);
        }

        public string GetStartEvent(string _input = null)
        {
            int index = startOn.data.startIndex;
            string inputString = ((StartOn)index).ToString();
            
            if (!string.IsNullOrEmpty(_input))
                inputString = _input;
            
            if (Enum.TryParse(inputString, out StartOn enumValue))
            {
                var eventName = EditorOp.Resolve<DataProvider>().GetEnumValue(enumValue);
                return eventName;
            }
            return EditorOp.Resolve<DataProvider>().GetEnumValue(StartOn.OnClick);
        }


        public string GetStartCondition(string _input = null)
        {
            int index = startOn.data.startIndex;
            string inputString = ((StartOn)index).ToString();
            if (!string.IsNullOrEmpty(_input))
                inputString = _input;
            
            if (inputString.ToLower().Contains("listen"))
            {
                return string.IsNullOrEmpty(startOn.data.listenName) ? "None" : startOn.data.listenName;
            }
            else
            {
                if (Enum.TryParse(inputString, out StartOn enumValue))
                {
                    return EditorOp.Resolve<DataProvider>().GetEnumConditionDataValue(enumValue);
                }
                return EditorOp.Resolve<DataProvider>().GetEnumConditionDataValue(StartOn.GameStart);
            }
        }


        public void Import(EntityBasedComponent cdata)
        {
            OscillateComponent comp = JsonConvert.DeserializeObject<OscillateComponent>($"{cdata.data}");
            fromPoint = comp.fromPoint;
            toPoint = comp.toPoint;
            Speed = comp.speed;
            Loop = comp.loop;

            if (EditorOp.Resolve<DataProvider>().TryGetEnum(comp.ConditionType, typeof(StartOn), out object result))
            {
                startOn.data.startIndex = (int)(StartOn)result;
            }
            
            startOn.data.startName = comp.ConditionType;
            startOn.data.listenName = (comp.ConditionData == "None")? "" : comp.ConditionData;
            
            EditorOp.Resolve<UILogicDisplayProcessor>().ImportVisualisation(gameObject, this.GetType().Name, null, startOn.data.listenName);
        }
    }
}