using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Terra.Studio
{
    public class VisualisationView : View
    {
       
        private void Awake()
        {
            EditorOp.Register(this);
        }

        public override void Flush()
        {
            Destroy(this);
        }

     

        private void OnDestroy()
        {
            EditorOp.Unregister(this);
        }
    }
}
