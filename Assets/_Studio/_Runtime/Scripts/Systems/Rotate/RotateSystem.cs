using UnityEngine;
using Leopotam.EcsLite;
using static Terra.Studio.GlobalEnums;
using static Terra.Studio.RuntimeWrappers;

namespace Terra.Studio
{
    public class RotateSystem : IAbsRunsystem, IConditionalOp
    {
        public void Init(EcsWorld currentWorld, int entity)
        {
            var filter = currentWorld.Filter<RotateComponent>().End();
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
            RuntimeOp.Resolve<ComponentsData>()
                .ProvideEventContext(conditionType, (obj) =>
                {
                    OnConditionalCheck((entity, conditionType, goRef, conditionData, obj));
                },
                true, (goRef, conditionData));
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
            compsData.ProvideEventContext(conditionType, null, false, (go, conditionData));
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
                PlaySFX(rotatable.sfxName);
            }
            if (rotatable.canPlayVFX)
            {
                PlayVFX(rotatable.vfxName, rotatable.refObj.transform.position);
            }
            var rotateParams = GetParams(rotatable);
            if (rotatable.rotationType == RotationType.Oscillate)
            {
                rotateParams.shouldPingPong = true;
            }
            RotateObject(rotateParams);
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
                onRotated = OnRotationDone,
                shouldPause = rotatable.pauseFor > 0,
                pauseForTime = rotatable.pauseFor,
                targetObj = rotatable.refObj,
                broadcastAt = rotatable.broadcastAt
            };
            return rotateParams;
        }

        private void OnRotationDone()
        {
            Debug.Log("Rotated obj!!!");
        }
    }
}
