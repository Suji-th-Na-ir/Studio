using System;

namespace Terra.Studio
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class EditorDrawComponentAttribute : Attribute
    {
        private readonly string componentTarget;
        public string ComponentTarget { get { return componentTarget; } }

        public EditorDrawComponentAttribute(string componentTarget)
        {
            this.componentTarget = componentTarget;
        }
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class EditorEnumFieldAttribute : Attribute
    {
        private readonly string componentTarget;
        public string ComponentTarget { get { return componentTarget; } }

        public EditorEnumFieldAttribute(string componentTarget)
        {
            this.componentTarget = componentTarget;
        }
    }
}
