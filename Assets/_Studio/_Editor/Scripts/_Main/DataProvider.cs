using System;
using UnityEngine;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Terra.Studio
{
    public class DataProvider
    {
        private readonly Dictionary<string, string> ComponentTargets;
        private readonly Dictionary<string, string[]> EnumTargets;

        public DataProvider()
        {
            var compFileData = EditorOp.Load<TextAsset>("ComponentDrawersVariants").text;
            ComponentTargets = JsonConvert.DeserializeObject<Dictionary<string, string>>(compFileData);
            var enumFileData = EditorOp.Load<TextAsset>("EnumFieldsVariants").text;
            EnumTargets = JsonConvert.DeserializeObject<Dictionary<string, string[]>>(enumFileData);
        }

        public string GetCovariance<T>(T _)
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
            var actualKey = typeof(T).FullName + "." + instance.ToString();
            if (EnumTargets.ContainsKey(actualKey))
            {
                if (EnumTargets.TryGetValue(actualKey, out string[] value))
                {
                    if (!string.IsNullOrEmpty(value[0]))
                        return value[0];
                }
            }

            return null;
        }

        public string GetEnumConditionDataValue<T>(T instance) where T : Enum
        {
            var actualKey = typeof(T).FullName + "." + instance.ToString();
            if (EnumTargets.ContainsKey(actualKey))
            {
                if (EnumTargets.TryGetValue(actualKey, out string[] value))
                {
                    if (!string.IsNullOrEmpty(value[1]))
                        return value[1];
                }
            }

            return "";
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

        public bool TryGetEnum(string key, Type type, out object result)
        {
            result = null;

            foreach (var e in EnumTargets)
            {

                if (e.Key.Contains(type.ToString()) && EnumTargets.TryGetValue(e.Key, out string[] value))
                {
                    if (!string.IsNullOrEmpty(value[0]))
                    {
                        if (value[0] == key)
                        {
                            Enum.TryParse(type, e.Key.Replace(type.ToString() + ".", ""), out result);
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        
        #region Listen Types Dict

        public List<string> ListenToTypes
        {
            get
            {
                var newList = new List<string> {  "None", "Game Win", "Game Lose"};
                foreach (var item in listenDictionary)
                {
                    if(!newList.Contains(item.Value))
                        newList.Add(item.Value);
                }
                newList.Add("Custom");
                return newList;
            }
        }

        private Dictionary<string, string> listenDictionary = new ();
        private string prevListenType = "";

        public void UpdateToListenList(string _id, string _type)
        {
            if (string.IsNullOrEmpty(_type) || string.IsNullOrWhiteSpace(_type))
            {
                // Debug.Log("removing signal "+_type);
                // listenDictionary.Remove(_id);
                return;
            }

            if (!listenDictionary.ContainsKey(_id))
            {
                listenDictionary.Add(_id, _type);
            }
            else
            {
                listenDictionary[_id] = _type;
            }
        }

        public string GetListenString(int _index)
        {
            if (_index < ListenToTypes.Count)
            {
                return ListenToTypes[_index];
            }

            Debug.Log($"getlistensting index error {_index}");
            return ListenToTypes[0];
        }
        #endregion
        
    }
}
