using Newtonsoft.Json;

namespace Terra.Studio
{
    [EditorDrawComponent("Terra.Studio.GameScore")]
    public class GameScore : BaseBehaviour
    {
        public int targetScore = 0;
        [AliasDrawer("Broadcast")]
        [OnValueChanged(UpdateBroadcast = true)]
        public string broadcast = "Game Win";

        protected override string ComponentName => nameof(GameScore);
        protected override bool CanBroadcast => true;
        protected override bool CanListen => false;
        protected override string[] BroadcasterRefs => new string[]
        {
            broadcast
        };

        protected override void Awake()
        {
            base.Awake();
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
                IsBroadcastable = !string.IsNullOrEmpty(broadcast) && targetScore != 0,
                Broadcast = broadcast
            };
            var json = JsonConvert.SerializeObject(compData);
            var type = EditorOp.Resolve<DataProvider>().GetCovariance(this);
            return (type, json);
        }

        public override void Import(EntityBasedComponent data)
        {
            var obj = JsonConvert.DeserializeObject<InGameScoreComponent>(data.data);
            targetScore = obj.targetScore;
            broadcast = obj.Broadcast;
            ImportVisualisation(broadcast, null);
        }
    }
}