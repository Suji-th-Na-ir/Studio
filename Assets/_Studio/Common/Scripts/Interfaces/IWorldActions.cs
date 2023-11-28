using Leopotam.EcsLite;

namespace Terra.Studio
{
    public interface IWorldActions
    {
        public void OnHaltRequested(EcsWorld currentWorld);
        public void OnEntityQueuedToDestroy(int entity);
    }
}
