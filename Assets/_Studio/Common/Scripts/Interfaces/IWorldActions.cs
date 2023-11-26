using Leopotam.EcsLite;

namespace Terra.Studio
{
    public interface IWorldActions
    {
        public void OnHaltRequested(EcsWorld currentWorld);
    }
}
