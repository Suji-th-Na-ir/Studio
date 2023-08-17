using System;
using UnityEngine;
using Leopotam.EcsLite;
using System.Collections.Generic;

namespace Terra.Studio
{
    public class CollectableSystem : BaseSystem
    {
        public override Dictionary<int, Action<object>> IdToConditionalCallback { get; set; }

        public override void Init(EcsWorld currentWorld, int entity)
        {
            var filter = currentWorld.Filter<CollectableComponent>().End();
            var collectablePool = currentWorld.GetPool<CollectableComponent>();
            ref var collectable = ref collectablePool.Get(entity);
            if (collectable.isRegistered)
            {
                return;
            }
            collectable.isRegistered = true;
            var conditionType = collectable.ConditionType;
            var goRef = collectable.refObject;
            var conditionData = collectable.ConditionData;
            var compsData = RuntimeOp.Resolve<ComponentsData>();
            IdToConditionalCallback ??= new();
            IdToConditionalCallback.Add(entity, (obj) =>
            {
                OnConditionalCheck((entity, conditionType, goRef, conditionData, obj));
            });
            compsData.ProvideEventContext(conditionType, IdToConditionalCallback[entity], true, (goRef, conditionData));
            if (collectable.IsBroadcastable)
            {
                RuntimeOp.Resolve<Broadcaster>().SetBroadcastable(collectable.Broadcast);
            }
        }

        public override void OnConditionalCheck(object data)
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
            var collectablePool = world.GetPool<CollectableComponent>();
            OnDemandRun(entity, in collectablePool.Get(entity));
        }

        public void OnDemandRun(int entityID, in CollectableComponent component)
        {
            if (component.canPlaySFX)
            {
                RuntimeWrappers.PlaySFX(component.sfxName);
            }
            if (component.canPlayVFX)
            {
                RuntimeWrappers.PlayVFX(component.vfxName, component.refObject.transform.position);
            }
            if (component.IsBroadcastable)
            {
                RuntimeOp.Resolve<Broadcaster>().Broadcast(component.Broadcast, true);
            }
            if (component.canUpdateScore)
            {
                RuntimeWrappers.AddScore(component.scoreValue);
            }
            UnityEngine.Object.Destroy(component.refObject);
            EntityAuthorOp.Degenerate(entityID);
        }

        public override void OnHaltRequested(EcsWorld currentWorld)
        {
            var filter = currentWorld.Filter<CollectableComponent>().End();
            var collectablePool = currentWorld.GetPool<CollectableComponent>();
            var compsData = RuntimeOp.Resolve<ComponentsData>();
            foreach (var entity in filter)
            {
                if (!IdToConditionalCallback.ContainsKey(entity)) continue;
                var collectable = collectablePool.Get(entity);
                compsData.ProvideEventContext(collectable.ConditionType, IdToConditionalCallback[entity], false, (collectable.refObject, collectable.ConditionData));
                IdToConditionalCallback.Remove(entity);
            }
        }
    }
}
