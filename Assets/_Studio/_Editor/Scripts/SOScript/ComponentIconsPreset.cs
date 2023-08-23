using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Terra.Studio
{
    [Serializable]
    public struct IconData
    {
       public string type;
       public Sprite icon;
    }


    [CreateAssetMenu(fileName = "Component_Icon_SO", menuName = "Terra/Presets/Component Icons")]
    public class ComponentIconsPreset : ScriptableObject
    {
        public List<IconData> allIcons = new List<IconData>();

        public Sprite GetIcon(string key)
        {
            for (int i = 0; i < allIcons.Count; i++)
            {
                if (allIcons[i].type == key)
                    return allIcons[i].icon;
            }
            return null;
        }
    }
}
