using UnityEngine.Scripting;

namespace Terra.Studio
{
    [Preserve]
    [EventExecutor("Terra.Studio.TriggerAction")]
    public struct TriggerActionEvent : IEventExecutor
    {
        public readonly void Execute(bool subscribe, EventContext context)
        {
            if (subscribe)
            {
                var triggerAction = context.goRef.AddComponent<OnTriggerAction>();
                triggerAction.tagAgainst = context.conditionData;
                triggerAction.OnTriggered = (gameObject) =>
                {
                    context.onConditionMet?.Invoke(gameObject);
                };
            }
            else if (context.goRef && context.goRef.TryGetComponent(out OnTriggerAction triggerAction))
            {
                triggerAction.OnTriggered = null;
                UnityEngine.Object.Destroy(triggerAction);
            }
        }
    }
}
