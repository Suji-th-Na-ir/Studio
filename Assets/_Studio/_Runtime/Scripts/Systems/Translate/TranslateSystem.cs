using UnityEngine;
using PlayShifu.Terra;
using Leopotam.EcsLite;

namespace Terra.Studio
{
    public class TranslateSystem : BaseSystem, IEcsRunSystem
    {
        public override void Init<T>(int entity)
        {
            base.Init<T>(entity);
            ref var entityRef = ref entity.GetComponent<TranslateComponent>();
            var rb = entityRef.RefObj.AddRigidbody();
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        public override void OnConditionalCheck(int entity, object data)
        {
            ref var entityRef = ref entity.GetComponent<TranslateComponent>();
            Init(ref entityRef);
            var compsData = RuntimeOp.Resolve<ComponentsData>();
            compsData.ProvideEventContext(false, entityRef.EventContext);
            OnDemandRun(ref entityRef);
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
            if (!entityRef.isInitialProcessDone)
            {
                if (tr.parent != null)
                {
                    entityRef.targetPosition = tr.TransformVector(entityRef.targetPosition);
                }
                entityRef.isInitialProcessDone = true;
            }
            var targetPos = tr.parent == null ? entityRef.targetPosition + entityRef.startPosition : entityRef.startPosition + tr.TransformDirection(entityRef.targetPosition);
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
            entityRef.shouldPingPong = entityRef.translateType is RepeatDirectionType.PingPong;
            entityRef.loopsFinished = 0;
            entityRef.coveredDistance = 0f;
            entityRef.remainingDistance = pauseDistance;
        }

        public void OnDemandRun(ref TranslateComponent translatable)
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
                if (translatable.broadcastAt == BroadcastAt.AtEveryPause && !isDone)
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
            if (component.remainingDistance > 0)
            {
                step = Mathf.Clamp(step, 0.0f, component.remainingDistance);
            }
            var movement = component.direction.normalized * step;
            component.RefObj.transform.position += movement;
            component.remainingDistance -= step;
            component.coveredDistance += step;
            if (component.remainingDistance <= 0f)
            {
                component.loopsFinished++;
                component.remainingDistance = component.pauseDistance;
            }
            if (component.shouldPause && component.coveredDistance >= component.pauseDistance)
            {
                component.isPaused = true;
                component.pauseStartTime = Time.time;
                component.coveredDistance = 0f;
                OnTranslateDone(false, entity);
            }
        }

        private void PerformOscillation(ref TranslateComponent component, int entity)
        {
            var step = component.speed * Time.deltaTime;
            if (component.remainingDistance > 0)
            {
                step = Mathf.Clamp(step, 0.0f, component.remainingDistance);
            }
            var movement = component.direction.normalized * step;
            component.RefObj.transform.position += movement;
            component.remainingDistance -= step;
            var targetPosition = component.RefObj.transform.parent == null ? component.targetPosition + component.startPosition :
                component.startPosition + component.RefObj.transform.TransformDirection(component.targetPosition);
            if (component.remainingDistance <= 0.01f)
            {
                component.loopsFinished++;
                component.direction = component.loopsFinished % 2 == 0 ?
                    (targetPosition - component.startPosition).normalized :
                    (component.startPosition - targetPosition).normalized;
                component.remainingDistance = component.pauseDistance;
                if (component.shouldPause)
                {
                    component.isPaused = true;
                    component.pauseStartTime = Time.time;
                }
                OnTranslateDone(false, entity);
            }
        }
    }
}
