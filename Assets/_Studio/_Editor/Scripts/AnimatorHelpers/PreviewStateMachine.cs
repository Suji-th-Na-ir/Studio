using UnityEngine;

namespace Terra.Studio
{
    public class PreviewStateMachine : StateMachineBehaviour
    {
        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            EditorOp.Resolve<SceneView>().OnAnimationDone?.Invoke();
        }
    }
}
