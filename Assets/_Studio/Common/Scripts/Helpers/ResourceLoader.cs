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
                return Load<T>(LoadFor.Common, path);
            }
            else
            {
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
                return Load(LoadFor.Common, tag);
            }
            else
            {
                return obj;
            }
        }
    }
}
