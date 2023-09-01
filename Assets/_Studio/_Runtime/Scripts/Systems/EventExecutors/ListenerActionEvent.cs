using System;
using UnityEngine.Scripting;

namespace Terra.Studio
{
    [Preserve]
    [EventExecutor("Terra.Studio.Listener")]
    public struct ListenerActionEvent : IEventExecutor
    {
        public readonly void Execute(Action<object> onConditionalCheck, bool subscribe, EventExecutorData conditionalCheck)
        {
            var broadcaster = RuntimeOp.Resolve<Broadcaster>();
            if (subscribe)
            {
                broadcaster.ListenTo(conditionalCheck.data, onConditionalCheck);
            }
            else
            {
                broadcaster.StopListenTo(conditionalCheck.data, onConditionalCheck);
            }
        }
    }
}
