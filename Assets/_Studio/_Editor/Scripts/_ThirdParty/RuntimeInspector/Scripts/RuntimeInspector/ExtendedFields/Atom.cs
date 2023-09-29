using System;
using UnityEngine;
using PlayShifu.Terra;
using RuntimeInspectorNamespace;
using System.Collections.Generic;

namespace Terra.Studio
{
    public class Atom : IDisposable
    {
        public List<StartOn> AllStartOns = new();
        public List<PlaySfx> AllSfxes = new();
        public List<PlayVfx> AllVfxes = new();

        [Serializable]
        public class StartOn
        {
            public GameObject target;
            public StartOnField field;
            public StartOnData data = new();
            public string componentType = null;
            public List<string> startList = new();
            public List<string> aliasNameList = new();
            public Action<string, string> OnListenerUpdated;

            public void Setup<T>(GameObject target, string componentType) where T : Enum
            {
                Setup<T>(target, componentType, null, false);
            }

            public void Setup<T>(GameObject target, string componentType, Action<string, string> OnListenerUpdated, bool updateListenerInstantly) where T : Enum
            {
                UpdateInstance();
                this.target = target;
                this.componentType = componentType;
                this.OnListenerUpdated = OnListenerUpdated;
                startList = Helper.GetEnumValuesAsStrings<T>();
                aliasNameList = Helper.GetEnumWithAliasNames<T>();
                RecalibrateStartName();
                if (updateListenerInstantly)
                {
                    OnListenerUpdated?.Invoke(data.listenName, string.Empty);
                }
            }

            private void RecalibrateStartName()
            {
                if (startList.Count >= data.startIndex)
                {
                    data.startName = startList[data.startIndex];
                }
            }

            private void UpdateInstance()
            {
                var allStartOns = EditorOp.Resolve<Atom>().AllStartOns;
                if (!allStartOns.Contains(this))
                {
                    allStartOns.Add(this);
                }
            }
        }

        public class BasePlay
        {
            public GameObject target;
            public PlayFXData data = new();
            public Type componentType = null;
            public string fieldName;

            public virtual void Setup<T>(GameObject _target, string fieldName = null)
            {
                target = _target;
                componentType = typeof(T);
                this.fieldName = fieldName;
            }
        }

        [Serializable]
        public class PlaySfx : BasePlay
        {
            public PlaySFXField field;

            public override void Setup<T>(GameObject _target, string fieldName = null)
            {
                base.Setup<T>(_target, fieldName);
                var allSfxes = EditorOp.Resolve<Atom>().AllSfxes;
                if (!allSfxes.Contains(this))
                {
                    allSfxes.Add(this);
                }
                data.clipName = Helper.GetSfxClipNameByIndex(data.clipIndex);
                EditorOp.Resolve<IURCommand>().UpdateReference(this);
            }
        }

        [Serializable]
        public class PlayVfx : BasePlay
        {
            public PlayVFXField field;

            public override void Setup<T>(GameObject _target, string fieldName = null)
            {
                base.Setup<T>(_target, fieldName);
                var allVfxes = EditorOp.Resolve<Atom>().AllVfxes;
                if (!allVfxes.Contains(this))
                {
                    allVfxes.Add(this);
                }
                data.clipName = Helper.GetVfxClipNameByIndex(data.clipIndex);
                EditorOp.Resolve<IURCommand>().UpdateReference(this);
            }
        }

        public class BaseBroadcasterTemplate
        {
            public string id;
            public GameObject target;
            public Action<string, string> OnBroadcastUpdated;

            public virtual void Setup(GameObject target, Action<string, string> OnBroadcastUpdated)
            {
                this.target = target;
                id = Guid.NewGuid().ToString("N");
                this.OnBroadcastUpdated = OnBroadcastUpdated;
            }
        }

        [Serializable]
        public class Rotate : BaseBroadcasterTemplate
        {
            public RotateField field;
            public RotateComponentData data = new();

            public override void Setup(GameObject target, Action<string, string> OnBroadcastUpdated)
            {
                base.Setup(target, OnBroadcastUpdated);
                data.onBroadcastValueModified = OnBroadcastUpdated;
            }
        }

        [Serializable]
        public class Translate : BaseBroadcasterTemplate
        {
            public TranslateField field;
            public TranslateComponentData data = new();

            public override void Setup(GameObject target, Action<string, string> OnBroadcastUpdated)
            {
                base.Setup(target, OnBroadcastUpdated);
                data.onBroadcastValueModified = OnBroadcastUpdated;
            }
        }

        [Serializable]
        public class ScoreData
        {
            public int score;
            public ScoreField field;
            public string instanceId;

            public ScoreData()
            {
                instanceId = Guid.NewGuid().ToString("N");
            }
        }

        public void Dispose()
        {
            AllStartOns.Clear();
            AllSfxes.Clear();
            AllVfxes.Clear();
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
        public Listen listen;
        public BroadcastAt broadcastAt;
        public Action<string, string> onBroadcastValueModified;
        private string broadcast;
        public string Broadcast
        {
            readonly get
            {
                return broadcast;
            }
            set
            {
                onBroadcastValueModified?.Invoke(value, broadcast);
                broadcast = value;
            }
        }
    }

    [Serializable]
    public struct TranslateComponentData
    {
        public int translateType;
        public Vector3 moveBy;
        public float pauseFor;
        public float speed;
        public int repeat;
        public string listenTo;
        public Listen listen;
        public BroadcastAt broadcastAt;
        public Action<string, string> onBroadcastValueModified;
        private string broadcast;
        public string Broadcast
        {
            readonly get
            {
                return broadcast;
            }
            set
            {
                if (!value.Equals(broadcast))
                {
                    onBroadcastValueModified?.Invoke(value, broadcast);
                    broadcast = value;
                }
            }
        }
    }
}
