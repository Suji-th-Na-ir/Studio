using UnityEngine;
using Terra.Studio;
using Newtonsoft.Json;

namespace RuntimeInspectorNamespace
{
    [EditorDrawComponent("Terra.Studio.GameTimer")]
    public class InGameTimer : MonoBehaviour, IComponent
    {
        public uint Time;
        public string Broadcast;

        public (string type, string data) Export()
        {
            GameTimerComponent component = new()
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
            var comp = JsonConvert.DeserializeObject<GameTimerComponent>($"{data.data}");
            Time = comp.totalTime;
            Broadcast = comp.Broadcast;
        }
    }
}
