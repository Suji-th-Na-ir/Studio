using UnityEngine;
using Leopotam.EcsLite;

namespace Terra.Studio
{
    public class InGameTimerSystem : BaseSystem, IEcsRunSystem
    {
        public override void Init<T>(int entity)
        {
            var entityRef = EntityAuthorOp.GetComponent<InGameTimerComponent>(entity);
            if (entityRef.totalTime == 0)
            {
                RuntimeOp.Resolve<RuntimeSystem>().RemoveRunningInstance(this);
                return;
            }
            base.Init<T>(entity);
        }

        public override void OnConditionalCheck(int entity, object data)
        {
            ref var entityRef = ref EntityAuthorOp.GetComponent<InGameTimerComponent>(entity);
            var compData = RuntimeOp.Resolve<ComponentsData>();
            compData.ProvideEventContext(false, entityRef.EventContext);
            entityRef.CanExecute = true;
            RuntimeOp.Resolve<CoreGameManager>().EnableModule<InGameTimeHandler>();
            var startTime = entityRef.timerType == TimerType.CountUp ? 0f : entityRef.totalTime;
            RuntimeOp.Resolve<InGameTimeHandler>().SetTime(startTime);
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
                var factor = timer.timerType == TimerType.CountUp ? 1f : -1f;
                time += Time.deltaTime * factor;
                if (timer.timerType == TimerType.CountUp && time >= timer.totalTime)
                {
                    BroadcastOnTimerEnd(ref timer);
                }
                else if (timer.timerType == TimerType.CountDown && time <= 0f)
                {
                    BroadcastOnTimerEnd(ref timer);
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

        private void BroadcastOnTimerEnd(ref InGameTimerComponent timer)
        {
            timer.IsExecuted = true;
            if (timer.IsBroadcastable)
            {
                RuntimeOp.Resolve<Broadcaster>().Broadcast(timer.Broadcast, true);
            }
        }
    }
}
