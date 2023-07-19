using System;
using System.Collections;
using System.Collections.Generic;
using RuntimeInspectorNamespace;
using UnityEngine;

namespace Terra.Studio
{
    public class InspectorView : View
    {
        [SerializeField] private RuntimeInspector _inspector;

        private void Awake()
        {
            Interop<EditorInterop>.Current.Register<InspectorView>(this);
        }

        public override void Init()
        {
            _inspector.gameObject.SetActive(true);
        }

        public override void Draw()
        {
            
        }

        public override void Flush()
        {
            
        }

        public override void Repaint()
        {
            
        }

        private void OnDestroy()
        {
            Interop<EditorInterop>.Current.Unregister<InspectorView>(this);
        }
    }
}
