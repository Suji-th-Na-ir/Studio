using System;
using UnityEngine;
using UnityEngine.Scripting;

namespace Terra.Studio
{
    [Preserve]
    [EventExecutor("Terra.Studio.Listener")]
    public struct ListenerActionEvent : IEventExecutor
    {
        public readonly void Execute(Action<object> onConditionalCheck, bool subscribe, object conditionalCheck = null)
        {
            var broadcaster = RuntimeOp.Resolve<Broadcaster>();
            var (_, condition) = ((GameObject go, string condition))conditionalCheck;
            if (subscribe)
            {
                broadcaster.ListenTo(condition, onConditionalCheck);
            }
            else
            {
                broadcaster.StopListenTo(condition, onConditionalCheck);
            }
        }
    }
}
