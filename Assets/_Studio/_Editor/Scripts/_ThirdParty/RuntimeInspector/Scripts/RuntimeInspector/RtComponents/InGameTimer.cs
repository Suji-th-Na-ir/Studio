using UnityEngine;
using Terra.Studio;
using Newtonsoft.Json;

namespace RuntimeInspectorNamespace
{
    [EditorDrawComponent("Terra.Studio.InGameTimer")]
    public class InGameTimer : MonoBehaviour, IComponent
    {
        public uint Time = 180;
        public string Broadcast = "";
        private string guid;

        private void Awake()
        {
            guid = GetInstanceID() + "_timer";
        }

        public (string type, string data) Export()
        {
            InGameTimerComponent comp = new()
            {
                IsConditionAvailable = true,
                ConditionType = "Terra.Studio.GameStart",
                ConditionData = "OnStart",
                IsBroadcastable = !string.IsNullOrEmpty(Broadcast),
                Broadcast = string.IsNullOrEmpty(Broadcast) ? "None" : Broadcast,
                // broadcastTypeIndex = Broadcast.data.broadcastTypeIndex,
                totalTime = Time
            };
            var type = EditorOp.Resolve<DataProvider>().GetCovariance(this);
            var data = JsonConvert.SerializeObject(comp);
            return (type, data);
        }

        public void Import(EntityBasedComponent data)
        {
            var comp = JsonConvert.DeserializeObject<InGameTimerComponent>($"{data.data}");
            Time = comp.totalTime;
           
            Broadcast = string.IsNullOrEmpty(comp.Broadcast) ? "None" : comp.Broadcast;
            // Broadcast.data.broadcastTypeIndex = comp.broadcastTypeIndex;
            EditorOp.Resolve<UILogicDisplayProcessor>().ImportVisualisation(gameObject, this.GetType().Name, Broadcast, null);
        }
    }
}
