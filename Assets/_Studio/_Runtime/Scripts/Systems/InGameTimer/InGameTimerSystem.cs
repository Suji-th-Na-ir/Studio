using System;
using UnityEngine;
using Leopotam.EcsLite;
using System.Collections.Generic;

namespace Terra.Studio
{
    public class InGameTimerSystem : BaseSystem, IEcsRunSystem
    {
        public override Dictionary<int, Action<object>> IdToConditionalCallback { get; set; }
        private EventExecutorData eventExecutorData;

        public override void Init(EcsWorld currentWorld, int entity)
        {
            var pool = currentWorld.GetPool<InGameTimerComponent>();
            ref var entityRef = ref pool.Get(entity);
            entityRef.CanExecute = false;
            var conditionData = entityRef.ConditionData;
            var conditionType = entityRef.ConditionType;
            if (entityRef.IsBroadcastable)
            {
                RuntimeOp.Resolve<Broadcaster>().SetBroadcastable(entityRef.Broadcast);
            }
            RuntimeOp.Resolve<CoreGameManager>().EnableModule<InGameTimeHandler>();
            eventExecutorData = new()
            {
                goRef = null,
                data = conditionData
            };
            IdToConditionalCallback ??= new();
            IdToConditionalCallback.Add(entity, (obj) =>
            {
                OnConditionalCheck((entity, conditionData, conditionType));
            });
            RuntimeOp.Resolve<ComponentsData>().ProvideEventContext(conditionType, IdToConditionalCallback[entity], true, eventExecutorData);
        }

        public override void OnConditionalCheck(object data)
        {
            var (entity, conditionData, conditionType) = ((int, string, string))data;
            var compData = RuntimeOp.Resolve<ComponentsData>();
            compData.ProvideEventContext(conditionType, IdToConditionalCallback[entity], false, eventExecutorData);
            IdToConditionalCallback.Remove(entity);
            var world = RuntimeOp.Resolve<RuntimeSystem>().World;
            var pool = world.GetPool<InGameTimerComponent>();
            ref var timer = ref pool.Get(entity);
            timer.CanExecute = true;
            RuntimeOp.Resolve<InGameTimeHandler>().UpdateTime(timer.totalTime);
        }

        public void Run(IEcsSystems systems)
        {
            var filter = systems.GetWorld().Filter<InGameTimerComponent>().End();
            var timerPool = systems.GetWorld().GetPool<InGameTimerComponent>();
            var totalEntitiesFinishedJob = 0;
            foreach (var entity in filter)
            {
                ref var timer = ref timerPool.Get(entity);
                if (!timer.CanExecute)
                {
                    continue;
                }
                if (timer.IsExecuted)
                {
                    ++totalEntitiesFinishedJob;
                    continue;
                }
                var time = RuntimeOp.Resolve<InGameTimeHandler>().CurrentTime;
                time -= Time.deltaTime;
                if (time <= 0f)
                {
                    timer.IsExecuted = true;
                    RuntimeOp.Resolve<Broadcaster>().Broadcast(timer.Broadcast, true);
                }
                else
                {
                    RuntimeOp.Resolve<InGameTimeHandler>().UpdateTime(time);
                }
            }
            if (totalEntitiesFinishedJob == filter.GetEntitiesCount())
            {
                RuntimeOp.Resolve<RuntimeSystem>().RemoveRunningInstance(this);
            }
        }
    }
}
