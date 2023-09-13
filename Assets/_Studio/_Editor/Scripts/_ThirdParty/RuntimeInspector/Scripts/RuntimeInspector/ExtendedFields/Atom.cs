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
        public class StartOn
        {
            public static List<StartOn> AllInstances = new();
            [HideInInspector] public StartOnField field;
            [HideInInspector] public StartOnData data;
            [HideInInspector] public GameObject target;
            [HideInInspector] public List<string> StartList = new List<string>();
            [HideInInspector] public string componentType = null;

            public void Setup(GameObject _target, List<string> _list, string _componentType,bool updateListen)
            {
                StartList = _list;
                target = _target;
                if (!AllInstances.Contains(this))
                    AllInstances.Add(this);
                componentType = _componentType;
                if(updateListen)
                EditorOp.Resolve<UILogicDisplayProcessor>().UpdateListenerString(data.listenName, ""
                                , new ComponentDisplayDock() { componentGameObject = _target, componentType = _componentType });
                data = new();
                if (StartList.Count >= data.startIndex)
                {
                    data.startName = StartList[data.startIndex];
                }
            }
        }

        [Serializable]
        public class PlaySfx
        {
            public static List<PlaySfx> AllInstances = new();
            [HideInInspector] public PlaySFXField field;
            [HideInInspector] public PlaySFXData data;
            [HideInInspector] public GameObject target;
            public Type componentType = null;
            public string fieldName;

            public void Setup<T>(GameObject _target, string fieldName = null)
            {
                target = _target;
                componentType = typeof(T);
                this.fieldName = fieldName;
                if (!AllInstances.Contains(this))
                    AllInstances.Add(this);
                data = new();
                data.clipName = Helper.GetSfxClipNameByIndex(data.clipIndex);
            }
        }

        [Serializable]
        public class PlayVfx
        {
            public static List<PlayVfx> AllInstances = new();
            [HideInInspector] public PlayVFXField field;
            [HideInInspector] public PlayVFXData data;
            [HideInInspector] public GameObject target;
            public Type componentType = null;
            public string fieldName;

            public void Setup<T>(GameObject _target, string fieldName = null)
            {
                target = _target;
                componentType = typeof(T);
                this.fieldName = fieldName;
                if (!AllInstances.Contains(this))
                    AllInstances.Add(this);
                data = new();
                data.clipName = Helper.GetSfxClipNameByIndex(data.clipIndex);
            }
        }

        [Serializable]
        public class Rotate
        {
            [HideInInspector] public RotateField field;
            [HideInInspector] public RotateComponentData data = new();
            [HideInInspector] public GameObject target;
            [HideInInspector] public string id;
            [HideInInspector] public string componentType = null;
            
            public void Setup(string _id, GameObject _target, string _componentType)
            {
                target = _target;
                id = _id;
                componentType = _componentType;
            }
        }

        [Serializable]
        public class Translate
        {
            [HideInInspector] public TranslateField field;
            [HideInInspector] public TranslateComponentData data = new();
            [HideInInspector] public GameObject target;
            [HideInInspector] public string id;
            [HideInInspector] public string componentType = null;
            
            public void Setup(string _id, GameObject _target, string _componentType)
            {
                target = _target;
                id = _id;
                componentType = _componentType;
            }
        }

        [Serializable]
        public class ScoreData
        {
            [HideInInspector] public ScoreField field;
            [HideInInspector] public int score;
            [HideInInspector] public string instanceId;
        }

        [Serializable]
        public class SwitchData
        {
            [HideInInspector] public SwitchComponent data;
        }
    }

    // define component data classes here
    [Serializable]
    public struct StartOnData
    {
        public string startName;
        public int startIndex;
        public string listenName;
        // public int listenIndex;
    }

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
        public bool Xaxis;
        public bool Yaxis;
        public bool Zaxis;
        public Direction direction;
        public float degrees;
        public float speed;
        public int repeat;
        public float pauseBetween;
        
        public string broadcast;
        public Listen listen;
        public BroadcastAt broadcastAt;
    }

    [Serializable]
    public struct TranslateComponentData
    {
        public int translateType;
        public Vector3 moveBy;
        public float pauseFor;
        public float speed;
        public int repeat;
        
        public string broadcast;

        public string listenTo;
        public Listen listen;
        public BroadcastAt broadcastAt;
    }
}
