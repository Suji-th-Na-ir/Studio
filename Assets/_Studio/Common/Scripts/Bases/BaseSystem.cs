using System;
using Leopotam.EcsLite;
using System.Collections.Generic;

namespace Terra.Studio
{
    public abstract class BaseSystem
    {
        public abstract Dictionary<int, Action<object>> IdToConditionalCallback { get; set; }
        public abstract void Init(EcsWorld currentWorld, int entity);
        public virtual void OnConditionalCheck(object data) { }
        public virtual void OnHaltRequested(EcsWorld currentWorld) { }
    }
}
