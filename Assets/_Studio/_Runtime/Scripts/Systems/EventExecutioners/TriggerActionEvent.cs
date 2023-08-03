using System;
using UnityEngine;
using UnityEngine.Scripting;

namespace Terra.Studio
{
    [Preserve]
    [EventExecutor("Terra.Studio.TriggerAction")]
    public struct TriggerActionEvent : IEventExecutor
    {
        public readonly void Execute(Action<object> onConditionalCheck, bool subscribe, object conditionalCheck = null)
        {
            if (conditionalCheck == null)
            {
                return;
            }
            var (goRef, tagCheck) = ((GameObject, string))conditionalCheck;
            var go = goRef;
            if (subscribe)
            {
                var triggerAction = go.AddComponent<OnTriggerAction>();
                triggerAction.TagAgainst = tagCheck;
                triggerAction.OnTriggered = () =>
                {
                    triggerAction.OnTriggered = null;
                    onConditionalCheck?.Invoke(null);
                };
            }
            else if (go.TryGetComponent(out OnTriggerAction triggerAction1))
            {
                UnityEngine.Object.Destroy(triggerAction1);
            }
        }
    }
}
