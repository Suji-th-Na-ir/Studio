using System;

namespace Terra.Studio
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class OnValueChangedAttribute : Attribute
    {
        public bool UpdateBroadcast;
        public bool UpdateListener;

        public Action<string, string> OnValueUpdated<T>(T instance) where T : BaseBehaviour
        {
            if (UpdateBroadcast)
            {
                return instance.OnBroadcastUpdated;
            }
            else if (UpdateListener)
            {
                return instance.OnListenerUpdated;
            }
            return null;
        }
    }
}