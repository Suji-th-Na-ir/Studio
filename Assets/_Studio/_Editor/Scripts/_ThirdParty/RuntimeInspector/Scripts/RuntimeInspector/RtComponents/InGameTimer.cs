using UnityEngine;
using Terra.Studio;
using Newtonsoft.Json;

namespace RuntimeInspectorNamespace
{
    [EditorDrawComponent("Terra.Studio.InGameTimer")]
    public class InGameTimer : MonoBehaviour, IComponent
    {
        public uint Time = 180;
        public Atom.Broadcast Broadcast = new();
        private string guid;

        private void Awake()
        {
            guid = GetInstanceID() + "_timer";
            Broadcast.Setup(gameObject, GetType().Name, guid);
        }

        public (string type, string data) Export()
        {
            InGameTimerComponent comp = new()
            {
                IsConditionAvailable = true,
                ConditionType = "Terra.Studio.GameStart",
                ConditionData = "OnStart",
                IsBroadcastable = !string.IsNullOrEmpty(Broadcast.data.broadcastName),
                Broadcast = string.IsNullOrEmpty(Broadcast.data.broadcastName) ? "None" : Broadcast.data.broadcastName,
                broadcastTypeIndex = Broadcast.data.broadcastTypeIndex,
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
            EditorOp.Resolve<DataProvider>().AddToListenList(guid,comp.Broadcast);
            Broadcast.data.broadcastName = string.IsNullOrEmpty(comp.Broadcast) ? "None" : comp.Broadcast;
            Broadcast.data.broadcastTypeIndex = comp.broadcastTypeIndex;
            EditorOp.Resolve<UILogicDisplayProcessor>().ImportVisualisation(gameObject, this.GetType().Name, Broadcast.data.broadcastName, null);
        }
    }
}
