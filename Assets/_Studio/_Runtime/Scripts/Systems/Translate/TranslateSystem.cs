using UnityEngine;
using Leopotam.EcsLite;

namespace Terra.Studio
{
    public class TranslateSystem : IAbsRunsystem, IConditionalOp
    {
        public void Init(EcsWorld currentWorld, int entity)
        {
            var filter = currentWorld.Filter<TranslateComponent>().End();
            var pool = currentWorld.GetPool<TranslateComponent>();
            ref var entityRef = ref pool.Get(entity);
        }

        public void OnConditionalCheck(object data)
        {
            
        }
    }
}
