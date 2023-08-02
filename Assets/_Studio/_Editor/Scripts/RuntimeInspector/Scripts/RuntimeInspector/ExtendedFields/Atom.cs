using System;
using RuntimeInspectorNamespace;
using UnityEngine;

namespace Terra.Studio.RTEditor
{
    public class Atom
    {
        [Serializable]
        public class PlaySfx
        {
            [HideInInspector] public PlaySFXField field;
            [HideInInspector] public bool canPlay;
            [HideInInspector] public string clipName;
            [HideInInspector] public int clipIndex; 
        }
        
        [Serializable]
        public class PlayVfx
        {
            [HideInInspector] public PlayVFXField field;
            [HideInInspector] public bool canPlay;
            [HideInInspector] public string clipName;
            [HideInInspector] public int clipIndex;
        }
    }
}
