using System;
using UnityEngine.Scripting;

namespace Terra.Studio
{
    [Preserve]
    [EventExecutor("Terra.Studio.Listener")]
    public struct ListenerActionEvent : IEventExecutor
    {
        public void Execute(Action<object> onConditionalCheck, bool subscribe, object conditionalCheck = null)
        {
            var broadcaster = Interop<RuntimeInterop>.Current.Resolve<Broadcaster>();
            if (subscribe)
            {
                broadcaster.ListenTo((string)conditionalCheck, () => { onConditionalCheck?.Invoke(null); });
            }
            else
            {
                broadcaster.StopListenTo((string)conditionalCheck, () => { onConditionalCheck?.Invoke(null); });
            }
        }
    }
}
