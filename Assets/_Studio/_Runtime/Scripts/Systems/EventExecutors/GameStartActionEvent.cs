using UnityEngine.Scripting;

namespace Terra.Studio
{
    [Preserve]
    [EventExecutor("Terra.Studio.GameStart")]
    public struct GameStartActionEvent : IEventExecutor
    {
        public readonly void Execute(bool subscribe, EventContext context)
        {
            RuntimeOp.Resolve<GameStateHandler>().SubscribeToGameStart(subscribe, context.onConditionMet);
        }
    }
}
