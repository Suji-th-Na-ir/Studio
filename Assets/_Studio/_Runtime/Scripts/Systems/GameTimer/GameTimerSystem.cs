using System;
using UnityEngine;
using Leopotam.EcsLite;
using System.Collections.Generic;

namespace Terra.Studio
{
    public class GameTimerSystem : BaseSystem, IEcsRunSystem
    {
        public override Dictionary<int, Action<object>> IdToConditionalCallback { get; set; }

        public override void Init(EcsWorld currentWorld, int entity)
        {
            var pool = currentWorld.GetPool<GameTimerComponent>();
            ref var entityRef = ref pool.Get(entity);
            entityRef.CanExecute = false;
            var conditionData = entityRef.ConditionData;
            var conditionType = entityRef.ConditionType;
            object data = null;
            if (entityRef.IsBroadcastable)
            {
                RuntimeOp.Resolve<Broadcaster>().SetBroadcastable(entityRef.Broadcast);
            }
            RuntimeOp.Resolve<CoreGameManager>().EnableModule<InGameTimeHandler>();
            IdToConditionalCallback ??= new();
            IdToConditionalCallback.Add(entity, (obj) =>
            {
                OnConditionalCheck((entity, conditionData, conditionType));
            });
            RuntimeOp.Resolve<ComponentsData>().ProvideEventContext(conditionType, IdToConditionalCallback[entity], true, (data, conditionData));
        }

        public override void OnConditionalCheck(object data)
        {
            object temp = null;
            var (entity, conditionData, conditionType) = ((int, string, string))data;
            var compData = RuntimeOp.Resolve<ComponentsData>();
            compData.ProvideEventContext(conditionType, IdToConditionalCallback[entity], false, (temp, conditionData));
            IdToConditionalCallback.Remove(entity);
            var world = RuntimeOp.Resolve<RuntimeSystem>().World;
            var pool = world.GetPool<GameTimerComponent>();
            ref var timer = ref pool.Get(entity);
            timer.CanExecute = true;
            RuntimeOp.Resolve<InGameTimeHandler>().UpdateTime(timer.totalTime);
        }

        public void Run(IEcsSystems systems)
        {
            var filter = systems.GetWorld().Filter<GameTimerComponent>().End();
            var timerPool = systems.GetWorld().GetPool<GameTimerComponent>();
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
