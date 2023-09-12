using System;
using UnityEngine;
using PlayShifu.Terra;
using RuntimeInspectorNamespace;
using System.Collections.Generic;

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

            public void Setup(GameObject _target, List<string> _list, string _componentType, bool updateListen)
            {
                StartList = _list;
                target = _target;
                if (!AllInstances.Contains(this))
                    AllInstances.Add(this);
                componentType = _componentType;
                if (updateListen)
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
        public class BasePlay
        {
            [HideInInspector] public GameObject target;
            [HideInInspector] public PlayFXData data;
            public Type componentType = null;
            public string fieldName;

            public virtual void Setup<T>(GameObject _target, string fieldName = null)
            {
                target = _target;
                componentType = typeof(T);
                this.fieldName = fieldName;
                data = new();
            }
        }

        [Serializable]
        public class PlaySfx : BasePlay
        {
            public static List<PlaySfx> AllInstances = new();
            [HideInInspector] public PlaySFXField field;

            public override void Setup<T>(GameObject _target, string fieldName = null)
            {
                base.Setup<T>(_target, fieldName);
                if (!AllInstances.Contains(this))
                    AllInstances.Add(this);
                data.ClipName = Helper.GetSfxClipNameByIndex(data.ClipIndex);
            }
        }

        [Serializable]
        public class PlayVfx : BasePlay
        {
            public static List<PlayVfx> AllInstances = new();
            [HideInInspector] public PlayVFXField field;

            public override void Setup<T>(GameObject _target, string fieldName = null)
            {
                base.Setup<T>(_target, fieldName);
                if (!AllInstances.Contains(this))
                    AllInstances.Add(this);
                data.ClipName = Helper.GetVfxClipNameByIndex(data.ClipIndex);
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

    [Serializable]
    public struct StartOnData
    {
        public string startName;
        public int startIndex;
        public string listenName;
    }

    [Serializable]
    public struct PlayFXData
    {
        public bool CanPlay;
        public string ClipName;
        public int ClipIndex;
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
        public Vector3 moveTo;
        public float pauseFor;
        public float speed;
        public int repeat;

        public string broadcast;

        public string listenTo;
        public Listen listen;
        public BroadcastAt broadcastAt;
    }
}
