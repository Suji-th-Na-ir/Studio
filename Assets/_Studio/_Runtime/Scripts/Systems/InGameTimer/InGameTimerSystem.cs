using UnityEngine;
using Leopotam.EcsLite;

namespace Terra.Studio
{
    public class InGameTimerSystem : BaseSystem, IEcsRunSystem
    {
        public override void OnConditionalCheck(int entity, object data)
        {
            ref var entityRef = ref EntityAuthorOp.GetComponent<InGameTimerComponent>(entity);
            var compData = RuntimeOp.Resolve<ComponentsData>();
            compData.ProvideEventContext(false, entityRef.EventContext);
            entityRef.CanExecute = true;
            RuntimeOp.Resolve<InGameTimeHandler>().UpdateTime(entityRef.totalTime);
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
