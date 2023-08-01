using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Terra.Studio
{
    public class SceneView : View
    {
        private void Awake()
        {
            EditorOp.Register(this);
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
            EditorOp.Unregister(this);
        }
    }
}
