using System;
using UnityEngine.Scripting;

namespace Terra.Studio
{
    [Preserve]
    [EventExecutor("Terra.Studio.TriggerAction")]
    public struct TriggerActionEvent : IEventExecutor
    {
        public readonly void Execute(Action<object> onConditionalCheck, bool subscribe, EventExecutorData conditionalCheck)
        {
            if (conditionalCheck.Equals(default(EventExecutorData)))
            {
                return;
            }
            if (subscribe)
            {
                var triggerAction = conditionalCheck.goRef.AddComponent<OnTriggerAction>();
                triggerAction.TagAgainst = conditionalCheck.data;
                triggerAction.OnTriggered = () =>
                {
                    onConditionalCheck?.Invoke(null);
                };
            }
            else if (conditionalCheck.goRef && conditionalCheck.goRef.TryGetComponent(out OnTriggerAction triggerAction))
            {
                triggerAction.OnTriggered = null;
                UnityEngine.Object.Destroy(triggerAction);
            }
        }
    }
}
