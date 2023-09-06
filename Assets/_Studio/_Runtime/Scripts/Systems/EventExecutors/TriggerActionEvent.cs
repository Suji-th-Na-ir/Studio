using System;
using UnityEngine.Scripting;

namespace Terra.Studio
{
    [Preserve]
    [EventExecutor("Terra.Studio.TriggerAction")]
    public struct TriggerActionEvent : IEventExecutor
    {
        public readonly void Execute(Action<object> onConditionalCheck, bool subscribe, EventConditionalCheckData conditionalCheck)
        {
            if (conditionalCheck.Equals(default(EventConditionalCheckData)))
            {
                return;
            }
            if (subscribe)
            {
                var triggerAction = conditionalCheck.goRef.AddComponent<OnTriggerAction>();
                triggerAction.tagAgainst = conditionalCheck.conditionData;
                triggerAction.OnTriggered = (gameObject) =>
                {
                    onConditionalCheck?.Invoke(gameObject);
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
