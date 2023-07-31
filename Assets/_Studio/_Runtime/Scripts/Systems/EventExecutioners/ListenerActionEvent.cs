using System;
using UnityEngine;
using UnityEngine.Scripting;

namespace Terra.Studio
{
    [Preserve]
    [EventExecutor("Terra.Studio.Listener")]
    public struct ListenerActionEvent : IEventExecutor
    {
        public void Execute(Action<object> onConditionalCheck, bool subscribe, object conditionalCheck = null)
        {
            var broadcaster = RuntimeOp.Resolve<Broadcaster>();
            var tuple = ((GameObject go, string condition))conditionalCheck;
            if (subscribe)
            {
                broadcaster.ListenTo(tuple.condition, () => { onConditionalCheck?.Invoke(null); });
            }
            else
            {
                broadcaster.StopListenTo(tuple.condition, () => { onConditionalCheck?.Invoke(null); });
            }
        }
    }
}
