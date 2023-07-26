using UnityEngine;
using Leopotam.EcsLite;

namespace Terra.Studio
{
    public class OscillateSystem : IAbsRunsystem, IEcsRunSystem, IConditionalOp
    {
        public void Init(EcsWorld currentWorld)
        {
            var filter = currentWorld.Filter<OscillateComponent>().End();
            var oscillatorPool = currentWorld.GetPool<OscillateComponent>();
            foreach (var entity in filter)
            {
                ref var oscillatable = ref oscillatorPool.Get(entity);
                if (oscillatable.IsExecuted || !oscillatable.IsConditionAvailable)
                {
                    oscillatable.CanExecute = true;
                    return;
                }
                if (oscillatable.isRegistered)
                {
                    continue;
                }
                var conditionType = oscillatable.ConditionType;
                var conditionData = oscillatable.ConditionData;
                oscillatable.isRegistered = true;
                ComponentsData.GetSystemForCondition(conditionType, (obj) =>
                {
                    var go = obj != null ? obj as GameObject : null;
                    OnConditionalCheck((entity, go, conditionType));
                }, true, conditionData);
            }
        }

        public virtual void Run(IEcsSystems systems)
        {
            var filter = systems.GetWorld().Filter<OscillateComponent>().End();
            var oscillatorPool = systems.GetWorld().GetPool<OscillateComponent>();
            foreach (var entity in filter)
            {
                ref var oscillatable = ref oscillatorPool.Get(entity);
                if (oscillatable.IsConditionAvailable && !oscillatable.CanExecute)
                {
                    return;
                }
                if (oscillatable.IsExecuted)
                {
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
                        (oscillatable.fromPoint, oscillatable.toPoint) = (oscillatable.toPoint, oscillatable.fromPoint);
                    }
                }
                targetTr.position = Vector3.MoveTowards(targetTr.position, oscillatable.toPoint, oscillatable.speed * Time.deltaTime);
            }
        }

        public void OnConditionalCheck(object data)
        {
            var tuple = ((int id, UnityEngine.GameObject reference, string conditionalData))data;
            if (!tuple.reference)
            {
                ComponentsData.GetSystemForCondition(tuple.conditionalData, OnConditionalCheck, false);
                return;
            }
            var world = Interop<RuntimeInterop>.Current.Resolve<RuntimeSystem>().World;
            var filter = world.Filter<OscillateComponent>().End();
            var oscillatorPool = world.GetPool<OscillateComponent>();
            ref var oscillatable = ref oscillatorPool.Get(tuple.id);
            oscillatable.CanExecute = true;
            oscillatable.IsExecuted = false;
            oscillatable.oscillatableTr.position = oscillatable.fromPoint;
        }
    }
}
