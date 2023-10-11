using System;
using UnityEngine;

namespace Terra.Studio
{
    public class BehaviourPreview
    {
        private bool currentState;
        private Type currentType;

        public void Preview<T>(T instance) where T : BaseBehaviour
        {
            var type = instance.GetType();
            if (currentState)
            {
                if (type != currentType)
                {
                    Debug.Log($"Cannot preview because the last preview is still in progress");
                    return;
                }
                currentType = null;
            }
            else
            {
                currentType = type;
            }
            SwapState();
            EditorOp.Resolve<SceneView>().OnAnimationDone = () => { ToggleState(instance); };
        }

        private void SwapState()
        {
            currentState = !currentState;
            if (currentState)
            {
                OnEnable();
            }
            else
            {
                OnDisable();
            }
        }

        private void ToggleState<T>(T instance) where T : BaseBehaviour
        {
            EditorOp.Resolve<SceneView>().OnAnimationDone = null;
            PseudoRuntime<T>.GenerateFor(instance, currentState);
        }

        private void OnEnable()
        {
            EditorOp.Resolve<EditorSystem>().RequestIncognitoMode(true);
            EditorOp.Resolve<SelectionHandler>().ToggleGizmo(false);
            EditorOp.Resolve<SceneView>().TogglePreviewAnim(true);
        }

        private void OnDisable()
        {
            EditorOp.Resolve<EditorSystem>().RequestIncognitoMode(false);
            EditorOp.Resolve<SelectionHandler>().ToggleGizmo(true);
            EditorOp.Resolve<SceneView>().TogglePreviewAnim(false);
        }
    }
}
