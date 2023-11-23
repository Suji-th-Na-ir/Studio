using UnityEngine;
using PlayShifu.Terra;
using Leopotam.EcsLite;

namespace Terra.Studio
{
    public class RotateSystem : BaseSystem, IEcsRunSystem
    {
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
            OnDemandRun(in entityRef);
        }

        private void Init(ref RotateComponent entityRef)
        {
            entityRef.CanExecute = true;
            if (entityRef.isHaltedByEvent)
            {
                entityRef.isHaltedByEvent = false;
                return;
            }
            CalculateStartAndTargetRotation(ref entityRef);
            entityRef.currentRotateCount = 0;
            entityRef.canPause = entityRef.pauseFor > 0f;
            entityRef.shouldPingPong = entityRef.repeatType == RepeatDirectionType.PingPong;
            entityRef.rotateForever = entityRef.repeatForever;
            entityRef.directionFactor = (entityRef.direction == Direction.Clockwise) ? -1 : 1;
            RefreshRotation(ref entityRef);
        }

        private void RefreshRotation(ref RotateComponent entityRef)
        {
            if (!entityRef.shouldPingPong && (entityRef.repeatFor > 1 || entityRef.repeatForever))
            {
                CalculateStartAndTargetRotation(ref entityRef);
            }
            entityRef.currentStartRotation = Quaternion.Euler(entityRef.startRotation);
            entityRef.currentTargetRotation = Quaternion.Euler(entityRef.trueRotateTarget);

            if (entityRef.shouldPingPong)
            {
                if (entityRef.currentRotateCount % 2 == 1)
                {
                    entityRef.currentStartRotation = Quaternion.Euler(entityRef.trueRotateTarget);
                    entityRef.currentTargetRotation = Quaternion.Euler(entityRef.startRotation);
                }
            }
            if (entityRef.direction == Direction.AntiClockwise)
            {
                var inverseX = entityRef.rotateTo.x - 360f;
                var inverseY = entityRef.rotateTo.y - 360f;
                var inverseZ = entityRef.rotateTo.z - 360f;
                var newRotateTarget = entityRef.RefObj.transform.eulerAngles + new Vector3(inverseX, inverseY, inverseZ);
                if (entityRef.shouldPingPong)
                {
                    if (entityRef.currentRotateCount % 2 == 1)
                    {
                        entityRef.currentStartRotation = Quaternion.Euler(newRotateTarget);
                    }
                    else
                    {
                        entityRef.currentTargetRotation = Quaternion.Euler(newRotateTarget);
                    }
                }
                else
                    entityRef.currentTargetRotation = Quaternion.Euler(newRotateTarget);
            }
            var angle = Quaternion.Angle(entityRef.currentStartRotation, entityRef.currentTargetRotation);
            entityRef.rotationTime = angle / entityRef.speed;
            entityRef.currentRatio = 0f;
            entityRef.elapsedTime = 0f;
        }

        private void CalculateStartAndTargetRotation(ref RotateComponent entityRef)
        {
            entityRef.startRotation = entityRef.RefObj.transform.eulerAngles;
            entityRef.trueRotateTarget = entityRef.RefObj.transform.eulerAngles + entityRef.rotateTo;
        }

        public void OnDemandRun(in RotateComponent rotatable)
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

                component.currentRatio = component.elapsedTime / component.rotationTime;
                component.elapsedTime += Time.deltaTime;
                var isShortWay = component.direction == Direction.Clockwise;
                var newRotation = Helper.Slerp(component.currentStartRotation, component.currentTargetRotation, component.currentRatio, isShortWay);
                component.RefObj.transform.rotation = newRotation;


                if (Mathf.Abs(component.currentRatio) < 1)
                {
                    continue;
                }
                else
                {
                    component.RefObj.transform.rotation = component.currentTargetRotation;
                }

                if (component.canPause)
                {
                    component.pauseStartTime = Time.time;
                    component.isPaused = true;
                }

                component.currentRotateCount++;
                if (component.rotateForever && component.currentRotateCount == 100)
                {
                    component.currentRotateCount = 0;
                }

                RefreshRotation(ref component);
                OnRotationDone(false, entity);
            }
            if (totalEntitiesFinishedJob == filter.GetEntitiesCount())
            {
                RuntimeOp.Resolve<RuntimeSystem>().RemoveRunningInstance(this);
            }
        }

        private void OnRotationDone(bool isDone, int entity)
        {
            ref var rotatable = ref entity.GetComponent<RotateComponent>();
            if (rotatable.IsBroadcastable)
            {
                if (rotatable.broadcastAt == BroadcastAt.AtEveryPause && !isDone)
                {
                    RuntimeOp.Resolve<Broadcaster>().Broadcast(rotatable.Broadcast);
                }
                if (rotatable.broadcastAt == BroadcastAt.End && isDone)
                {
                    RuntimeOp.Resolve<Broadcaster>().Broadcast(rotatable.Broadcast);
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
