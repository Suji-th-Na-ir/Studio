using System;
using UnityEngine.Scripting;

namespace Terra.Studio
{
    [Preserve]
    [EventExecutor("Terra.Studio.GameStart")]
    public struct GameStartActionEvent : IEventExecutor
    {
        public readonly void Execute(Action<object> onConditionalCheck, bool subscribe, EventExecutorData _)
        {
            RuntimeOp.Resolve<GameStateHandler>().SubscribeToGameStart(subscribe, onConditionalCheck);
        }
    }
}
