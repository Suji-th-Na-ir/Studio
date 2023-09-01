using System;
using UnityEngine;
using Terra.Studio;
using Newtonsoft.Json;

namespace RuntimeInspectorNamespace
{
    [EditorDrawComponent("Terra.Studio.InGameTimer")]
    public class InGameTimer : MonoBehaviour, IComponent
    {
        public uint Time = 180;
        public string Broadcast = "Game Lose";
        private string guid;

        private void Awake()
        {
            guid = GetInstanceID() + "_timer"; //Guid.NewGuid().ToString("N");
            var timer = EditorOp.Resolve<SceneDataHandler>().TimerManagerObj;
            if (timer)
            {
                Destroy(gameObject);
            }
            else
            {
                EditorOp.Resolve<SceneDataHandler>().TimerManagerObj = gameObject;
            }
        }

        public void Update()
        {
            if (!String.IsNullOrEmpty(Broadcast))
            {
                EditorOp.Resolve<DataProvider>().UpdateListenToTypes(guid, Broadcast);
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
            EditorOp.Resolve<UILogicDisplayProcessor>().ImportVisualisation(gameObject, this.GetType().Name, Broadcast, null);
        }
    }
}
