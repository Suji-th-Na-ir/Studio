using TMPro;
using UnityEngine;

namespace Terra.Studio
{
    public class InGameView : View
    {
        [SerializeField]
        private TextMeshProUGUI text;

        public override void Init()
        {
            RuntimeOp.Resolve<ScoreHandler>().OnScoreModified += (_) => { Draw(); };
        }

        public override void Draw()
        {
            var score = RuntimeOp.Resolve<ScoreHandler>().CurrentScore;
            text.text = score.ToString();
        }

        public override void Flush()
        {

        }

        public override void Repaint()
        {

        }
    }
}
