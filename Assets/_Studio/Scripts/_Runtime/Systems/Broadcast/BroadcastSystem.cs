using UnityEngine;
using Leopotam.EcsLite;

namespace Terra.Studio
{
    public class BroadcastSystem : IEcsInitSystem, IConditionalOp
    {
        public void Init(IEcsSystems systems)
        {
            var filter = systems.GetWorld().Filter<BroadcastComponent>().End();
            var broadCastPool = systems.GetWorld().GetPool<BroadcastComponent>();
            foreach (var entity in filter)
            {
                ref var broadcastable = ref broadCastPool.Get(entity);
                if (broadcastable.IsExecuted || !broadcastable.IsConditionAvailable)
                {
                    broadcastable.CanExecute = true;
                    return;
                }
                var conditionalData = broadcastable.ConditionData;
                var broadcastMsg = broadcastable.Broadcast;
                ComponentsData.GetSystemForCondition(conditionalData, (obj) =>
                {
                    OnConditionalCheck(broadcastMsg);
                }, true);
            }
        }

        public void OnConditionalCheck(object data)
        {
            var msg = (string)data;
            var actions = Interop<RuntimeInterop>.Current.Resolve<ConditionHolder>().Get(msg);
            foreach (var action in actions)
            {
                action?.Invoke();
            }
        }
    }
}
