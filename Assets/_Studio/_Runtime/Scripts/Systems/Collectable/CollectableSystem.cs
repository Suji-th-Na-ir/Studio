namespace Terra.Studio
{
    public class CollectableSystem : BaseSystem<CollectableComponent>
    {
        public override void Init(int entity)
        {
            base.Init(entity);
            ref var collectable = ref entity.GetComponent<CollectableComponent>();
            if (collectable.canUpdateScore)
            {
                RuntimeOp.Resolve<CoreGameManager>().EnableModule<ScoreHandler>();
            }
        }

        protected override void OnConditionalCheck(int entity, object data)
        {
            base.OnConditionalCheck(entity, data);
            ref var entityRef = ref entity.GetComponent<CollectableComponent>();
            OnDemandRun(entityRef, entity);
        }

        public void OnDemandRun(in CollectableComponent component, int entityID)
        {
            if (component.canUpdateScore)
            {
                RuntimeOp.Resolve<ScoreHandler>().AddScore(component.scoreValue);
            }
            PlayFXIfExists(component, 0);
            Broadcast(component);
            DeleteEntity(entityID);
        }
    }
}
