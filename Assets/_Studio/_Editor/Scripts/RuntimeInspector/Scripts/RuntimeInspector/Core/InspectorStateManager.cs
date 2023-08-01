using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RuntimeInspectorNamespace
{
    public class InspectorStateManager : MonoBehaviour
    {
        [SerializeField]
        private Dictionary<string, object> savedItems = new Dictionary<string, object>();

        public void SetItem(string _key, [SerializeField] System.Object _object)
        {
            savedItems[_key] = _object;
        }

        public T GetItem<T>(string _key)
        {
            if (savedItems.ContainsKey(_key))
            {
                return (T)savedItems[_key];
            }
            else
                return default;
        }
    }
}
