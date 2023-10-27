using Newtonsoft.Json;

namespace Terra.Studio
{
    [EditorDrawComponent("Terra.Studio.InGameTimer")]
    public class InGameTimer : BaseBehaviour
    {
        public uint Time = 180;
        public Atom.Broadcast broadcastData = new();

        public override string ComponentName => nameof(InGameTimer);
        public override bool CanPreview => false;
        protected override bool CanBroadcast => true;
        protected override bool CanListen => false;
        protected override string[] BroadcasterRefs => new string[]
        {
            broadcastData.broadcast
        };

        protected override void Awake()
        {
            base.Awake();
            broadcastData.Setup(gameObject, this);
            var timer = EditorOp.Resolve<SceneDataHandler>().TimerManagerObj;
            if (timer)
            {
                if (timer.activeSelf)
                {
                    Destroy(gameObject);
                    return;
                }
            }
            EditorOp.Resolve<SceneDataHandler>().TimerManagerObj = gameObject;
        }

        public override (string type, string data) Export()
        {
            InGameTimerComponent comp = new()
            {
                IsConditionAvailable = true,
                ConditionType = "Terra.Studio.GameStart",
                ConditionData = "OnStart",
                IsBroadcastable = !string.IsNullOrEmpty(broadcastData.broadcast),
                Broadcast = broadcastData.broadcast,
                totalTime = Time
            };
            var type = EditorOp.Resolve<DataProvider>().GetCovariance(this);
            var data = JsonConvert.SerializeObject(comp);
            return (type, data);
        }

        public override void Import(EntityBasedComponent data)
        {
            var comp = JsonConvert.DeserializeObject<InGameTimerComponent>(data.data);
            Time = comp.totalTime;
            broadcastData.broadcast = comp.Broadcast;
            ImportVisualisation(broadcastData.broadcast, null);
        }
    }
}
