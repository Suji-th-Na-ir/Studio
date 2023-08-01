using System;
using UnityEngine;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Terra.Studio
{
    public class DataProvider
    {
        private const string EDITOR_DATA_FILE_PATH = "Editortime/ComponentDrawersVariants";
        private readonly Dictionary<string, string> ComponentTargets;

        public DataProvider()
        {
            var fileData = Resources.Load<TextAsset>(EDITOR_DATA_FILE_PATH).text;
            ComponentTargets = JsonConvert.DeserializeObject<Dictionary<string, string>>(fileData);
        }

        public string GetCovariance<T>(T instance)
        {
            foreach (var target in ComponentTargets)
            {
                if (target.Value.Equals(typeof(T).FullName))
                {
                    return target.Key;
                }
            }
            return null;
        }

        public Type GetVariance(string key)
        {
            if (ComponentTargets.ContainsKey(key))
            {
                var typeName = ComponentTargets[key];
                var type = Type.GetType(typeName);
                return type;
            }
            return default;
        }
    }
}
