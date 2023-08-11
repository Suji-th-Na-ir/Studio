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
            [HideInInspector] public PlaySFXData data;
        }
        
        [Serializable]
        public class PlayVfx
        {
            [HideInInspector] public PlayVFXField field;
            [HideInInspector] public PlayVFXData data;
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
    public struct PlaySFXData
    {
        public bool canPlay;
        public string clipName;
        public int clipIndex;
    }
    
    [Serializable]
    public struct PlayVFXData
    {
        public bool canPlay;
        public string clipName;
        public int clipIndex;
    }
    
    [Serializable]
    public struct RotateComponentData
    {
        public int rotateType;
        public Axis axis;
        public Direction direction;
        public float degrees;
        public float speed;
        public int repeat;
        public float pauseBetween;
        public string broadcast;
        public string listenTo;
        public BroadcastAt broadcastAt;
    }
    
    [Serializable]
    public struct TranslateComponentData
    {
        public int translateType;
        public Vector3 moveTo;
        public float pauseFor;
        public float speed;
        public int repeat;
        public string broadcast;
        public BroadcastAt broadcastAt;
        public Vector3 targetPosition;
        public string listenTo;
    }

}
