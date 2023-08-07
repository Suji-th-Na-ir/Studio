using System;
using UnityEngine;
using Leopotam.EcsLite;
using System.Collections.Generic;

namespace Terra.Studio
{
    public class RespawnSystem : BaseSystem, IAbsRunsystem, IConditionalOp
    {
        public override Dictionary<int, Action<object>> IdToConditionalCallback { get; set; }

        public void Init(EcsWorld currentWorld, int entity)
        {
            var pool = currentWorld.GetPool<RespawnComponent>();
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
            var pool = world.GetPool<RespawnComponent>();
            OnDemandRun(in pool.Get(entity));
        }

        public void OnDemandRun(in RespawnComponent component)
        {
            if (component.canPlaySFX)
            {
                RuntimeWrappers.PlaySFX(component.sfxName);
            }
            if (component.canPlayVFX)
            {
                RuntimeWrappers.PlaySFX(component.vfxName);
            }
            if (component.IsBroadcastable)
            {
                RuntimeOp.Resolve<Broadcaster>().Broadcast(component.Broadcast, true);
            }
            var respawnPoint = RuntimeOp.Resolve<GameData>().RespawnPoint;
            RuntimeWrappers.RespawnPlayer(respawnPoint);
        }
    }
}