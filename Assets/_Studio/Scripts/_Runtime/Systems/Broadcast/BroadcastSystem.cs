using UnityEngine;
using Leopotam.EcsLite;

namespace Terra.Studio
{
    public class BroadcastSystem : IAbsRunsystem, IConditionalOp
    {
        public void Init(EcsWorld currentWorld)
        {
            var filter = currentWorld.Filter<BroadcastComponent>().End();
            var broadCastPool = currentWorld.GetPool<BroadcastComponent>();
            foreach (var entity in filter)
            {
                ref var broadcastable = ref broadCastPool.Get(entity);
                if (broadcastable.IsExecuted)
                {
                    broadcastable.CanExecute = true;
                    return;
                }
                if (broadcastable.isRegistered)
                {
                    continue;
                }
                var conditionType = broadcastable.ConditionType;
                var broadcastMsg = broadcastable.Broadcast;
                broadcastable.isRegistered = true;
                ComponentsData.GetSystemForCondition(conditionType, (obj) =>
                {
                    OnConditionalCheck((entity, broadcastMsg, obj));
                }, true);
            }
        }

        public void OnConditionalCheck(object data)
        {
            var tuple = ((int entity, string msg, object obj))data;
            if (tuple.obj is GameObject)
            {
                var world = Interop<RuntimeInterop>.Current.Resolve<RuntimeSystem>().World;
                var filter = world.Filter<BroadcastComponent>().End();
                var oscillatorPool = world.GetPool<BroadcastComponent>();
                ref var broadcastable = ref oscillatorPool.Get(tuple.entity);
                if (broadcastable.reference != tuple.obj as GameObject)
                {
                    return;
                }
            }
            var actions = Interop<RuntimeInterop>.Current.Resolve<ConditionHolder>().Get(tuple.msg);
            if (actions == null)
            {
                return;
            }
            foreach (var action in actions)
            {
                action?.Invoke(tuple.obj);
            }
        }
    }
}
