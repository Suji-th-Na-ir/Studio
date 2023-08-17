using TMPro;
using UnityEngine;
using System.Collections;

namespace Terra.Studio
{
    public class PostGameView : View
    {
        private const float WAIT_TIME_FOR_UI = 1f;

        [SerializeField] private Animator animator;
        [SerializeField] private GameObject tryAgainGO;
        [SerializeField] private GameObject winGO;
        [SerializeField] private TextMeshProUGUI[] scoreTexts;

        private bool hasWon;

        public override void Init()
        {
            var state = RuntimeOp.Resolve<GameData>().EndState;
            hasWon = state == GameEndState.Win;
            StartCoroutine(WaitAndShowUI());
        }

        private IEnumerator WaitAndShowUI()
        {
            animator.Play("FadeIn");
            yield return new WaitForSeconds(WAIT_TIME_FOR_UI);
            Draw();
        }

        public override void Draw()
        {
            tryAgainGO.SetActive(!hasWon);
            winGO.SetActive(hasWon);
            if (RuntimeOp.Resolve<ScoreHandler>() == null)
            {
                if (scoreTexts != null && scoreTexts.Length != 0)
                {
                    foreach (var scoreText in scoreTexts)
                    {
                        scoreText.gameObject.SetActive(false);
                    }
                }
            }
            else
            {
                var score = RuntimeOp.Resolve<ScoreHandler>().CurrentScore;
                if (scoreTexts != null && scoreTexts.Length != 0)
                {
                    foreach (var scoreText in scoreTexts)
                    {
                        if (scoreText)
                        {
                            scoreText.text = $"Score: {score}";
                        }
                    }
                }
            }
        }

        public override void Flush()
        {
            //Nothing to flush
        }

        public override void Repaint()
        {
            //Nothing to repaint
        }
    }
}
