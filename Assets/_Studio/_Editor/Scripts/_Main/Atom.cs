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
        public List<Repeat> AllRepeats = new();
        public List<Broadcast> AllBroadcasts = new();
        public List<ScoreData> AllScores = new();

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

            public void Import(bool canPlay, int clipIndex, string clipName)
            {
                data.canPlay = canPlay;
                data.clipIndex = clipIndex;
                data.clipName = clipName;
            }
        }

        public class BaseTargetTemplate
        {
            [HideInInspector]
            public string id;
            [HideInInspector]
            public GameObject target;
            [HideInInspector]
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
            AllRepeats.Clear();
            AllBroadcasts.Clear();
            AllScores.Clear();
        }

        [Serializable]
        public class StartOn
        {
            public GameObject target;
            public StartOnData data = new();
            public string componentType = null;
            public List<string> startList = new();
            public List<string> aliasNameList = new();
            public Action<string, string> OnListenerUpdated;
            public Action<int> OnStartOnUpdated;

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
                else
                {
                    Debug.Log($"Attempting to add multiple start on instance!");
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
        public class Repeat : BaseTargetTemplate
        {
            [AliasDrawer("Repeat")] public int repeat = 1;
            [AliasDrawer("Pause For")] public float pauseFor;
            [AliasDrawer("Repeat\nType")] public RepeatDirectionType repeatType;
            [AliasDrawer("Repeat\nForever")] public bool repeatForever;
            [AliasDrawer("Broadcast At")] public BroadcastAt broadcastAt;
            public Atom.Broadcast broadcastData = new();
            [HideInInspector] public string lastEnteredBroadcast;

            [HideInInspector]
            public string Broadcast
            {
                get
                {
                    return broadcastData.broadcast;
                }
                set
                {
                    var last = broadcastData.broadcast;
                    broadcastData.broadcast = value;
                    behaviour.OnBroadcastStringUpdated(value, last);
                }
            }
            public override void Setup(GameObject target, BaseBehaviour behaviour)
            {
                base.Setup(target, behaviour);
                broadcastData.Setup(target, behaviour);
                var allrepeats = EditorOp.Resolve<Atom>().AllRepeats;
                if (!allrepeats.Contains(this))
                {
                    allrepeats.Add(this);
                }
            }
        }

        [Serializable]
        public class Rotate : BaseTargetTemplate
        {
            [AliasDrawer("Rotate By")] public Atom.RecordedVector3 vector3;
            [HideInInspector] public Vector3 LastVector3;

            public override void Setup(GameObject target, BaseBehaviour behaviour)
            {
                base.Setup(target, behaviour);
                vector3 = new();
                vector3.Setup(behaviour);
                vector3.Set(new Vector3(0f, 15f, 0f));
            }
        }

        [Serializable]
        public class Translate : BaseTargetTemplate
        {
            [AliasDrawer("Move By")] public Atom.RecordedVector3 recordedVector3;
            [HideInInspector] public Vector3 LastVector3;

            public override void Setup(GameObject target, BaseBehaviour behaviour)
            {
                base.Setup(target, behaviour);
                recordedVector3 = new();
                recordedVector3.Setup(behaviour);
                recordedVector3.Set(new Vector3(0f, 1f, 0f));
            }
        }

        [Serializable]
        public class ScoreData : BaseTargetTemplate
        {
            public int score;
            public string instanceId;
            public ScoreData()
            {
                instanceId = Guid.NewGuid().ToString("N");
            }
            public override void Setup(GameObject target, BaseBehaviour behaviour)
            {
                base.Setup(target, behaviour);
                var allscores = EditorOp.Resolve<Atom>().AllScores;
                if (!allscores.Contains(this))
                {
                    allscores.Add(this);
                }
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
            public Action OnValueReset;
            public Action<bool> OnModified;
            public Type ObscureType => typeof(Vector3);
            public Type DeclaredType => behaviourType;
            public Func<bool> IsValueModified => IsModified;
            public Action ToggleRecordMode => behaviour.GhostDescription.ToggleRecordMode;
            public Action UpdateGhostTrs => behaviour.GhostDescription.UpdateSlectionGhostTRS;

            public static readonly Vector3 INFINITY = new(-float.MaxValue, -float.MaxValue, -float.MaxValue);

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

            public void Reset()
            {
                vector3 = defaultVector3;
                OnValueReset?.Invoke();
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

        [Serializable]
        public class Broadcast : BaseTargetTemplate
        {
            [AliasDrawer("Custom"), OnValueChanged(UpdateBroadcast = true)]
            public string broadcast = string.Empty;

            public override void Setup(GameObject target, BaseBehaviour behaviour)
            {
                base.Setup(target, behaviour);
                var allbroadcasts = EditorOp.Resolve<Atom>().AllBroadcasts;
                if (!allbroadcasts.Contains(this))
                {
                    allbroadcasts.Add(this);
                }
            }

            public void Import(string value)
            {
                broadcast = value;
            }
        }

        [Serializable]
        public class InstantiateOnData : BaseTargetTemplate
        {
            [AliasDrawer("Spawn when")] public StartOn spawnWhen = new();
            [AliasDrawer("Spawn where")] public SpawnWhere spawnWhere;
            [AliasDrawer("How many")] public uint howMany;
            [AliasDrawer("Interval")] public int interval;
            [AliasDrawer("Rounds")] public uint rounds;
            [AliasDrawer("Repeat forever")] public bool repeatForever;
            [HideInInspector] public InstantiateOn instantiateOn;
            [HideInInspector] public Vector3[] trs;
            public Action<InstantiateOn> OnSpawnWhenUpdated;
            public Action<bool> OnRecordToggled;

            public override void Setup(GameObject target, BaseBehaviour behaviour)
            {
                base.Setup(target, behaviour);
                spawnWhen.Setup<InstantiateOn>(target, behaviour.ComponentName, behaviour.OnListenerUpdated, spawnWhen.data.startIndex == 2);
                spawnWhen.OnStartOnUpdated += (index) =>
                {
                    instantiateOn = (InstantiateOn)index;
                    OnSpawnWhenUpdated?.Invoke(instantiateOn);
                };
                SetupTRS();
                SetupDefault();
            }

            public void UpdateTRS(Vector3[] trs)
            {
                Array.Copy(trs, this.trs, trs.Length);
                CheckForMultiselectScenario(trs);
            }

            public void Import(InstantiateStudioObjectComponent component)
            {
                spawnWhere = component.spawnWhere;
                howMany = component.duplicatesToSpawn;
                rounds = component.rounds;
                repeatForever = component.canRepeatForver;
                trs = InstantiateStudioObjectComponent.TRS.GetVector3Array(component.trs);
                var isAvailable = EditorOp.Resolve<DataProvider>().TryGetEnum(component.ConditionType, typeof(InstantiateOn), out var result);
                if (isAvailable)
                {
                    instantiateOn = (InstantiateOn)result;
                    spawnWhen.data.startIndex = (int)instantiateOn;
                    spawnWhen.data.startName = instantiateOn.GetStringValue();
                    if (instantiateOn == InstantiateOn.EveryXSeconds)
                    {
                        interval = int.Parse(component.ConditionData);
                    }
                    else if (instantiateOn == InstantiateOn.BroadcastListen)
                    {
                        spawnWhen.data.listenName = component.ConditionData;
                    }
                }
            }

            private void SetupTRS()
            {
                var tr = target.transform;
                var position = tr.position;
                var rotation = tr.eulerAngles;
                var scale = GetBoundsSize();
                trs = new Vector3[]
                {
                    position,
                    rotation,
                    scale
                };
            }

            private void SetupDefault()
            {
                howMany = 1;
                interval = 1;
                rounds = 1;
            }

            private void CheckForMultiselectScenario(Vector3[] trs)
            {
                var selectedObjs = EditorOp.Resolve<SelectionHandler>().GetSelectedObjects();
                foreach (var obj in selectedObjs)
                {
                    if (obj == target) continue;
                    if (obj.TryGetComponent(out InstantiateStudioObject component))
                    {
                        component.instantiateData.UpdateTRS(trs);
                    }
                }
            }

            private Vector3 GetBoundsSize()
            {
                var renderers = target.GetComponentsInChildren<Renderer>();
                if (renderers.Length == 0)
                {
                    return Vector3.one;
                }
                Bounds bounds = renderers[0].bounds;
                foreach (var renderer in renderers)
                {
                    bounds.Encapsulate(renderer.bounds);
                }
                return bounds.size;
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

}