using Leopotam.EcsLite;

namespace Terra.Studio
{
    public class DecreasePlayerHealthSystem : BaseSystem
    {
        public override void OnConditionalCheck(int entity, object data)
        {
            ref var entityRef = ref EntityAuthorOp.GetComponent<DecreasePlayerHealthComponent>(entity);
            OnDemandRun(in entityRef);
        }

        public void OnDemandRun(in DecreasePlayerHealthComponent component)
        {
            var value = (int)component.playerHealthModifier * -1;
            RuntimeOp.Resolve<PlayerData>().OnPlayerHealthChangeRequested?.Invoke(value);
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
        }

        public override void OnHaltRequested(EcsWorld currentWorld)
        {
            var filter = currentWorld.Filter<DecreasePlayerHealthComponent>().End();
            var compPool = currentWorld.GetPool<DecreasePlayerHealthComponent>();
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
