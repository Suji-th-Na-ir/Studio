using UnityEngine;
using Leopotam.EcsLite;

namespace Terra.Studio
{
    public class TranslateSystem : BaseSystem, IEcsRunSystem
    {
        public override void OnConditionalCheck(int entity, object data)
        {
            ref var entityRef = ref EntityAuthorOp.GetComponent<TranslateComponent>(entity);
            if (entityRef.ConditionType.Equals("Terra.Studio.MouseAction"))
            {
                if (data == null)
                {
                    return;
                }
                var selection = (GameObject)data;
                if (selection != entityRef.RefObj)
                {
                    return;
                }
            }
            Init(ref entityRef);
            var compsData = RuntimeOp.Resolve<ComponentsData>();
            compsData.ProvideEventContext(false, entityRef.EventContext);
            OnDemandRun(ref entityRef, entity);
        }

        private void Init(ref TranslateComponent entityRef)
        {
            entityRef.CanExecute = true;
            if (entityRef.isHaltedByEvent)
            {
                entityRef.isHaltedByEvent = false;
                return;
            }
            var tr = entityRef.RefObj.transform;
            var targetPos = tr.parent == null ? entityRef.targetPosition : tr.TransformPoint(entityRef.targetPosition);
            var pauseDistance = Vector3.Distance(entityRef.startPosition, targetPos);
            var direction = targetPos - entityRef.startPosition;
            entityRef.pauseDistance = pauseDistance;
            entityRef.direction = direction;
            if (entityRef.startPosition != tr.position)
            {
                targetPos = tr.position + direction * pauseDistance;
            }
            entityRef.currentTargetPosition = targetPos;
            entityRef.currentStartPosition = tr.position;
            entityRef.shouldPause = entityRef.pauseFor > 0f;
            entityRef.shouldPingPong = entityRef.translateType is TranslateType.PingPong or TranslateType.PingPongForever;
            entityRef.loopsFinished = 0;
            entityRef.coveredDistance = 0f;
            entityRef.remainingDistance = pauseDistance;
            entityRef.repeatForever = entityRef.repeatFor == int.MaxValue;
        }

        public void OnDemandRun(ref TranslateComponent translatable, int _)
        {
            if (translatable.canPlaySFX)
            {
                RuntimeWrappers.PlaySFX(translatable.sfxName);
            }
            if (translatable.canPlayVFX)
            {
                RuntimeWrappers.PlayVFX(translatable.vfxName, translatable.RefObj.transform.position);
            }
        }

        private void OnTranslateDone(bool isDone, int entity)
        {
            ref var translatable = ref entity.GetComponent<TranslateComponent>();
            if (translatable.IsBroadcastable)
            {
                if (translatable.broadcastAt == BroadcastAt.AtEveryInterval && !isDone)
                {
                    RuntimeOp.Resolve<Broadcaster>().Broadcast(translatable.Broadcast, false);
                }
                if (translatable.broadcastAt == BroadcastAt.End && isDone)
                {
                    RuntimeOp.Resolve<Broadcaster>().Broadcast(translatable.Broadcast, true);
                }
            }
            if (translatable.listen == Listen.Always && !translatable.ConditionType.Equals("Terra.Studio.GameStart") && isDone)
            {
                translatable.CanExecute = false;
                translatable.IsExecuted = false;
                var compsData = RuntimeOp.Resolve<ComponentsData>();
                compsData.ProvideEventContext(true, translatable.EventContext);
            }
        }

        public override void OnHaltRequested(EcsWorld currentWorld)
        {
            var filter = currentWorld.Filter<TranslateComponent>().End();
            var translatePool = currentWorld.GetPool<TranslateComponent>();
            foreach (var entity in filter)
            {
                var translatable = translatePool.Get(entity);
                if (translatable.CanExecute)
                {
                    continue;
                }
                var compsData = RuntimeOp.Resolve<ComponentsData>();
                compsData.ProvideEventContext(false, translatable.EventContext);
            }
        }

        public void Run(IEcsSystems systems)
        {
            var filter = systems.GetWorld().Filter<TranslateComponent>().End();
            var pool = systems.GetWorld().GetPool<TranslateComponent>();
            var totalEntitiesFinishedJob = 0;
            foreach (var entity in filter)
            {
                ref var component = ref pool.Get(entity);
                if (!component.CanExecute)
                {
                    continue;
                }
                if (component.IsExecuted)
                {
                    totalEntitiesFinishedJob++;
                    continue;
                }
                if (!component.repeatForever &&
                    component.loopsFinished >= component.repeatFor)
                {
                    component.IsExecuted = true;
                    OnTranslateDone(true, entity);
                    continue;
                }
                if (component.isHaltedByEvent)
                {
                    continue;
                }
                if (component.isPaused)
                {
                    var delta = Time.time - component.pauseStartTime;
                    var isDone = delta >= component.pauseFor;
                    if (isDone)
                    {
                        component.isPaused = false;
                    }
                    continue;
                }
                if (!component.shouldPingPong)
                {
                    PerformTranslation(ref component, entity);
                }
                else
                {
                    PerformOscillation(ref component, entity);
                }
            }
            if (totalEntitiesFinishedJob == filter.GetEntitiesCount())
            {
                RuntimeOp.Resolve<RuntimeSystem>().RemoveRunningInstance(this);
            }
        }

        private void PerformTranslation(ref TranslateComponent component, int entity)
        {
            var step = component.speed * Time.deltaTime;
            var movement = component.direction.normalized * step;
            component.RefObj.transform.position += movement;
            component.remainingDistance -= step;
            component.coveredDistance += step;
            if (component.shouldPause && component.coveredDistance >= component.pauseDistance)
            {
                component.isPaused = true;
                component.pauseStartTime = Time.time;
                component.coveredDistance = 0f;
                OnTranslateDone(false, entity);
                return;
            }
            if (component.remainingDistance <= 0.01f)
            {
                component.loopsFinished++;
                component.remainingDistance = component.pauseDistance;
            }
        }

        private void PerformOscillation(ref TranslateComponent component, int entity)
        {
            var step = component.speed * Time.deltaTime;
            var movement = component.direction.normalized * step;
            component.RefObj.transform.position += movement;
            component.remainingDistance -= step;
            if (component.remainingDistance <= 0.01f)
            {
                component.loopsFinished++;
                component.direction = component.loopsFinished % 2 == 0 ?
                    (component.targetPosition - component.startPosition).normalized :
                    (component.startPosition - component.targetPosition).normalized;
                component.remainingDistance = component.pauseDistance;
                if (component.shouldPause && !component.repeatForever)
                {
                    component.isPaused = true;
                    component.pauseStartTime = Time.time;
                }
                OnTranslateDone(false, entity);
            }
        }
    }
}
