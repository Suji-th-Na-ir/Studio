using UnityEngine;
using Leopotam.EcsLite;

namespace Terra.Studio
{
    public class RotateSystem : BaseSystem
    {
        public override void OnConditionalCheck(int entity, object data)
        {
            ref var entityRef = ref EntityAuthorOp.GetComponent<RotateComponent>(entity);
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
                RuntimeWrappers.PlayVFX(rotatable.vfxName, rotatable.RefObj.transform.position);
            }
            var rotateParams = GetParams(rotatable, entity);
            RuntimeWrappers.RotateObject(rotateParams);
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
                targetObj = rotatable.RefObj,
                shouldPingPong = rotatable.rotationType is RotationType.Oscillate or RotationType.OscillateForever,
                onRotated = (isDone) =>
                {
                    OnRotationDone(isDone, entity);
                }
            };
            return rotateParams;
        }

        private void OnRotationDone(bool isDone, int entity)
        {
            ref var rotatable = ref EntityAuthorOp.GetComponent<RotateComponent>(entity);
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
                if (rotatable.IsExecuted)
                {
                    continue;
                }
                var compsData = RuntimeOp.Resolve<ComponentsData>();
                compsData.ProvideEventContext(false, rotatable.EventContext);
            }
        }
    }
}
