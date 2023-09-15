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
        private readonly string componentData;
        public string ComponentTarget { get { return componentTarget; } }
        public string ComponentData { get { return componentData; } }

        public EditorEnumFieldAttribute(string componentTarget, string componentData)
        {
            this.componentTarget = componentTarget;
            this.componentData = componentData;
        }

        public EditorEnumFieldAttribute(string componentTarget)
        {
            this.componentTarget = componentTarget;
        }
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property| AttributeTargets.Class|AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
    public class DisplayNameAttribute : Attribute
    {
        private readonly string displayName;

        public string DisplayName { get { return displayName; } }

        public DisplayNameAttribute(string _displayname)
        {
            this.displayName = _displayname;
        }
    }
}
