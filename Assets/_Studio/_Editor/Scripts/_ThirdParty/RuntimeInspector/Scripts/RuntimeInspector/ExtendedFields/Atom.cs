using System;
using RuntimeInspectorNamespace;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Serialization;

namespace Terra.Studio
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
    
    // define component data classes here
    [Serializable]
    public class RotateComponentData
    {
        public int rotateType;
        public Axis axis;
        public Direction direction;
        public float degrees = 0f;
        public float speed = 0f;
        public int repeat = 0;
        public float pauseBetween = 0f;
        public string broadcast = "";
        public string listenTo = "";
        public BroadcastAt broadcastAt;
    }
    
    [Serializable]
    public class TranslateComponentData
    {
        public int translateType;
        public Vector3 moveTo = Vector3.zero;
        public float pauseFor = 0f;
        public float speed = 0f;
        public int repeat = 0;
        public string broadcast = "";
        public BroadcastAt broadcastAt;
        public Vector3 targetPosition;
        public string listenTo;
    }

}
