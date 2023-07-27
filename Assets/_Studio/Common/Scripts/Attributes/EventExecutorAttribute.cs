using System;

namespace Terra.Studio
{
    [AttributeUsage(AttributeTargets.Struct, AllowMultiple = true, Inherited = false)]
    public class EventExecutorAttribute : Attribute
    {
        private readonly string eventTarget;
        public string EventTarget { get { return eventTarget; } }

        public EventExecutorAttribute(string eventTarget)
        {
            this.eventTarget = eventTarget;
        }
    }
}
