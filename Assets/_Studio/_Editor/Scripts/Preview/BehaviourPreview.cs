using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Terra.Studio
{
    public class BehaviourPreview
    {
        public const float CUSTOM_TIME_DELATION = 1f;
        private const string PREVIEW_PREFAB_RESOURCE_PATH = "Prefabs/PreviewCanvas";

        private bool currentState;
        private Type currentType;
        private GameObject previewUIGO;
        private object instance;

        public void Preview<T>(T instance) where T : BaseBehaviour
        {
            var type = instance.GetType();
            if (currentState)
            {
                if (type != currentType) return;
                currentType = null;
                this.instance = null;
                HandleRuntimeStates(instance);
            }
            else
            {
                currentType = type;
                this.instance = instance;
            }
            SwapState();
            EditorOp.Resolve<SceneView>().OnAnimationDone = () =>
            {
                ToggleState(instance);
            };
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
            TogglePreviewUI();
        }

        public void Restart<T>(T instance) where T : BaseBehaviour
        {
            if (!currentState) return;
            if (instance.GetType() != currentType)
            {
                Debug.Log($"Cannot restart because the last preview is still in progress");
                return;
            }
            SystemOp.Resolve<PseudoRuntime<T>>().OnRestartRequested();
        }

        private void ToggleState<T>(T instance) where T : BaseBehaviour
        {
            EditorOp.Resolve<SceneView>().OnAnimationDone = null;
            PseudoRuntime<T>.GenerateFor(instance, currentState);
            if (currentState)
            {
                TogglePreviewUI();
                HandleRuntimeStates(instance);
            }
        }

        private void TogglePreviewUI()
        {
            var behaviour = (BaseBehaviour)instance;
            if (currentState)
            {
                var obj = EditorOp.Load<GameObject>(PREVIEW_PREFAB_RESOURCE_PATH);
                previewUIGO = Object.Instantiate(obj);
                var previewUI = previewUIGO.AddComponent<BehaviourPreviewUI>();
                previewUI.Init(behaviour);
            }
            else
            {
                Object.Destroy(previewUIGO);
            }
        }

        private void HandleRuntimeStates<T>(T _) where T : BaseBehaviour
        {
            var previewUI = EditorOp.Resolve<BehaviourPreviewUI>();
            if (currentState)
            {
                SystemOp.Resolve<PseudoRuntime<T>>().OnRuntimeInitialized += previewUI.ToggleToEventActionGroup;
                SystemOp.Resolve<PseudoRuntime<T>>().OnEventsExecuted += previewUI.ToggleToPropertiesGroup;
                SystemOp.Resolve<PseudoRuntime<T>>().OnBroadcastExecuted += previewUI.ToggleToBroadcastGroup;
            }
            else
            {
                SystemOp.Resolve<PseudoRuntime<T>>().OnRuntimeInitialized -= previewUI.ToggleToEventActionGroup;
                SystemOp.Resolve<PseudoRuntime<T>>().OnEventsExecuted -= previewUI.ToggleToPropertiesGroup;
                SystemOp.Resolve<PseudoRuntime<T>>().OnBroadcastExecuted -= previewUI.ToggleToBroadcastGroup;
            }
        }

        public class Constants
        {
            public const string SFX_PREVIEW_NAME = "SFX";
            public const string VFX_PREVIEW_NAME = "VFX";
            public const string BROADCAST_PREVIEW_KEY = "Broadcast_Key";
        }
    }
}
