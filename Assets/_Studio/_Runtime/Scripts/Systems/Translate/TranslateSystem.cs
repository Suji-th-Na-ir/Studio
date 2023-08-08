using System;
using UnityEngine;
using Leopotam.EcsLite;
using System.Collections.Generic;

namespace Terra.Studio
{
    public class TranslateSystem : BaseSystem, IAbsRunsystem, IConditionalOp
    {
        public override Dictionary<int, Action<object>> IdToConditionalCallback { get; set; }

        public void Init(EcsWorld currentWorld, int entity)
        {
            var pool = currentWorld.GetPool<TranslateComponent>();
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

        public void OnConditionalCheck(object data)
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
            var pool = world.GetPool<TranslateComponent>();
            ref var entityRef = ref pool.Get(entity);
            entityRef.CanExecute = true;
            entityRef.IsExecuted = true;
            OnDemandRun(in entityRef);
        }

        public void OnDemandRun(in TranslateComponent translatable)
        {
            if (translatable.canPlaySFX)
            {
                RuntimeWrappers.PlaySFX(translatable.sfxName);
            }
            if (translatable.canPlayVFX)
            {
                RuntimeWrappers.PlayVFX(translatable.vfxName, translatable.refObj.transform.position);
            }
            var translateParams = GetParams(translatable);
            RuntimeWrappers.TranslateObject(translateParams);
        }

        private TranslateParams GetParams(TranslateComponent translatable)
        {
            var translateParams = new TranslateParams()
            {
                translateFrom = translatable.startPosition,
                translateTo = translatable.targetPosition,
                speed = translatable.speed,
                translateTimes = translatable.repeatFor,
                shouldPingPong = translatable.translateType is TranslateType.PingPong or TranslateType.PingPongForever,
                shouldPause = translatable.pauseFor > 0f,
                pauseDistance = Vector3.Distance(translatable.startPosition, translatable.targetPosition),
                pauseForTime = translatable.pauseFor,
                targetObj = translatable.refObj,
                broadcastAt = translatable.broadcastAt,
                onTranslated = (isDone) =>
                {
                    OnTranslateDone(translatable.Broadcast, translatable.broadcastAt == BroadcastAt.End || isDone);
                }
            };
            return translateParams;
        }

        private void OnTranslateDone(string broadcast, bool removeOnceBroadcasted)
        {
            RuntimeOp.Resolve<Broadcaster>().Broadcast(broadcast, removeOnceBroadcasted);
        }
    }
}
