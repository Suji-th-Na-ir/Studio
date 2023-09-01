using System;
using UnityEngine.Scripting;

namespace Terra.Studio
{
    [Preserve]
    [EventExecutor("Terra.Studio.MouseAction")]
    public struct MouseActionEvent : IEventExecutor
    {
        public readonly void Execute(Action<object> onConditionalCheck, bool subscribe, EventExecutorData _)
        {
            var mouseEvents = RuntimeOp.Resolve<RuntimeSystem>() as IMouseEvents;
            if (subscribe)
            {
                mouseEvents.OnClicked += onConditionalCheck;
            }
            else
            {
                mouseEvents.OnClicked -= onConditionalCheck;
            }
        }
    }
}
