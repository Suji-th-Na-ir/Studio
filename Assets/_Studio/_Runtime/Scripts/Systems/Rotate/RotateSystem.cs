using UnityEngine;
using PlayShifu.Terra;
using Leopotam.EcsLite;

namespace Terra.Studio
{
    public class RotateSystem : BaseSystem, IEcsRunSystem
    {
        private const float DEFAULT_AXIS_CONST = -1000f;
        private readonly Vector3 DEFAULT_TARGET_ROTATION = new(DEFAULT_AXIS_CONST, DEFAULT_AXIS_CONST, DEFAULT_AXIS_CONST);

        public override void Init<T>(int entity)
        {
            base.Init<T>(entity);
            ref var entityRef = ref entity.GetComponent<RotateComponent>();
            var rb = entityRef.RefObj.AddRigidbody();
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        public override void OnConditionalCheck(int entity, object data)
        {
            ref var entityRef = ref entity.GetComponent<RotateComponent>();
            Init(ref entityRef);
            var compsData = RuntimeOp.Resolve<ComponentsData>();
            compsData.ProvideEventContext(false, entityRef.EventContext);
            OnDemandRun(in entityRef, entity);
        }

        private void Init(ref RotateComponent entityRef)
        {
            entityRef.CanExecute = true;
            if (entityRef.isHaltedByEvent)
            {
                entityRef.isHaltedByEvent = false;
                return;
            }
            entityRef.currentRotateCount = 0;
            entityRef.directionFactor = (entityRef.direction == Direction.Clockwise) ? 1 : -1;
            SetTargetRotation(ref entityRef);
            entityRef.canPause = entityRef.pauseFor > 0f;
            entityRef.shouldPingPong = entityRef.rotationType is RotationType.Oscillate or RotationType.OscillateForever;
            entityRef.rotateForever = entityRef.repeatFor == int.MaxValue;
        }

        public void OnDemandRun(in RotateComponent rotatable, int _)
        {
            if (rotatable.canPlaySFX)
            {
                RuntimeWrappers.PlaySFX(rotatable.sfxName);
            }
            if (rotatable.canPlayVFX)
            {
                RuntimeWrappers.PlayVFX(rotatable.vfxName, rotatable.RefObj.transform.position);
            }
        }

        public void Run(IEcsSystems systems)
        {
            var filter = systems.GetWorld().Filter<RotateComponent>().End();
            var pool = systems.GetWorld().GetPool<RotateComponent>();
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
                if (!component.rotateForever &&
                    component.currentRotateCount >= component.repeatFor)
                {
                    component.IsExecuted = true;
                    OnRotationDone(true, entity);
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
                var targetRotation = CalculateAngleChangePerAxis(ref component);
                if (targetRotation != DEFAULT_TARGET_ROTATION)
                {
                    targetRotation = GetCleanVector(targetRotation);
                    component.RefObj.transform.Rotate(targetRotation);
                    continue;
                }
                if (component.canPause)
                {
                    component.pauseStartTime = Time.time;
                    component.isPaused = true;
                }
                if (component.shouldPingPong)
                {
                    component.directionFactor *= -1;
                }
                component.currentRotateCount++;
                ResetDirtyValues(ref component);
                OnRotationDone(false, entity);
            }
            if (totalEntitiesFinishedJob == filter.GetEntitiesCount())
            {
                RuntimeOp.Resolve<RuntimeSystem>().RemoveRunningInstance(this);
            }
        }

        private Vector3 CalculateAngleChangePerAxis(ref RotateComponent component)
        {
            var targetRotation = DEFAULT_TARGET_ROTATION;
            var rotationThisFrame = component.speed * Time.deltaTime;
            var step = rotationThisFrame * component.directionFactor;
            if (component.xCurrentRotation * component.directionFactor < component.xTargetRotation * component.directionFactor)
            {
                component.xCurrentRotation += step;
                targetRotation.x = step;
            }
            if (component.yCurrentRotation * component.directionFactor < component.yTargetRotation * component.directionFactor)
            {
                component.yCurrentRotation += step;
                targetRotation.y = step;
            }
            if (component.zCurrentRotation * component.directionFactor < component.zTargetRotation * component.directionFactor)
            {
                component.zCurrentRotation += step;
                targetRotation.z = step;
            }
            return targetRotation;
        }

        private void ResetDirtyValues(ref RotateComponent component)
        {
            component.xCurrentRotation = 0f;
            component.yCurrentRotation = 0f;
            component.zCurrentRotation = 0f;
            SetTargetRotation(ref component);
        }

        private void SetTargetRotation(ref RotateComponent component)
        {
            component.xTargetRotation = component.xCurrentRotation + (component.expectedRotateBy.x * component.directionFactor);
            component.yTargetRotation = component.yCurrentRotation + (component.expectedRotateBy.y * component.directionFactor);
            component.zTargetRotation = component.zCurrentRotation + (component.expectedRotateBy.z * component.directionFactor);
        }

        private Vector3 GetCleanVector(Vector3 vector)
        {
            if (vector.x == DEFAULT_AXIS_CONST)
            {
                vector.x = 0f;
            }
            if (vector.y == DEFAULT_AXIS_CONST)
            {
                vector.y = 0f;
            }
            if (vector.z == DEFAULT_AXIS_CONST)
            {
                vector.z = 0f;
            }
            return vector;
        }

        private void OnRotationDone(bool isDone, int entity)
        {
            ref var rotatable = ref entity.GetComponent<RotateComponent>();
            if (rotatable.IsBroadcastable)
            {
                if (rotatable.broadcastAt == BroadcastAt.AtEveryInterval && !isDone)
                {
                    RuntimeOp.Resolve<Broadcaster>().Broadcast(rotatable.Broadcast, false);
                }
                if (rotatable.broadcastAt == BroadcastAt.End && isDone)
                {
                    RuntimeOp.Resolve<Broadcaster>().Broadcast(rotatable.Broadcast, true);
                }
            }
            if (rotatable.listen == Listen.Always && !rotatable.ConditionType.Equals("Terra.Studio.GameStart") && isDone)
            {
                rotatable.IsExecuted = false;
                rotatable.CanExecute = false;
                var compsData = RuntimeOp.Resolve<ComponentsData>();
                compsData.ProvideEventContext(true, rotatable.EventContext);
            }
        }

        public override void OnHaltRequested(EcsWorld currentWorld)
        {
            var filter = currentWorld.Filter<RotateComponent>().End();
            var rotatePool = currentWorld.GetPool<RotateComponent>();
            foreach (var entity in filter)
            {
                var rotatable = rotatePool.Get(entity);
                if (rotatable.CanExecute)
                {
                    continue;
                }
                var compsData = RuntimeOp.Resolve<ComponentsData>();
                compsData.ProvideEventContext(false, rotatable.EventContext);
            }
        }
    }
}
