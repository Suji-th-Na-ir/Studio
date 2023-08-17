using System;
using UnityEngine;
using Leopotam.EcsLite;
using System.Collections.Generic;

namespace Terra.Studio
{
    public class DestroyOnSystem : BaseSystem
    {
        public override Dictionary<int, Action<object>> IdToConditionalCallback { get; set; }

        public override void Init(EcsWorld currentWorld, int entity)
        {
            var pool = currentWorld.GetPool<DestroyOnComponent>();
            ref var destroyable = ref pool.Get(entity);
            if (destroyable.isRegistered)
            {
                return;
            }
            destroyable.isRegistered = true;
            var conditionType = destroyable.ConditionType;
            var conditionData = destroyable.ConditionData;
            var goRef = destroyable.refObj;
            var compsData = RuntimeOp.Resolve<ComponentsData>();
            IdToConditionalCallback ??= new();
            IdToConditionalCallback.Add(entity, (obj) =>
            {
                var go = obj == null ? null : obj as GameObject;
                OnConditionalCheck((entity, conditionType, conditionData, goRef, go));
            });
            compsData.ProvideEventContext(conditionType, IdToConditionalCallback[entity], true, (goRef, conditionData));
            if (destroyable.IsBroadcastable)
            {
                RuntimeOp.Resolve<Broadcaster>().SetBroadcastable(destroyable.Broadcast);
            }
        }

        public override void OnConditionalCheck(object data)
        {
            var (entity, conditionType, conditionData, go, selection) = ((int, string, string, GameObject, GameObject))data;
            if (conditionType.Equals("Terra.Studio.MouseAction"))
            {
                if (selection == null || selection != go)
                {
                    return;
                }
            }
            var compsData = RuntimeOp.Resolve<ComponentsData>();
            compsData.ProvideEventContext(conditionType, IdToConditionalCallback[entity], false, (go, conditionData));
            IdToConditionalCallback.Remove(entity);
            var world = RuntimeOp.Resolve<RuntimeSystem>().World;
            var destroyPool = world.GetPool<DestroyOnComponent>();
            OnDemandRun(entity, in destroyPool.Get(entity));
        }

        public void OnDemandRun(int entityID, in DestroyOnComponent component)
        {
            if (component.canPlaySFX)
            {
                RuntimeWrappers.PlaySFX(component.sfxName);
            }
            if (component.canPlayVFX)
            {
                RuntimeWrappers.PlayVFX(component.vfxName, component.refObj.transform.position);
            }
            //Unsubscribe to all listeners
            //Destroy gracefully
            UnityEngine.Object.Destroy(component.refObj);
            if (component.IsBroadcastable)
            {
                RuntimeOp.Resolve<Broadcaster>().Broadcast(component.Broadcast, true);
            }
            EntityAuthorOp.Degenerate(entityID);
        }

        public override void OnHaltRequested(EcsWorld currentWorld)
        {
            var filter = currentWorld.Filter<DestroyOnComponent>().End();
            var destroyPool = currentWorld.GetPool<DestroyOnComponent>();
            var compsData = RuntimeOp.Resolve<ComponentsData>();
            foreach (var entity in filter)
            {
                if (!IdToConditionalCallback.ContainsKey(entity)) continue;
                var destroyable = destroyPool.Get(entity);
                compsData.ProvideEventContext(destroyable.ConditionType, IdToConditionalCallback[entity], false, (destroyable.refObj, destroyable.ConditionData));
                IdToConditionalCallback.Remove(entity);
            }
        }
    }
}
