using UnityEngine;
using Leopotam.EcsLite;

namespace Terra.Studio
{
    public class RotateSystem : BaseSystem, IEcsRunSystem
    {
        public override void OnConditionalCheck(int entity, object data)
        {
            ref var entityRef = ref EntityAuthorOp.GetComponent<RotateComponent>(entity);
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
            entityRef.targetRotation = entityRef.currentRotation + (entityRef.rotateBy * entityRef.directionFactor);
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
                if (component.currentRotation * component.directionFactor <
                    component.targetRotation * component.directionFactor)
                {
                    var rotationThisFrame = component.speed * Time.deltaTime;
                    component.currentRotation += rotationThisFrame * component.directionFactor;
                    var targetRotation = GetVector3(rotationThisFrame * component.directionFactor, component.axis);
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
                component.currentRotation = 0f;
                component.targetRotation = component.rotateBy * component.directionFactor;
                OnRotationDone(false, entity);
            }
            if (totalEntitiesFinishedJob == filter.GetEntitiesCount())
            {
                RuntimeOp.Resolve<RuntimeSystem>().RemoveRunningInstance(this);
            }
        }

        private Vector3 GetVector3(float newRotation, Axis[] axes)
        {
            var vector = Vector3.zero;
            for (int i = 0; i < axes.Length; i++)
            {
                switch (axes[i])
                {
                    case Axis.X:
                        vector.x = newRotation;
                        break;
                    case Axis.Y:
                        vector.y = newRotation;
                        break;
                    case Axis.Z:
                        vector.z = newRotation;
                        break;
                }
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
