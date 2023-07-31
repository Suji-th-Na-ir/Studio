using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Terra.Studio;
using RuntimeInspectorNamespace;
using Newtonsoft.Json;

namespace RuntimeInspectorNamespace
{
    public class Oscillate : MonoBehaviour, IComponent
    {
        public Terra.Studio.OscillateComponent Component;

        private void Start()
        {
            Component.fromPoint = transform.position;
        }

        public (string type, string data) Export()
        {
            var type = "Terra.Studio.Oscillate";
            var data = JsonConvert.SerializeObject(Component);
            return (type, data);
        }

        public void ExportData()
        {
            var resp = Interop<EditorInterop>.Current.Resolve<SelectionHandler>().GetSceneData(this);
            Interop<SystemInterop>.Current.Resolve<CrossSceneDataHolder>().Set(resp);
        }
    }
}