using System;
using Leopotam.EcsLite;
using System.Collections.Generic;

namespace Terra.Studio
{
    public static class Utility
    {
        public static Dictionary<Type, object> Add<T>(this Dictionary<Type, object> dict, T instance) where T : IEcsRunSystem
        {
            dict ??= new();
            dict.Add(typeof(T), instance);
            return dict;
        }
    }
}
