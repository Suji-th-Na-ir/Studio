using UnityEngine;

namespace Terra.Studio
{
    public abstract class ComponentPresetsSO<T> : ScriptableObject where T : struct
    {
        public abstract T Value { get; }
    }
}