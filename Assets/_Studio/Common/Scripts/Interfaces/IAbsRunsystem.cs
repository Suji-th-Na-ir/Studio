using Leopotam.EcsLite;

namespace Terra.Studio
{
    public interface IAbsRunsystem
    {
        public void Init(EcsWorld currentWorld, int entity);
    }
}
