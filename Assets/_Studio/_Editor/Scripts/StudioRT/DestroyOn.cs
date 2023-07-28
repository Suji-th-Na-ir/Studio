using System.Collections;
using System.Collections.Generic;
using RuntimeInspectorNamespace;
using Terra.Studio.RTEditor;
using UnityEngine;


namespace Terra.Studio.RTEditor
{
    public enum DestroyOnEventType
    {
        None, 
        OnPlayerCollide,
        OnClick
    }
    public class DestroyOn : MonoBehaviour, IComponent
    {
        public DestroyOnEventType Start = DestroyOnEventType.None;
        
        public void ExportData()
        {
            
        }
    }
}