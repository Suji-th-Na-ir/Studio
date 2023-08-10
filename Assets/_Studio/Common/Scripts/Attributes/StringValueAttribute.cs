using System;

namespace Terra.Studio
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class StringValueAttribute : Attribute
    {
        private readonly string value;
        private readonly Type storedType;
        public string Value { get { return value; } }
        public Type StoredType { get { return storedType; } }

        public StringValueAttribute(string value)
        {
            this.value = value;
        }

        public StringValueAttribute(string value, Type type)
        {
            this.value = value;
            storedType = type;
        }
    }

    public static class StringValueHelper
    {
        public static string GetStringValue(this Enum value)
        {
            var type = value.GetType();
            var fieldInfo = type.GetField(value.ToString());
            var attribs = fieldInfo.GetCustomAttributes(typeof(StringValueAttribute), false) as StringValueAttribute[];
            return attribs.Length > 0 ? attribs[0].Value : null;
        }

        public static Type GetStoredType(this Enum value)
        {
            var type = value.GetType();
            var fieldInfo = type.GetField(value.ToString());
            var attribs = fieldInfo.GetCustomAttributes(typeof(StringValueAttribute), false) as StringValueAttribute[];
            return attribs.Length > 0 ? attribs[0].StoredType : null;
        }
    }
}
