using System;
using System.Reflection;

namespace Terra.Studio
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
    public class AliasDrawerAttribute : Attribute
    {
        private readonly string alias;
        public string Alias { get { return alias; } }

        public AliasDrawerAttribute(string alias)
        {
            this.alias = alias;
        }
    }

    public static class AliasValueHelper
    {
        public static string GetAliasIfAny(this FieldInfo fieldInfo)
        {
            var attribs = fieldInfo.GetCustomAttributes(typeof(AliasDrawerAttribute), false) as AliasDrawerAttribute[];
            return attribs.Length > 0 ? attribs[0].Alias : null;
        }
    }
}
