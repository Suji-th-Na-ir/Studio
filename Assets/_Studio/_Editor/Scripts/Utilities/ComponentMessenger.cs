using RuntimeInspectorNamespace;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;

namespace Terra.Studio
{
    public static class ComponentMessenger
    {
        public static void UpdateCompData<T>(T _data)
        {
            // var type = typeof(T);
            // List<GameObject> selectedObjecs = EditorOp.Resolve<SelectionHandler>().GetSelectedObjects();
            //
            // foreach (var obj in selectedObjecs)
            // {
            //     // if (obj.GetComponent((Type)_data) != null)
            //     {
            //         // Type rotate = obj.GetComponent((Type)_data);
            //         // rotate.Type.data = _data;
            //     }
            // }
            
            // Debug.Log($"data revieced {typeof(T)} - {_data}");
            //
            // List<GameObject> selectedObjecs = EditorOp.Resolve<SelectionHandler>().GetSelectedObjects();
            // foreach (var obj in selectedObjecs)
            // {
                // Component cc = obj.GetComponent();
                // if (cc != null)
                // {
                //     Type type = _data.GetType();
                //     FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public);
                //
                //     foreach (FieldInfo field in fields)
                //     {
                //         object value = field.GetValue(_data);
                //         Debug.Log($"Field: {field.Name}, Value: {value}");
                //         Debug.Log($"typeof {typeof(T)}");
                //     }
                // }
            // }
        }
    }
}
