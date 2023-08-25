using UnityEngine;

namespace Terra.Studio
{
    public class ResourceLoader
    {
        public static T Load<T>(LoadFor loadFor, string path) where T : Object
        {
            var prefix = loadFor.GetStringValue();
            if (path.Contains(prefix))
            {
                path = path.Replace(prefix, string.Empty);
            }
            var finalPath = string.Concat(prefix, path);
            var obj = Resources.Load<T>(finalPath);
            if (obj == null && loadFor != LoadFor.Common)
            {
                obj = Load<T>(LoadFor.Common, path);
            }
            return obj;
        }

        public static Object Load(LoadFor loadFor, ResourceTag tag, string appendPath = null)
        {
            var prefix = loadFor.GetStringValue();
            var finalPath = string.Concat(prefix, tag.GetStringValue());
            if (!string.IsNullOrEmpty(appendPath))
            {
                finalPath = string.Concat(finalPath, appendPath);
            }
            var type = tag.GetStoredType();
            var obj = Resources.Load(finalPath, type);
            if (obj == null && loadFor != LoadFor.Common)
            {
                obj = Load(LoadFor.Common, tag, appendPath);
            }
            return obj;
        }
    }
}
