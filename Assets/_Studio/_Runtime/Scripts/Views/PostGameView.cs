using System.Collections;
using UnityEngine;

namespace Terra.Studio
{
    public class PostGameView : View
    {
        private const float WAIT_TIME_FOR_UI = 1f;

        [SerializeField] private Animator animator;
        [SerializeField] private GameObject tryAgainGO;
        [SerializeField] private GameObject winGO;

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
