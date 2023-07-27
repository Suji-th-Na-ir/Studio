using System;
using UnityEngine;
using UnityEngine.Scripting;

namespace Terra.Studio
{
    [Preserve]
    [EventExecutor("Terra.Studio.TriggerAction")]
    public struct TriggerActionEvent : IEventExecutor
    {
        public void Execute(Action<object> onConditionalCheck, bool subscribe, object conditionalCheck = null)
        {
            if (conditionalCheck == null)
            {
                return;
            }
            var tuple = ((GameObject goRef, string tagCheck))conditionalCheck;
            var go = tuple.goRef;
            if (subscribe)
            {
                var triggerAction = go.AddComponent<OnTriggerAction>();
                triggerAction.TagAgainst = tuple.tagCheck;
                triggerAction.onTriggered = () =>
                {
                    triggerAction.onTriggered = null;
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
