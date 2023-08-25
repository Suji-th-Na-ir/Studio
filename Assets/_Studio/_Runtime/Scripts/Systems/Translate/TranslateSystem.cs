using System;
using UnityEngine;
using Leopotam.EcsLite;
using System.Collections.Generic;

namespace Terra.Studio
{
    public class TranslateSystem : BaseSystem
    {
        public override Dictionary<int, Action<object>> IdToConditionalCallback { get; set; }

        public override void Init(EcsWorld currentWorld, int entity)
        {
            var pool = currentWorld.GetPool<TranslateComponent>();
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
            var pool = world.GetPool<TranslateComponent>();
            ref var entityRef = ref pool.Get(entity);
            entityRef.CanExecute = true;
            entityRef.IsExecuted = true;
            InjectCondition(false, entity, entityRef);
            OnDemandRun(ref entityRef, entity);
        }

        public void OnDemandRun(ref TranslateComponent translatable, int entity)
        {
            if (translatable.canPlaySFX)
            {
                RuntimeWrappers.PlaySFX(translatable.sfxName);
            }
            if (translatable.canPlayVFX)
            {
                RuntimeWrappers.PlayVFX(translatable.vfxName, translatable.refObj.transform.position);
            }
            var translateParams = GetParams(ref translatable, entity);
            RuntimeWrappers.TranslateObject(translateParams);
        }

        private void InjectCondition(bool inject, int entity, TranslateComponent entityRef)
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

        private TranslateParams GetParams(ref TranslateComponent translatable, int entity)
        {
            var copy = translatable;
            var tr = translatable.refObj.transform;
            var targetPos = tr.parent == null ? translatable.targetPosition : tr.TransformPoint(translatable.targetPosition);
            var pauseDistance = Vector3.Distance(tr.position, targetPos);
            if (translatable.pauseFor != 0f && translatable.pauseDistance == 0)
            {
                var direction = targetPos - translatable.startPosition;
                translatable.direction = direction;
                var distance = Vector3.Distance(translatable.startPosition, targetPos);
                translatable.pauseDistance = pauseDistance = distance;
                var newPos = tr.position;
                if (translatable.startPosition != newPos)
                {
                    targetPos = newPos + direction * distance;
                }
            }
            var translateParams = new TranslateParams()
            {
                translateFrom = translatable.refObj.transform.position,
                translateTo = targetPos,
                speed = translatable.speed,
                translateTimes = translatable.repeatFor,
                shouldPingPong = translatable.translateType is TranslateType.PingPong or TranslateType.PingPongForever,
                shouldPause = translatable.pauseFor > 0f,
                pauseDistance = pauseDistance,
                pauseForTime = translatable.pauseFor,
                targetObj = translatable.refObj,
                onTranslated = (isDone) =>
                {
                    OnTranslateDone(copy, isDone, entity);
                }
            };
            return translateParams;
        }

        private void OnTranslateDone(TranslateComponent translatable, bool isDone, int entity)
        {
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
                InjectCondition(true, entity, translatable);
            }
        }

        public override void OnHaltRequested(EcsWorld currentWorld)
        {
            var filter = currentWorld.Filter<TranslateComponent>().End();
            var translatePool = currentWorld.GetPool<TranslateComponent>();
            foreach (var entity in filter)
            {
                var translatable = translatePool.Get(entity);
                InjectCondition(false, entity, translatable);
            }
        }
    }
}
