using PlayShifu.Terra;

namespace Terra.Studio
{
    public class CollideSystem : BaseSystem<CollideComponent>
    {
        public override void Init(int entity)
        {
            base.Init(entity);
            ref var entityRef = ref entity.GetComponent<CollideComponent>();
            var rb = entityRef.RefObj.AddRigidbody();
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        protected override void OnConditionalCheck(int entity, object data)
        {
            base.OnConditionalCheck(entity, data);
            ref var entityRef = ref entity.GetComponent<CollideComponent>();
            OnDemandRun(in entityRef);
        }

        public void OnDemandRun(in CollideComponent component)
        {
            PlayFXIfExists(component, 0);
            Broadcast(component);
        }
    }
}