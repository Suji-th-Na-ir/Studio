using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Terra.Studio
{
    public class EditorUtils 
    {
        public static void ApplyLayerToChildren(Transform gm,string layerName)
        {
            gm.gameObject.layer = LayerMask.NameToLayer(layerName);
            for (int i = 0; i < gm.childCount; i++)
            {
                Transform child = gm.GetChild(i);
                ApplyLayerToChildren(child, layerName);
            }
        }
    }
}
