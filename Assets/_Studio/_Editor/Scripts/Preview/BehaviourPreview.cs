using System;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Terra.Studio
{
    public class BehaviourPreview
    {
        public const float CUSTOM_TIME_DELATION = 1f;

        private const string PREVIEW_PREFAB_RESOURCE_PATH = "Prefabs/PreviewCanvas";
        private static readonly string[] MULTI_STATE_COMPONENTS = new[]
        {
            "Switch"
        };
        private bool currentState;
        private Type currentType;
        private GameObject previewUIGO;
        private BaseBehaviour instance;
        private Action cachedForceExecuteAction;

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
                cachedForceExecuteAction = SystemOp.Resolve<PseudoRuntime<T>>().ForceInitiateConditionalEvent;
            }
        }

        private void TogglePreviewUI()
        {
            if (currentState)
            {
                var obj = EditorOp.Load<GameObject>(PREVIEW_PREFAB_RESOURCE_PATH);
                previewUIGO = Object.Instantiate(obj);
                var previewUI = previewUIGO.AddComponent<BehaviourPreviewUI>();
                previewUI.Init(instance);
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
                SystemOp.Resolve<PseudoRuntime<T>>().OnEventsExecuted += OnPropertyExecuted;
                SystemOp.Resolve<PseudoRuntime<T>>().OnBroadcastExecuted += OnBroadcastExecuted;
            }
            else
            {
                SystemOp.Resolve<PseudoRuntime<T>>().OnRuntimeInitialized -= previewUI.ToggleToEventActionGroup;
                SystemOp.Resolve<PseudoRuntime<T>>().OnEventsExecuted -= OnPropertyExecuted;
                SystemOp.Resolve<PseudoRuntime<T>>().OnBroadcastExecuted -= OnBroadcastExecuted;
            }
        }

        private void OnPropertyExecuted()
        {
            EditorOp.Resolve<BehaviourPreviewUI>().ToggleToPropertiesGroup();
            if (IsMultiStateComponent())
            {
                var data = EditorOp.Resolve<BehaviourPreviewUI>().CurrentPreviewData;
                var broadcast = data.GetBroadcast();
                var isEmpty = string.IsNullOrEmpty(broadcast);
                if (isEmpty)
                {
                    CheckForNextStateInMultiStateComponent();
                }
            }
        }

        private void OnBroadcastExecuted()
        {
            EditorOp.Resolve<BehaviourPreviewUI>().ToggleToBroadcastGroup();
            if (IsMultiStateComponent())
            {
                CheckForNextStateInMultiStateComponent();
            }
        }

        private void CheckForNextStateInMultiStateComponent()
        {
            var data = EditorOp.Resolve<BehaviourPreviewUI>().CurrentPreviewData;
            var max = data.MaxProperties;
            var current = data.CurrentPropertyIndex;
            if (max - 1 != current)
            {
                CoroutineService.RunCoroutine(InvokeNextStateInMultiStateComponent,
                CoroutineService.DelayType.WaitForXSeconds, CUSTOM_TIME_DELATION);
            }
        }

        private void InvokeNextStateInMultiStateComponent()
        {
            EditorOp.Resolve<BehaviourPreviewUI>().ToggleToNextPropertyState();
            cachedForceExecuteAction?.Invoke();
        }

        private bool IsMultiStateComponent()
        {
            var behaviourName = instance.GetDisplayName();
            var doesContainMultipleStates = MULTI_STATE_COMPONENTS.Any(x => x.Equals(behaviourName));
            return doesContainMultipleStates;
        }

        public class Constants
        {
            public const string SFX_PREVIEW_NAME = "SFX";
            public const string VFX_PREVIEW_NAME = "VFX";
            public const string BROADCAST_PREVIEW_KEY = "Broadcast_Key";
        }
    }
}
