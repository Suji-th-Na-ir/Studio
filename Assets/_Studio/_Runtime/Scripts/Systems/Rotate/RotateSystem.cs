using UnityEngine;
using Leopotam.EcsLite;
using static Terra.Studio.RuntimeWrappers;
using static Terra.Studio.GlobalEnums;

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
            var tuple = ((int entity, string conditionType, GameObject go, string conditionData, object selection))data;
            if (tuple.conditionType.Equals("Terra.Studio.MouseAction"))
            {
                if (tuple.selection == null || tuple.selection as GameObject != tuple.go)
                {
                    return;
                }
            }
            var compsData = RuntimeOp.Resolve<ComponentsData>();
            compsData.ProvideEventContext(tuple.conditionType, null, false, (tuple.go, tuple.conditionData));
            var world = RuntimeOp.Resolve<RuntimeSystem>().World;
            var filter = world.Filter<RotateComponent>().End();
            var pool = world.GetPool<RotateComponent>();
            ref var entityRef = ref pool.Get(tuple.entity);
            entityRef.CanExecute = true;
            OnDemandRun(tuple.entity, ref entityRef);
        }

        public void OnDemandRun(int entityID, ref RotateComponent rotatable)
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
            if (rotatable.rotationType == RotationType.Oscillate)
            {
                rotateParams.shouldPingPong = true;
            }
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
