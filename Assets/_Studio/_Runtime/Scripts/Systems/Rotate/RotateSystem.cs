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
            var conditionType = entityRef.ConditionType;
            var conditionData = entityRef.ConditionData;
            var goRef = entityRef.refObj;
            if (entityRef.IsBroadcastable)
            {
                RuntimeOp.Resolve<Broadcaster>().SetBroadcastable(entityRef.Broadcast);
            }
            IdToConditionalCallback ??= new();
            IdToConditionalCallback.Add(entity, (obj) =>
            {
                OnConditionalCheck((entity, conditionType, goRef, conditionData, obj));
            });
            RuntimeOp.Resolve<ComponentsData>().ProvideEventContext(conditionType, IdToConditionalCallback[entity], true, (goRef, conditionData));
        }

        public override void OnConditionalCheck(object data)
        {
            var (entity, conditionType, go, conditionData, selection) = ((int, string, GameObject, string, object))data;
            if (conditionType.Equals("Terra.Studio.MouseAction"))
            {
                if (selection == null || selection as GameObject != go)
                {
                    return;
                }
            }
            var compsData = RuntimeOp.Resolve<ComponentsData>();
            compsData.ProvideEventContext(conditionType, IdToConditionalCallback[entity], false, (go, conditionData));
            IdToConditionalCallback.Remove(entity);
            var world = RuntimeOp.Resolve<RuntimeSystem>().World;
            var pool = world.GetPool<RotateComponent>();
            ref var entityRef = ref pool.Get(entity);
            entityRef.CanExecute = true;
            OnDemandRun(in entityRef);
        }

        public void OnDemandRun(in RotateComponent rotatable)
        {
            if (rotatable.canPlaySFX)
            {
                RuntimeWrappers.PlaySFX(rotatable.sfxName);
            }
            if (rotatable.canPlayVFX)
            {
                RuntimeWrappers.PlayVFX(rotatable.vfxName, rotatable.refObj.transform.position);
            }
            var rotateParams = GetParams(rotatable);
            RuntimeWrappers.RotateObject(rotateParams);
        }

        private RotateByParams GetParams(RotateComponent rotatable)
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
                broadcastAt = rotatable.broadcastAt,
                shouldPingPong = rotatable.rotationType is RotationType.Oscillate or RotationType.OscillateForever,
                onRotated = (isDone) =>
                {
                    if (rotatable.IsBroadcastable)
                    {
                        OnRotationDone(rotatable.Broadcast, rotatable.broadcastAt == BroadcastAt.End || isDone);
                    }
                }
            };
            return rotateParams;
        }

        private void OnRotationDone(string broadcast, bool removeOnceBroadcasted)
        {
            RuntimeOp.Resolve<Broadcaster>().Broadcast(broadcast, removeOnceBroadcasted);
        }

        public override void OnHaltRequested(EcsWorld currentWorld)
        {
            var filter = currentWorld.Filter<RotateComponent>().End();
            var rotatePool = currentWorld.GetPool<RotateComponent>();
            var compsData = RuntimeOp.Resolve<ComponentsData>();
            foreach (var entity in filter)
            {
                if (!IdToConditionalCallback.ContainsKey(entity)) continue;
                var rotatable = rotatePool.Get(entity);
                compsData.ProvideEventContext(rotatable.ConditionType, IdToConditionalCallback[entity], false, (rotatable.refObj, rotatable.ConditionData));
                IdToConditionalCallback.Remove(entity);
            }
        }
    }
}
