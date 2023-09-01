using System;
using UnityEngine;
using Leopotam.EcsLite;
using System.Collections.Generic;

namespace Terra.Studio
{
    public class RespawnSystem : BaseSystem
    {
        public override Dictionary<int, Action<object>> IdToConditionalCallback { get; set; }
        private EventExecutorData eventExecutorData;

        public override void Init(EcsWorld currentWorld, int entity)
        {
            var pool = currentWorld.GetPool<RespawnComponent>();
            ref var entityRef = ref pool.Get(entity);
            var conditionType = entityRef.ConditionType;
            var conditionData = entityRef.ConditionData;
            var goRef = entityRef.refObj;
            var compsData = RuntimeOp.Resolve<ComponentsData>();
            eventExecutorData = new()
            {
                goRef = goRef,
                data = conditionData
            };
            IdToConditionalCallback ??= new();
            IdToConditionalCallback.Add(entity, (obj) =>
            {
                OnConditionalCheck((entity, conditionType, conditionData, goRef));
            });
            compsData.ProvideEventContext(conditionType, IdToConditionalCallback[entity], true, eventExecutorData);
            if (entityRef.IsBroadcastable)
            {
                RuntimeOp.Resolve<Broadcaster>().SetBroadcastable(entityRef.Broadcast);
            }
        }

        public override void OnConditionalCheck(object data)
        {
            var (entity, _, _, _) = ((int, string, string, GameObject))data;
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

        public override void OnHaltRequested(EcsWorld currentWorld)
        {
            var filter = currentWorld.Filter<RespawnComponent>().End();
            var respawnPool = currentWorld.GetPool<RespawnComponent>();
            var compsData = RuntimeOp.Resolve<ComponentsData>();
            foreach (var entity in filter)
            {
                if (!IdToConditionalCallback.ContainsKey(entity)) continue;
                var respawn = respawnPool.Get(entity);
                compsData.ProvideEventContext(respawn.ConditionType, IdToConditionalCallback[entity], false, eventExecutorData);
                IdToConditionalCallback.Remove(entity);
            }
        }
    }
}
