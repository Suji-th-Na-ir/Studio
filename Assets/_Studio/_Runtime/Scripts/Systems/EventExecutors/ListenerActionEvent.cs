using UnityEngine.Scripting;

namespace Terra.Studio
{
    [Preserve]
    [EventExecutor("Terra.Studio.Listener")]
    public struct ListenerActionEvent : IEventExecutor
    {
        public readonly void Execute(bool subscribe, EventContext context)
        {
            var broadcaster = RuntimeOp.Resolve<Broadcaster>();
            if (subscribe)
            {
                broadcaster.ListenTo(context.conditionData, context.onConditionMet);
            }
            else
            {
                broadcaster.StopListenTo(context.conditionData, context.onConditionMet);
            }
        }
    }
}
