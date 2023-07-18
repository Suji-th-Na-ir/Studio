using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Terra.Studio
{
    public class SceneView : View
    {
        private void Awake()
        {
            Interop<EditorInterop>.Current.Register(this);
        }

        public override void Init()
        {
            //Initialize gizmo manager
        }

        public override void Draw()
        {

        }

        public override void Flush()
        {
            Destroy(this);
        }

        public override void Repaint()
        {

        }

        private void OnDestroy()
        {
            Interop<EditorInterop>.Current.Unregister(this);
        }
    }
}
