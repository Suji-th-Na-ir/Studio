using System;
using UnityEngine;
using RuntimeInspectorNamespace;

namespace Terra.Studio
{
    public class LabelField : InspectorField
    {
        public override void SetInteractable(bool on)
        {
           
        }

        public override bool SupportsType(Type type)
        {
            return type == typeof(HeaderAttribute);
        }
    }
}
