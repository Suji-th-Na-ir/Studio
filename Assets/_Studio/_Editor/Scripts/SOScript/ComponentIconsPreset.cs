using System;
using UnityEngine;
using System.Collections.Generic;

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
        [SerializeField]
        private List<IconData> allIcons = new List<IconData>();

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
