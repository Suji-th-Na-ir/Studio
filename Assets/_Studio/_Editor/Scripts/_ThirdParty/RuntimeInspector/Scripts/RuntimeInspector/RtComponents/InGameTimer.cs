using UnityEngine;
using Terra.Studio;
using Newtonsoft.Json;
using System;
using PlayShifu.Terra;

namespace RuntimeInspectorNamespace
{
    [EditorDrawComponent("Terra.Studio.InGameTimer")]
    public class InGameTimer : MonoBehaviour, IComponent
    {
        public uint Time;
        public string Broadcast;
        
        public void Update()
        {
            if (!String.IsNullOrEmpty(Broadcast))
            {
                Helper.UpdateListenToTypes(this.GetInstanceID()+"_timer", Broadcast);
            }
        }

        public (string type, string data) Export()
        {
            InGameTimerComponent component = new()
            {
                IsConditionAvailable = true,
                ConditionType = "Terra.Studio.GameStart",
                ConditionData = "OnStart",
                IsBroadcastable = !string.IsNullOrEmpty(Broadcast),
                Broadcast = Broadcast,
                totalTime = Time
            };
            var type = EditorOp.Resolve<DataProvider>().GetCovariance(this);
            var data = JsonConvert.SerializeObject(component);
            return (type, data);
        }

        public void Import(EntityBasedComponent data)
        {
            var comp = JsonConvert.DeserializeObject<InGameTimerComponent>($"{data.data}");
            Time = comp.totalTime;
            Broadcast = comp.Broadcast;
        }
    }
}
