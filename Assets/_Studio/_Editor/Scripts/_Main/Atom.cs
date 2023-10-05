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

        public class BaseBroadcasterTemplate
        {
            public string id;
            public GameObject target;
            public BaseBehaviour behaviour;

            public virtual void Setup(GameObject target, BaseBehaviour behaviour)
            {
                this.target = target;
                this.behaviour = behaviour;
                id = Guid.NewGuid().ToString("N");
            }
        }

        public void Dispose()
        {
            AllStartOns.Clear();
            AllSfxes.Clear();
            AllVfxes.Clear();
        }

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

        [Serializable]
        public class Rotate : BaseBroadcasterTemplate
        {
            public RotateField field;
            public RotateComponentData data = new();

            public override void Setup(GameObject target, BaseBehaviour behaviour)
            {
                base.Setup(target, behaviour);
                data.OnBroadcastUpdated = behaviour.OnBroadcastStringUpdated;
            }
        }

        [Serializable]
        public class Translate : BaseBroadcasterTemplate
        {
            public TranslateField field;
            public TranslateComponentData data = new();
            public Action ForceRefreshData;

            public override void Setup(GameObject target, BaseBehaviour behaviour)
            {
                base.Setup(target, behaviour);
                data.OnBroadcastUpdated = behaviour.OnBroadcastStringUpdated;
                data.recordedVector3 = new();
                data.recordedVector3.Setup(behaviour);
                data.recordedVector3.Set(new Vector3(0f, 1f, 0f));
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

        [Serializable]
        public class RecordedVector3 : IObscurer
        {
            public RecordedVector3Field field;

            private Vector3 vector3;
            private Vector3 defaultVector3;
            private Type behaviourType;
            private BaseBehaviour behaviour;

            public Vector3 LastVector3;
            public Action<bool> OnModified;
            public Type ObscureType => typeof(Vector3);
            public Type DeclaredType => behaviourType;
            public Func<bool> IsValueModified => IsModified;
            public Action ToggleGhostMode => behaviour.GhostDescription.ToggleGhostMode;

            public readonly Vector3 INFINITY = new(-float.MaxValue, -float.MaxValue, -float.MaxValue);

            public void Setup<T>(T instance) where T : BaseBehaviour
            {
                behaviourType = typeof(T);
                behaviour = instance;
                vector3 = new(-float.MaxValue, -float.MaxValue, -float.MaxValue);
                defaultVector3 = vector3;
            }

            public object Get()
            {
                return vector3;
            }

            public void Set(object obj)
            {
                try
                {
                    var vector3 = (Vector3)obj;
                    SetDefault(vector3);
                    this.vector3 = vector3;
                    HandleResetOption();
                }
                catch
                {
                    Debug.LogError("Type of object passed is incorrected. Expected: Vector3");
                }
            }

            public void Set(Axis axis, object obj)
            {
                var currentVector = (Vector3)Get();
                var newValue = (float)obj;
                switch (axis)
                {
                    case Axis.X:
                        currentVector.x = newValue;
                        break;
                    case Axis.Y:
                        currentVector.y = newValue;
                        break;
                    case Axis.Z:
                        currentVector.z = newValue;
                        break;
                }
                Set(currentVector);
            }

            public void Reset()
            {
                vector3 = defaultVector3;
            }

            private void SetDefault(Vector3 vector3)
            {
                if (vector3 != INFINITY && defaultVector3 == INFINITY)
                {
                    defaultVector3 = vector3;
                }
            }

            private void HandleResetOption()
            {
                if (!IsIncognito())
                {
                    var isModified = IsModified();
                    OnModified?.Invoke(isModified);
                }
            }

            private bool IsModified()
            {
                if (IsIncognito())
                {
                    return false;
                }
                if (vector3 == INFINITY || vector3 == defaultVector3)
                {
                    return false;
                }
                return true;
            }

            private bool IsIncognito()
            {
                var isIncognito = EditorOp.Resolve<EditorSystem>().IsIncognitoEnabled;
                return isIncognito;
            }
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
        public Action<string, string> OnBroadcastUpdated;
        public string broadcast;
        public string Broadcast
        {
            readonly get
            {
                return broadcast;
            }
            set
            {
                if (value != broadcast)
                {
                    OnBroadcastUpdated?.Invoke(value, broadcast);
                    broadcast = value;
                }
            }
        }

        public readonly bool IsEmpty()
        {
            if (Equals(default(RotateComponentData)))
            {
                return true;
            }
            return false;
        }
    }

    [Serializable]
    public struct TranslateComponentData
    {
        [HideInInspector]
        public Atom.RecordedVector3 recordedVector3;
        public int translateType;
        public float pauseFor;
        public float speed;
        public int repeat;
        public string listenTo;
        public Listen listen;
        public BroadcastAt broadcastAt;
        public Action<string, string> OnBroadcastUpdated;
        public Vector3 LastVector3;
        public string broadcast;
        public string Broadcast
        {
            readonly get
            {
                return broadcast;
            }
            set
            {
                if (value != broadcast)
                {
                    OnBroadcastUpdated?.Invoke(value, broadcast);
                    broadcast = value;
                }
            }
        }

        public readonly bool IsEmpty()
        {
            var cloneToTest = this;
            cloneToTest.recordedVector3 = null;
            cloneToTest.OnBroadcastUpdated = null;
            if (cloneToTest.Equals(default(TranslateComponentData)))
            {
                return true;
            }
            return false;
        }
    }
}