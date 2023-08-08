using System;
using UnityEngine.Scripting;

namespace Terra.Studio
{
    [Preserve]
    [EventExecutor("Terra.Studio.GameStart")]
    public struct GameStartActionEvent : IEventExecutor
    {
        public readonly void Execute(Action<object> onConditionalCheck, bool subscribe, object conditionalCheck = null)
        {
            RuntimeOp.Resolve<GameStateHandler>().SubscribeToGameStart(subscribe, onConditionalCheck);
        }
    }
}
