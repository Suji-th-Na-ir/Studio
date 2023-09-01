using System;
using UnityEngine;
using Leopotam.EcsLite;
using System.Collections.Generic;

namespace Terra.Studio
{
    public class OscillateSystem : BaseSystem, IEcsRunSystem
    {
        public override Dictionary<int, Action<object>> IdToConditionalCallback { get; set; }
        private EventExecutorData eventExecutorData;

        public override void Init(EcsWorld currentWorld, int entity)
        {
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
            oscillatable.fromPoint = oscillatable.oscillatableTr.TransformPoint(oscillatable.fromPoint);
            oscillatable.toPoint = oscillatable.oscillatableTr.TransformPoint(oscillatable.toPoint);
            oscillatable.isRegistered = true;
            var compsData = RuntimeOp.Resolve<ComponentsData>();
            eventExecutorData = new()
            {
                goRef = oscillatable.oscillatableTr.gameObject,
                data = conditionData
            };
            IdToConditionalCallback ??= new();
            IdToConditionalCallback.Add(entity, (obj) =>
            {
                var go = obj != null ? obj as GameObject : null;
                OnConditionalCheck((entity, go, conditionType, conditionData));
            });
            compsData.ProvideEventContext(conditionType, IdToConditionalCallback[entity], true, eventExecutorData);
        }

        public override void OnConditionalCheck(object data)
        {
            var (id, reference, conditionType, conditionData) = ((int, GameObject, string, string))data;
            var world = RuntimeOp.Resolve<RuntimeSystem>().World;
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
            compsData.ProvideEventContext(conditionType, IdToConditionalCallback[id], false, eventExecutorData);
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

        public override void OnHaltRequested(EcsWorld currentWorld)
        {
            var filter = currentWorld.Filter<OscillateComponent>().End();
            var oscillatorPool = currentWorld.GetPool<OscillateComponent>();
            var compsData = RuntimeOp.Resolve<ComponentsData>();
            foreach (var entity in filter)
            {
                if (!IdToConditionalCallback.ContainsKey(entity)) continue;
                var oscillatable = oscillatorPool.Get(entity);
                compsData.ProvideEventContext(oscillatable.ConditionType, IdToConditionalCallback[entity], false, eventExecutorData);
                IdToConditionalCallback.Remove(entity);
            }
        }
    }
}
