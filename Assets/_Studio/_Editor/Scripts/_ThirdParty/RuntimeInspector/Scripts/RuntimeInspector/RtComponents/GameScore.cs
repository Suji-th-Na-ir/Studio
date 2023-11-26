using Newtonsoft.Json;

namespace Terra.Studio
{
    [EditorDrawComponent("Terra.Studio.GameScore")]
    public class GameScore : BaseBehaviour
    {
        public int targetScore = 0;
        public Atom.Broadcast broadcastData = new();

        public override string ComponentName => nameof(GameScore);
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
            broadcastData.broadcast = "Game Win";
            broadcastData.Setup(gameObject, this);
            var score = EditorOp.Resolve<SceneDataHandler>().ScoreManagerObj;
            if (score)
            {
                if (score.activeSelf)
                {
                    Destroy(gameObject);
                    return;
                }
            }
            EditorOp.Resolve<SceneDataHandler>().ScoreManagerObj = gameObject;
        }

        public override (string type, string data) Export()
        {
            var compData = new InGameScoreComponent()
            {
                targetScore = targetScore,
                IsBroadcastable = !string.IsNullOrEmpty(broadcastData.broadcast) && targetScore != 0,
                Broadcast = broadcastData.broadcast,
                ConditionType = "Terra.Studio.GameStart",
                ConditionData = "OnStart",
                Listen = Listen.Once
            };
            var json = JsonConvert.SerializeObject(compData);
            var type = EditorOp.Resolve<DataProvider>().GetCovariance(this);
            return (type, json);
        }

        public override void Import(EntityBasedComponent data)
        {
            var obj = JsonConvert.DeserializeObject<InGameScoreComponent>(data.data);
            targetScore = obj.targetScore;
            broadcastData.broadcast = obj.Broadcast;
            ImportVisualisation(broadcastData.broadcast, null);
        }
    }
}