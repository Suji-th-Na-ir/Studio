using System;
using UnityEngine;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Terra.Studio
{
    public class DataProvider
    {
        private const string EDITOR_DATA_FILE_PATH = "Editortime/ComponentDrawersVariants";
        private const string EDITOR_ENUM_FILE_PATH = "Editortime/EnumFieldsVariants";
        private readonly Dictionary<string, string> ComponentTargets;
        private readonly Dictionary<string, string> EnumTargets;

        public DataProvider()
        {
            var compFileData = Resources.Load<TextAsset>(EDITOR_DATA_FILE_PATH).text;
            ComponentTargets = JsonConvert.DeserializeObject<Dictionary<string, string>>(compFileData);
            var enumFileData = Resources.Load<TextAsset>(EDITOR_ENUM_FILE_PATH).text;
            EnumTargets = JsonConvert.DeserializeObject<Dictionary<string, string>>(enumFileData);
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

        public string GetEnumValue<T>(T instance) where T : Enum
        {
            foreach (var target in EnumTargets)
            {
                if (target.Value.Equals(instance.ToString()))
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

        public string GetEnum(string key)
        {
            if (EnumTargets.ContainsKey(key))
            {
                var typeName = EnumTargets[key];
                return typeName;
            }
            return null;
        }
    }
}
