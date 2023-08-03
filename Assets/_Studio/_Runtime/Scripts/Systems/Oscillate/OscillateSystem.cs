using System;
using UnityEngine;
using Leopotam.EcsLite;
using System.Collections.Generic;

namespace Terra.Studio
{
    public class OscillateSystem : BaseSystem, IAbsRunsystem, IEcsRunSystem, IConditionalOp
    {
        public override Dictionary<int, Action<object>> IdToConditionalCallback { get; set; }

        public void Init(EcsWorld currentWorld, int entity)
        {
            var filter = currentWorld.Filter<OscillateComponent>().End();
            var oscillatorPool = currentWorld.GetPool<OscillateComponent>();
            ref var oscillatable = ref oscillatorPool.Get(entity);
            if (oscillatable.IsExecuted || !oscillatable.IsConditionAvailable)
            {
                oscillatable.CanExecute = true;
                return;
            }
            if (oscillatable.isRegistered)
            {
                return;
            }
            var conditionType = oscillatable.ConditionType;
            var conditionData = oscillatable.ConditionData;
            oscillatable.isRegistered = true;
            var compsData = RuntimeOp.Resolve<ComponentsData>();
            IdToConditionalCallback ??= new();
            IdToConditionalCallback.Add(entity, (obj) =>
            {
                var go = obj != null ? obj as GameObject : null;
                OnConditionalCheck((entity, go, conditionType, conditionData));
            });
            compsData.ProvideEventContext(conditionType, IdToConditionalCallback[entity], true, (oscillatable.oscillatableTr.gameObject, conditionData));
        }

        public void OnConditionalCheck(object data)
        {
            var (id, reference, conditionType, conditionData) = ((int, GameObject, string, string))data;
            var world = RuntimeOp.Resolve<RuntimeSystem>().World;
            var filter = world.Filter<OscillateComponent>().End();
            var oscillatorPool = world.GetPool<OscillateComponent>();
            ref var oscillatable = ref oscillatorPool.Get(id);
            if (conditionType.Equals("Terra.Studio.MouseAction"))
            {
                if (!reference)
                {
                    return;
                }
                else if (reference != oscillatable.oscillatableTr.gameObject)
                {
                    return;
                }
            }
            var compsData = RuntimeOp.Resolve<ComponentsData>();
            compsData.ProvideEventContext(conditionType, IdToConditionalCallback[id], false, (reference, conditionData));
            IdToConditionalCallback.Remove(id);
            oscillatable.CanExecute = true;
            oscillatable.IsExecuted = false;
            oscillatable.oscillatableTr.position = oscillatable.fromPoint;
        }

        public virtual void Run(IEcsSystems systems)
        {
            var filter = systems.GetWorld().Filter<OscillateComponent>().End();
            var oscillatorPool = systems.GetWorld().GetPool<OscillateComponent>();
            var totalEntitiesFinishedJob = 0;
            foreach (var entity in filter)
            {
                ref var oscillatable = ref oscillatorPool.Get(entity);
                if (oscillatable.IsConditionAvailable && !oscillatable.CanExecute)
                {
                    return;
                }
                if (oscillatable.IsExecuted)
                {
                    totalEntitiesFinishedJob++;
                    continue;
                }
                var targetTr = oscillatable.oscillatableTr;
                var delta = targetTr.position - oscillatable.toPoint;
                if (Mathf.Approximately(delta.sqrMagnitude, float.Epsilon))
                {
                    if (!oscillatable.loop)
                    {
                        oscillatable.IsExecuted = true;
                        if (oscillatable.IsBroadcastable)
                        {
                            RuntimeOp.Resolve<Broadcaster>().Broadcast(oscillatable.Broadcast);
                        }
                        continue;
                    }
                    else
                    {
                        (oscillatable.fromPoint, oscillatable.toPoint) = (oscillatable.toPoint, oscillatable.fromPoint);
                    }
                }
                targetTr.position = Vector3.MoveTowards(targetTr.position, oscillatable.toPoint, oscillatable.speed * Time.deltaTime);
            }
            if (totalEntitiesFinishedJob == filter.GetEntitiesCount())
            {
                RuntimeOp.Resolve<RuntimeSystem>().RemoveRunningInstance(this);
            }
        }
    }
}
