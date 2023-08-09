using System;
using UnityEngine;
using Leopotam.EcsLite;
using System.Collections.Generic;

namespace Terra.Studio
{
    public class CheckpointSystem : BaseSystem, IAbsRunsystem, IConditionalOp
    {
        public override Dictionary<int, Action<object>> IdToConditionalCallback { get; set; }

        public void Init(EcsWorld currentWorld, int entity)
        {
            var pool = currentWorld.GetPool<CheckpointComponent>();
            ref var entityRef = ref pool.Get(entity);
            var conditionType = entityRef.ConditionType;
            var conditionData = entityRef.ConditionData;
            var goRef = entityRef.refObj;
            var compsData = RuntimeOp.Resolve<ComponentsData>();
            IdToConditionalCallback ??= new();
            IdToConditionalCallback.Add(entity, (obj) =>
            {
                OnConditionalCheck((entity, conditionType, conditionData, goRef));
            });
            compsData.ProvideEventContext(conditionType, IdToConditionalCallback[entity], true, (goRef, conditionData));
            if (entityRef.IsBroadcastable)
            {
                RuntimeOp.Resolve<Broadcaster>().SetBroadcastable(entityRef.Broadcast);
            }
        }

        public void OnConditionalCheck(object data)
        {
            var (entity, conditionType, conditionData, go) = ((int, string, string, GameObject))data;
            var compsData = RuntimeOp.Resolve<ComponentsData>();
            compsData.ProvideEventContext(conditionType, IdToConditionalCallback[entity], false, (go, conditionData));
            IdToConditionalCallback.Remove(entity);
            var world = RuntimeOp.Resolve<RuntimeSystem>().World;
            var checkpointPool = world.GetPool<CheckpointComponent>();
            OnDemandRun(in checkpointPool.Get(entity));
        }

        public void OnDemandRun(in CheckpointComponent component)
        {
            if (component.canPlaySFX)
            {
                RuntimeWrappers.PlaySFX(component.sfxName);
            }
            if (component.canPlayVFX)
            {
                RuntimeWrappers.PlayVFX(component.vfxName, component.refObj.transform.position);
            }
            RuntimeOp.Resolve<GameData>().RespawnPoint = component.respawnPoint;
            if (component.IsBroadcastable)
            {
                RuntimeOp.Resolve<Broadcaster>().Broadcast(component.Broadcast, true);
            }
        }

        public void OnHaltRequested(EcsWorld currentWorld)
        {
            var filter = currentWorld.Filter<CheckpointComponent>().End();
            var checkPointPool = currentWorld.GetPool<CheckpointComponent>();
            var compsData = RuntimeOp.Resolve<ComponentsData>();
            foreach (var entity in filter)
            {
                if (!IdToConditionalCallback.ContainsKey(entity)) continue;
                var checkpoint = checkPointPool.Get(entity);
                compsData.ProvideEventContext(checkpoint.ConditionType, IdToConditionalCallback[entity], false, (checkpoint.refObj, checkpoint.ConditionData));
                IdToConditionalCallback.Remove(entity);
            }
        }
    }
}
