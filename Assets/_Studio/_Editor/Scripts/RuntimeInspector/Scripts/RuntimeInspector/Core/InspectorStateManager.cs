using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RuntimeInspectorNamespace
{
    public class InspectorStateManager : MonoBehaviour
    {
        private Dictionary<string, object> savedItems = new Dictionary<string, object>();

        public void SetItem(string _key, [SerializeField] System.Object _object)
        {
            // Debug.Log("state manager "+this.gameObject.GetInstanceID());
            // Debug.Log("saving item key: "+_key+" : value : "+_object.ToString());
            savedItems[_key] = _object;
        }

        public T GetItem<T>(string _key)
        {
            if (savedItems.ContainsKey(_key))
            {
                // Debug.Log("return key " + _key + " : value " + (T)savedItems[_key]);
                return (T)savedItems[_key];
            }
            else
                return default;
        }
    }
}
