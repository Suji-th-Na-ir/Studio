using UnityEngine;
using Newtonsoft.Json;
using RuntimeInspectorNamespace;

namespace Terra.Studio
{
    [EditorDrawComponent("Terra.Studio.GameScore")]
    public class GameScore : MonoBehaviour, IComponent
    {
        public int targetScore = 0;
        public string broadcast = "Game Win";

        private void Awake()
        {
            var score = EditorOp.Resolve<SceneDataHandler>().ScoreManagerObj;
            if (score)
            {
                Destroy(gameObject);
            }
            else
            {
                EditorOp.Resolve<SceneDataHandler>().ScoreManagerObj = gameObject;
            }
        }

        public (string type, string data) Export()
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

        public void Import(EntityBasedComponent data)
        {
            var obj = JsonConvert.DeserializeObject<InGameScoreComponent>(data.data);
            targetScore = obj.targetScore;
            broadcast = obj.Broadcast;
        }
    }
}