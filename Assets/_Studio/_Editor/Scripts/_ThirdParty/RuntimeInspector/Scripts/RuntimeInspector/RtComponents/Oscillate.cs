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
            startOn.Setup(gameObject, Helper.GetEnumValuesAsStrings<StartOn>());
            fromPoint = transform.localPosition;
            Component.fromPoint = fromPoint;
        }

        public (string type, string data) Export()
        {
            var type = EditorOp.Resolve<DataProvider>().GetCovariance(this);

            Component.fromPoint = fromPoint;
            Component.toPoint = toPoint;
            Component.ConditionType = GetStartEvent();
            Component.ConditionData = GetStartCondition();
            Component.listenIndex = startOn.data.listenIndex;
            
            Component.BroadcastListen = string.IsNullOrEmpty(startOn.data.listenName) ? null : startOn.data.listenName;
            
            Component.loop = Loop;
            Component.speed = Speed;
            Component.IsConditionAvailable = GetStartEvent() != "";
            gameObject.TrySetTrigger(false, true);
            var data = JsonConvert.SerializeObject(Component);
            return (type, data);
        }

        public string GetStartEvent(string _input = null)
        {
            string inputString = startOn.data.startName;
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
            string inputString = startOn.data.startName;
            if (!string.IsNullOrEmpty(_input))
                inputString = _input;
            
            if (inputString.ToLower().Contains("listen"))
            {
                return Helper.GetListenString(startOn.data.listenIndex);
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
            OscillateComponent cc = JsonConvert.DeserializeObject<OscillateComponent>($"{cdata.data}");
            fromPoint = cc.fromPoint;
            toPoint = cc.toPoint;
            Speed = cc.speed;
            Loop = cc.loop;

            if (EditorOp.Resolve<DataProvider>().TryGetEnum(cc.ConditionType, typeof(StartOn), out object result))
            {
                startOn.data.startIndex = (int)(StartOn)result;
            }

            if (cc.ConditionType.ToLower().Contains("listen"))
            {
                Helper.AddToListenList(cc.ConditionData);
            }
            startOn.data.listenIndex = cc.listenIndex;
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