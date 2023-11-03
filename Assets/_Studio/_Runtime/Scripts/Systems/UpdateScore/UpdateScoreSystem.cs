using Leopotam.EcsLite;

namespace Terra.Studio
{
    public class UpdateScoreSystem : BaseSystem
    {
        public override void Init<T>(int entity)
        {
            base.Init<T>(entity);
            ref var entityRef = ref entity.GetComponent<UpdateScoreComponent>();
            if (entityRef.AddScoreValue > 0)
            {
                RuntimeOp.Resolve<CoreGameManager>().EnableModule<ScoreHandler>();
            }
        }

        public override void OnConditionalCheck(int entity, object data)
        {
            ref var entityRef = ref entity.GetComponent<UpdateScoreComponent>();
            var comp = RuntimeOp.Resolve<ComponentsData>();
            comp.ProvideEventContext(false, entityRef.EventContext);
            entityRef.IsExecuted = true;
            OnDemandRun(in entityRef);
        }

        public void OnDemandRun(in UpdateScoreComponent component)
        {
            RuntimeOp.Resolve<ScoreHandler>().AddScore(component.AddScoreValue);
        }

        public override void OnHaltRequested(EcsWorld currentWorld)
        {
            var filter = currentWorld.Filter<UpdateScoreComponent>().End();
            var compPool = currentWorld.GetPool<UpdateScoreComponent>();
            foreach (var entity in filter)
            {
                var component = compPool.Get(entity);
                if (component.IsExecuted)
                {
                    continue;
                }
                var compsData = RuntimeOp.Resolve<ComponentsData>();
                compsData.ProvideEventContext(false, component.EventContext);
            }
        }
    }
}
