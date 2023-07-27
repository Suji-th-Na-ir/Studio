using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Terra.Studio;

namespace Terra.Studio.RTEditor
{
    public class Oscillate : MonoBehaviour, IComponent
    {
        [SerializeField] public Terra.Studio.OscillateComponent Component;

        public void ExportData()
        {
            var resp = Interop<EditorInterop>.Current.Resolve<SelectionHandler>().GetSceneData(this);
            Interop<SystemInterop>.Current.Resolve<CrossSceneDataHolder>().Set(resp);
        }
    }
}