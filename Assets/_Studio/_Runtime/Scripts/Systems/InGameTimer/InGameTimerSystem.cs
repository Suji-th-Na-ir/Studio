using UnityEngine;
using Leopotam.EcsLite;

namespace Terra.Studio
{
    public class InGameTimerSystem : BaseSystem<InGameTimerComponent>, IEcsRunSystem
    {
        public override void Init(int entity)
        {
            var entityRef = EntityAuthorOp.GetComponent<InGameTimerComponent>(entity);
            if (entityRef.totalTime == 0)
            {
                RemoveRunningInstance();
                return;
            }
            base.Init(entity);
        }

        protected override void OnConditionalCheck(int entity, object data)
        {
            base.OnConditionalCheck(entity, data);
            ref var entityRef = ref EntityAuthorOp.GetComponent<InGameTimerComponent>(entity);
            entityRef.IsExecuted = false;
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
                RemoveRunningInstance();
            }
        }

        private void BroadcastOnTimerEnd(ref InGameTimerComponent timer)
        {
            timer.IsExecuted = true;
            Broadcast(timer);
        }
    }
}
