using UnityEngine;
using Leopotam.EcsLite;

namespace Terra.Studio
{
    public class OscillateSystem : IEcsInitSystem, IEcsRunSystem, IConditionalOp
    {
        public virtual void Init(IEcsSystems systems)
        {
            var filter = systems.GetWorld().Filter<OscillateComponent>().End();
            var oscillatorPool = systems.GetWorld().GetPool<OscillateComponent>();
            foreach (var entity in filter)
            {
                ref var oscillatable = ref oscillatorPool.Get(entity);
                if (oscillatable.IsExecuted || !oscillatable.IsConditionAvailable)
                {
                    oscillatable.CanExecute = true;
                    return;
                }
                oscillatable.oscillatableTr.position = oscillatable.fromPoint;
                var conditionalData = oscillatable.ConditionData;
                ComponentsData.GetSystemForCondition(conditionalData, (obj) =>
                {
                    var go = obj != null ? obj as GameObject : null;
                    OnConditionalCheck((entity, go, conditionalData));
                }, true);
            }
        }

        public virtual void Run(IEcsSystems systems)
        {
            var filter = systems.GetWorld().Filter<OscillateComponent>().End();
            var totalEntitiesCount = filter.GetEntitiesCount();
            var currentExecutedCount = 0;
            var oscillatorPool = systems.GetWorld().GetPool<OscillateComponent>();
            foreach (var entity in filter)
            {
                ref var oscillatable = ref oscillatorPool.Get(entity);
                if (!oscillatable.CanExecute)
                {
                    return;
                }
                if (oscillatable.IsExecuted)
                {
                    currentExecutedCount++;
                    continue;
                }
                var targetTr = oscillatable.oscillatableTr;
                var delta = targetTr.position - oscillatable.toPoint;
                if (Mathf.Approximately(delta.sqrMagnitude, float.Epsilon))
                {
                    if (!oscillatable.loop)
                    {
                        oscillatable.IsExecuted = true;
                        continue;
                    }
                    else
                    {
                        // var tempRef = oscillatable.fromPoint;
                        // oscillatable.fromPoint = oscillatable.toPoint;
                        // oscillatable.toPoint = tempRef;
                        (oscillatable.fromPoint, oscillatable.toPoint) = (oscillatable.toPoint, oscillatable.fromPoint);
                    }
                }
                Debug.Log("oscillating move towards");
                targetTr.position = Vector3.MoveTowards(targetTr.position, oscillatable.toPoint, oscillatable.speed * Time.deltaTime);
            }
            if (currentExecutedCount == totalEntitiesCount)
            {
                Interop<RuntimeInterop>.Current.Resolve<RuntimeSystem>().RemoveUpdateSystem(this);
            }
        }

        public void OnConditionalCheck(object data)
        {
            Debug.Log("on condition check");
            var tuple = ((int id, UnityEngine.GameObject reference, string conditionalData))data;
            if (!tuple.reference)
            {
                return;
            }
            var world = Interop<RuntimeInterop>.Current.Resolve<RuntimeSystem>().World;
            var filter = world.Filter<OscillateComponent>().End();
            var oscillatorPool = world.GetPool<OscillateComponent>();
            ref var oscillatable = ref oscillatorPool.Get(tuple.id);
            oscillatable.CanExecute = true;
            ComponentsData.GetSystemForCondition(tuple.conditionalData, OnConditionalCheck, false);
        }
    }
}
