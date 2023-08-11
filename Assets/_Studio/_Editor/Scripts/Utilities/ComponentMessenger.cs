using RuntimeInspectorNamespace;
using System;
using System.Collections.Generic;
using UnityEngine;

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
            
            Debug.Log($"data revieced {typeof(T)} - {_data}");
        }   
    }
}
