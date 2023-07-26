using System;
using UnityEngine;

namespace Terra.Studio
{
    [Serializable]
    public class ExtendedComponentData : ComponentData
    {
        public virtual void SetTransform(string uniqueName, Transform transform)
        {
            SetVector3(uniqueName + ".position", transform.position);
            SetVector3(uniqueName + ".rotation", transform.eulerAngles);
            SetVector3(uniqueName + ".scale", transform.localScale);
        }
        
        public void GetTransform(string uniqueName, Transform transform)
        {
            transform.position = GetVector3(uniqueName + ".position");
            transform.eulerAngles = GetVector3(uniqueName + ".rotation");
            transform.localScale = GetVector3(uniqueName + ".scale");
        }
    }
}
