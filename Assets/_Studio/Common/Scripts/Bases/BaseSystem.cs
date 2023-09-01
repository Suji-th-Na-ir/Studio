using Leopotam.EcsLite;

namespace Terra.Studio
{
    public abstract class BaseSystem
    {
        public abstract void Init(int entity);
        public virtual void OnConditionalCheck(int entity, object data) { }
        public virtual void OnHaltRequested(EcsWorld currentWorld) { }
    }
}
