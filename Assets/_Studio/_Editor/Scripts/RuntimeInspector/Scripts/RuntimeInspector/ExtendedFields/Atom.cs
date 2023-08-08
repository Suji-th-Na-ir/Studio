using System;
using RuntimeInspectorNamespace;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Serialization;

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

        [Serializable]
        public class Rotate
        {
            [HideInInspector] public RotateField field;
            [HideInInspector] public RotateComponentData data = new RotateComponentData();
        }
        
        [Serializable]
        public class Translate
        {
            [HideInInspector] public RotateField field;
            [HideInInspector] public TranslateComponentData data = new TranslateComponentData();
        }
    }
}
