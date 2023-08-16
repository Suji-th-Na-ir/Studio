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
            RuntimeOp.Resolve<ScoreHandler>().OnScoreModified += SetScore;
        }

        public override void Draw()
        {
            var score = RuntimeOp.Resolve<ScoreHandler>().CurrentScore;
            SetScore(score);
        }

        public override void Flush()
        {

        }

        public override void Repaint()
        {

        }

        private void SetScore(int currentScore)
        {
            text.text = currentScore.ToString();
        }
    }
}
