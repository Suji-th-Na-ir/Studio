using UnityEngine;
using Leopotam.EcsLite;

namespace Terra.Studio
{
    public class TranslateSystem : BaseSystem
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
            var compsData = RuntimeOp.Resolve<ComponentsData>();
            compsData.ProvideEventContext(false, entityRef.EventContext);
            entityRef.CanExecute = true;
            entityRef.IsExecuted = true;
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
                RuntimeWrappers.PlayVFX(translatable.vfxName, translatable.RefObj.transform.position);
            }
            var translateParams = GetParams(ref translatable, entity);
            RuntimeWrappers.TranslateObject(translateParams);
        }

        private TranslateParams GetParams(ref TranslateComponent translatable, int entity)
        {
            var tr = translatable.RefObj.transform;
            var targetPos = tr.parent == null ? translatable.targetPosition : tr.TransformPoint(translatable.targetPosition);
            var pauseDistance = Vector3.Distance(translatable.startPosition, targetPos);
            var direction = targetPos - translatable.startPosition;
            translatable.pauseDistance = pauseDistance;
            translatable.direction = direction;
            if (translatable.startPosition != tr.position)
            {
                targetPos = tr.position + direction * pauseDistance;
            }
            var translateParams = new TranslateParams()
            {
                translateFrom = translatable.RefObj.transform.position,
                translateTo = targetPos,
                speed = translatable.speed,
                translateTimes = translatable.repeatFor,
                shouldPingPong = translatable.translateType is TranslateType.PingPong or TranslateType.PingPongForever,
                shouldPause = translatable.pauseFor > 0f,
                pauseDistance = pauseDistance,
                pauseForTime = translatable.pauseFor,
                targetObj = translatable.RefObj,
                onTranslated = (isDone) =>
                {
                    OnTranslateDone(isDone, entity);
                }
            };
            return translateParams;
        }

        private void OnTranslateDone(bool isDone, int entity)
        {
            ref var translatable = ref EntityAuthorOp.GetComponent<TranslateComponent>(entity);
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
                if (translatable.IsExecuted)
                {
                    continue;
                }
                var compsData = RuntimeOp.Resolve<ComponentsData>();
                compsData.ProvideEventContext(false, translatable.EventContext);
            }
        }
    }
}
