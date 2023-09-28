using UnityEngine;
//using Terra.Studio;
//using Newtonsoft.Json;
//using PlayShifu.Terra;

namespace RuntimeInspectorNamespace
{
    //[EditorDrawComponent("Terra.Studio.Oscillate")]
    public class Oscillate : MonoBehaviour
    {
        //[HideInInspector]
        //public OscillateComponent Component;
        //[AliasDrawer("OscillateWhen")]
        //public Atom.StartOn StartOn = new();
        //public Vector3 fromPoint;
        //public Vector3 toPoint;
        //public float Speed = 1f;
        //public bool Loop = false;

        //protected override string ComponentName => nameof(Oscillate);
        //protected override bool CanBroadcast => true;
        //protected override bool CanListen => true;

        //protected override void Awake()
        //{
        //    base.Awake();
        //    StartOn.Setup<StartOn>(gameObject, ComponentName, StartOn.data.startIndex == 4, DisplayDock, OnListenerUpdated);
        //    fromPoint = transform.localPosition;
        //    Component.fromPoint = fromPoint;
        //}

        //public override (string type, string data) Export()
        //{
        //    var type = EditorOp.Resolve<DataProvider>().GetCovariance(this);
        //    Component.fromPoint = fromPoint;
        //    Component.toPoint = toPoint;
        //    Component.loop = Loop;
        //    Component.speed = Speed;
        //    Component.IsConditionAvailable = !string.IsNullOrEmpty(GetStartEvent());
        //    Component.ConditionType = GetStartEvent();
        //    Component.ConditionData = GetStartCondition();
        //    gameObject.TrySetTrigger(false, true);
        //    var data = JsonConvert.SerializeObject(Component);
        //    return (type, data);
        //}

        //public string GetStartEvent()
        //{
        //    int index = StartOn.data.startIndex;
        //    var value = (StartOn)index;
        //    var eventName = EditorOp.Resolve<DataProvider>().GetEnumValue(value);
        //    return eventName;
        //}


        //public string GetStartCondition()
        //{
        //    int index = StartOn.data.startIndex;
        //    var value = (StartOn)index;
        //    string inputString = value.ToString();
        //    if (inputString.ToLower().Contains("listen"))
        //    {
        //        return StartOn.data.listenName;
        //    }
        //    else
        //    {
        //        return EditorOp.Resolve<DataProvider>().GetEnumConditionDataValue(value);
        //    }
        //}

        //public override void Import(EntityBasedComponent cdata)
        //{
        //    OscillateComponent comp = JsonConvert.DeserializeObject<OscillateComponent>(cdata.data);
        //    fromPoint = comp.fromPoint;
        //    toPoint = comp.toPoint;
        //    Speed = comp.speed;
        //    Loop = comp.loop;
        //    if (EditorOp.Resolve<DataProvider>().TryGetEnum(comp.ConditionType, typeof(StartOn), out object result))
        //    {
        //        var res = (StartOn)result;
        //        if (res == Terra.Studio.StartOn.OnPlayerCollide)
        //        {
        //            if (comp.ConditionData.Equals("Player"))
        //            {
        //                StartOn.data.startIndex = (int)res;
        //            }
        //            else
        //            {
        //                StartOn.data.startIndex = (int)Terra.Studio.StartOn.OnObjectCollide;
        //            }
        //        }
        //        else
        //        {
        //            StartOn.data.startIndex = (int)(StartOn)result;
        //        }
        //        StartOn.data.startName = res.ToString();
        //    }
        //    StartOn.data.listenName = comp.ConditionData;
        //    var listenString = "";
        //    if (StartOn.data.startIndex == 4)
        //        listenString = StartOn.data.listenName;
        //    EditorOp.Resolve<UILogicDisplayProcessor>().ImportVisualisation(gameObject, GetType().Name, null, listenString);
        //}
    }
}