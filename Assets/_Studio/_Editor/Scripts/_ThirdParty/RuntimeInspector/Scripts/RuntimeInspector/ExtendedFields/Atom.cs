using System;
using RuntimeInspectorNamespace;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Serialization;
using PlayShifu.Terra;

namespace Terra.Studio
{
    public class Atom
    {
        [Serializable]
        public class PlaySfx
        {
            [HideInInspector] public static List<PlaySfx> AllInstances = new();
            [HideInInspector] public PlaySFXField field;
            [HideInInspector] public PlaySFXData data;
            [HideInInspector] public GameObject target;

            public void Setup(GameObject _target)
            {
                target = _target;
                if (!AllInstances.Contains(this))
                    AllInstances.Add(this);
            }
        }

        [Serializable]
        public class PlayVfx
        {
            [HideInInspector] public static List<PlayVfx> AllInstances = new();
            [HideInInspector] public PlayVFXField field;
            [HideInInspector] public PlayVFXData data;
            [HideInInspector] public GameObject target;

            public void Setup(GameObject _target)
            {
                target = _target;
                if (!AllInstances.Contains(this))
                    AllInstances.Add(this);
            }
        }

        [Serializable]
        public class Rotate
        {
            [HideInInspector] public RotateField field;
            [HideInInspector] public RotateComponentData data = new();
        }

        [Serializable]
        public class Translate
        {
            [HideInInspector] public RotateField field;
            [HideInInspector] public TranslateComponentData data = new();
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
        public Listen listen;
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
        public string listenTo;
        public Listen listen;
    }

}
