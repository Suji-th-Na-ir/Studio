using UnityEngine;

namespace Terra.Studio
{
    public class ResourceLoader
    {
        public static T Load<T>(LoadFor loadFor, string path) where T : Object
        {
            var prefix = loadFor.GetStringValue();
            var finalPath = string.Concat(prefix, path);
            var obj = Resources.Load<T>(finalPath);
            if (obj == null && loadFor != LoadFor.Common)
            {
                var newObj = Load<T>(LoadFor.Common, path);
                Rulesets.ApplyRuleset(newObj);
                return newObj;
            }
            else
            {
                Rulesets.ApplyRuleset(obj);
                return obj;
            }
        }

        public static Object Load(LoadFor loadFor, ResourceTag tag)
        {
            var prefix = loadFor.GetStringValue();
            var finalPath = string.Concat(prefix, tag.GetStringValue());
            var type = tag.GetStoredType();
            var obj = Resources.Load(finalPath, type);
            if (obj == null && loadFor != LoadFor.Common)
            {
                var newObj = Load(LoadFor.Common, tag);
                Rulesets.ApplyRuleset(newObj);
                return newObj;
            }
            else
            {
                Rulesets.ApplyRuleset(obj);
                return obj;
            }
        }
    }
}
