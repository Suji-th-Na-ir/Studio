using System;
using UnityEngine;
using Leopotam.EcsLite;
using System.Collections.Generic;

namespace Terra.Studio
{
    public class RotateSystem : BaseSystem
    {
        public override Dictionary<int, Action<object>> IdToConditionalCallback { get; set; }

        public override void Init(EcsWorld currentWorld, int entity)
        {
            var pool = currentWorld.GetPool<RotateComponent>();
            ref var entityRef = ref pool.Get(entity);
            entityRef.CanExecute = false;
            if (entityRef.IsBroadcastable)
            {
                RuntimeOp.Resolve<Broadcaster>().SetBroadcastable(entityRef.Broadcast);
            }
            InjectCondition(true, entity, entityRef);
        }

        public override void OnConditionalCheck(object data)
        {
            var (entity, conditionType, go, _, selection) = ((int, string, GameObject, string, object))data;
            if (conditionType.Equals("Terra.Studio.MouseAction"))
            {
                if (selection == null || selection as GameObject != go)
                {
                    return;
                }
            }
            var world = RuntimeOp.Resolve<RuntimeSystem>().World;
            var pool = world.GetPool<RotateComponent>();
            ref var entityRef = ref pool.Get(entity);
            entityRef.CanExecute = true;
            InjectCondition(false, entity, entityRef);
            OnDemandRun(in entityRef, entity);
        }

        public void OnDemandRun(in RotateComponent rotatable, int entity)
        {
            if (rotatable.canPlaySFX)
            {
                RuntimeWrappers.PlaySFX(rotatable.sfxName);
            }
            if (rotatable.canPlayVFX)
            {
                RuntimeWrappers.PlayVFX(rotatable.vfxName, rotatable.refObj.transform.position);
            }
            var rotateParams = GetParams(rotatable, entity);
            RuntimeWrappers.RotateObject(rotateParams);
        }

        private void InjectCondition(bool inject, int entity, RotateComponent entityRef)
        {
            var conditionType = entityRef.ConditionType;
            var conditionData = entityRef.ConditionData;
            var goRef = entityRef.refObj;
            if (inject)
            {
                IdToConditionalCallback ??= new();
                IdToConditionalCallback.Add(entity, (obj) =>
                {
                    OnConditionalCheck((entity, conditionType, goRef, conditionData, obj));
                });
                RuntimeOp.Resolve<ComponentsData>().ProvideEventContext(conditionType, IdToConditionalCallback[entity], true, (goRef, conditionData));
            }
            else if (IdToConditionalCallback.ContainsKey(entity))
            {
                RuntimeOp.Resolve<ComponentsData>().ProvideEventContext(conditionType, IdToConditionalCallback[entity], false, (goRef, conditionData));
                IdToConditionalCallback.Remove(entity);
            }
        }

        private RotateByParams GetParams(RotateComponent rotatable, int entity)
        {
            var rotateParams = new RotateByParams()
            {
                axis = rotatable.axis,
                direction = rotatable.direction,
                rotationTimes = rotatable.repeatFor,
                rotationSpeed = rotatable.speed,
                rotateBy = rotatable.rotateBy,
                shouldPause = rotatable.pauseFor > 0f,
                pauseForTime = rotatable.pauseFor,
                targetObj = rotatable.refObj,
                shouldPingPong = rotatable.rotationType is RotationType.Oscillate or RotationType.OscillateForever,
                onRotated = (isDone) =>
                {
                    OnRotationDone(rotatable, isDone, entity);
                }
            };
            return rotateParams;
        }

        private void OnRotationDone(RotateComponent rotatable, bool isDone, int entity)
        {
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
                InjectCondition(true, entity, rotatable);
            }
        }

        public override void OnHaltRequested(EcsWorld currentWorld)
        {
            var filter = currentWorld.Filter<RotateComponent>().End();
            var rotatePool = currentWorld.GetPool<RotateComponent>();
            foreach (var entity in filter)
            {
                var rotatable = rotatePool.Get(entity);
                InjectCondition(false, entity, rotatable);
            }
        }
    }
}
