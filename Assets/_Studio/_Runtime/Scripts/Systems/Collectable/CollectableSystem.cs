using Leopotam.EcsLite;

namespace Terra.Studio
{
    public class CollectableSystem : BaseSystem
    {
        public override void Init<T>(int entity)
        {
            base.Init<T>(entity);
            ref var collectable = ref entity.GetComponent<CollectableComponent>();
            if (collectable.canUpdateScore)
            {
                RuntimeOp.Resolve<CoreGameManager>().EnableModule<ScoreHandler>();
            }
        }

        public override void OnConditionalCheck(int entity, object data)
        {
            ref var entityRef = ref entity.GetComponent<CollectableComponent>();
            var compsData = RuntimeOp.Resolve<ComponentsData>();
            compsData.ProvideEventContext(false, entityRef.EventContext);
            entityRef.IsExecuted = true;
            OnDemandRun(entityRef, entity);
        }

        public void OnDemandRun(in CollectableComponent component, int entityID)
        {
            if (component.canPlaySFX)
            {
                RuntimeWrappers.PlaySFX(component.sfxName);
            }
            if (component.canPlayVFX)
            {
                RuntimeWrappers.PlayVFX(component.vfxName, component.RefObj.transform.position);
            }
            if (component.IsBroadcastable)
            {
                RuntimeOp.Resolve<Broadcaster>().Broadcast(component.Broadcast, true);
            }
            if (component.canUpdateScore)
            {
                RuntimeWrappers.AddScore(component.scoreValue);
            }
            EntityAuthorOp.Degenerate(entityID);
        }

        public override void OnHaltRequested(EcsWorld currentWorld)
        {
            var filter = currentWorld.Filter<CollectableComponent>().End();
            var collectablePool = currentWorld.GetPool<CollectableComponent>();
            var compsData = RuntimeOp.Resolve<ComponentsData>();
            foreach (var entity in filter)
            {
                var collectable = collectablePool.Get(entity);
                if (collectable.IsExecuted)
                {
                    continue;
                }
                compsData.ProvideEventContext(false, collectable.EventContext);
            }
        }
    }
}
