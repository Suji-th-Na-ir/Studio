using System;
using UnityEngine;
using RuntimeInspectorNamespace;

namespace Terra.Studio
{
    public abstract class BaseBehaviour : MonoBehaviour, IComponent
    {
        public abstract (string type, string data) Export();
        public abstract void Import(EntityBasedComponent data);
        public Action<string, string> OnBroadcastUpdated;
        public Action<string, string> OnListenerUpdated;
        public Recorder.GhostDescription GhostDescription;

        public abstract string ComponentName { get; }
        public abstract bool CanPreview { get; }
        public virtual Atom.RecordedVector3 RecordedVector3 { get; }

        protected abstract bool CanBroadcast { get; }
        protected abstract bool CanListen { get; }
        protected virtual bool UpdateListenOnEnable { get { return true; } }
        protected virtual string[] BroadcasterRefs { get; }
        protected virtual string[] ListenerRefs { get; }
        protected virtual ComponentDisplayDock DisplayDock { get; private set; }

        protected virtual void Awake()
        {
            InitializeDisplayDock();
            if (CanBroadcast)
            {
                OnBroadcastUpdated = (newString, oldString) =>
                {
                    OnBroadcastStringUpdated(newString, oldString);
                    UpdateBroadcastForMultiSelected(newString, oldString);
                };
            }
            if (CanListen)
            {
                OnListenerUpdated = (newString, oldString ) =>
                {
                    OnListenerStringUpdated(newString, oldString);
                    UpdateListenerForMultiSelected(newString, oldString);
                };
            }
        }

        protected virtual void OnEnable()
        {
            if (EditorOp.Resolve<EditorSystem>().IsIncognitoEnabled)
                return;
            EditorOp.Resolve<UILogicDisplayProcessor>().AddComponentIcon(DisplayDock);
            CheckAndUpdateVisualisation();
        }

        protected virtual void OnDisable()
        {
            if (EditorOp.Resolve<UILogicDisplayProcessor>())
            {
                EditorOp.Resolve<UILogicDisplayProcessor>().RemoveComponentIcon(DisplayDock);
            }
        }

        protected void ImportVisualisation(string broadcast, string broadcastListen)
        {
            EditorOp.Resolve<UILogicDisplayProcessor>().ImportVisualisation(gameObject, ComponentName, broadcast, broadcastListen);
            SystemOp.Resolve<CrossSceneDataHolder>().UpdateNewBroadcast(broadcast);
            SystemOp.Resolve<CrossSceneDataHolder>().UpdateNewBroadcast(broadcastListen);
        }

        private void RegisterBroadcastToDisplayDock(string newString)
        {
            EditorOp.Resolve<UILogicDisplayProcessor>().UpdateBroadcastString(newString, string.Empty, DisplayDock);
        }

        private void RegisterListenerToDisplayDock(string newString)
        {
            EditorOp.Resolve<UILogicDisplayProcessor>().UpdateListenerString(newString, string.Empty, DisplayDock);
        }

        public virtual void OnBroadcastStringUpdated(string newString, string oldString)
        {
            EditorOp.Resolve<UILogicDisplayProcessor>().UpdateBroadcastString(newString, oldString, DisplayDock);
        }

        public void OnListenerStringUpdated(string newString, string oldString)
        {
            EditorOp.Resolve<UILogicDisplayProcessor>().UpdateListenerString(newString, oldString, DisplayDock);
        }

        private void InitializeDisplayDock()
        {
            DisplayDock = new()
            {
                ComponentGameObject = gameObject,
                ComponentType = ComponentName
            };
        }

        protected void CheckAndUpdateVisualisation()
        {
            if (CanBroadcast && BroadcasterRefs != null)
            {
                foreach (var broadcaster in BroadcasterRefs)
                {
                    if (!string.IsNullOrEmpty(broadcaster))
                    {
                        RegisterBroadcastToDisplayDock(broadcaster);
                    }
                }
            }
            if (CanListen && ListenerRefs != null && UpdateListenOnEnable)
            {
                foreach (var listener in ListenerRefs)
                {
                    if (!string.IsNullOrEmpty(listener))
                    {
                        RegisterListenerToDisplayDock(listener);
                    }
                }
            }
        }

        private void UpdateBroadcastForMultiSelected(string newString, string oldString)
        {
            foreach (var selected in EditorOp.Resolve<SelectionHandler>().GetSelectedObjects())
            {
                if (!selected) continue;
                if (selected == gameObject) continue;
                if (selected.TryGetComponent(out BaseBehaviour behaviour))
                {
                    if (behaviour.ComponentName.Equals(ComponentName))
                    {
                        behaviour.OnBroadcastStringUpdated(newString, oldString);
                    }
                }
            }
        }

        private void UpdateListenerForMultiSelected(string newString, string oldString)
        {
            foreach (var selected in EditorOp.Resolve<SelectionHandler>().GetSelectedObjects())
            {
                if (!selected) continue;
                if (selected == gameObject) continue;
                if (selected.TryGetComponent(out BaseBehaviour behaviour))
                {
                    if (behaviour.ComponentName.Equals(ComponentName))
                    {
                        behaviour.OnListenerStringUpdated(newString, oldString);
                    }
                }
            }
        }

        public void Import(string data)
        {
            var component = new EntityBasedComponent()
            {
                type = EditorOp.Resolve<DataProvider>().GetCovariance(this),
                data = data
            };
            Import(component);
        }

        public virtual string GetDisplayName()
        {
            var type = GetType().FullName;
            var isFound = SystemOp.Resolve<System>().SystemData.TryGetSystemDisplayName(type, out var displayName);
            if (isFound)
            {
                return displayName;
            }
            Debug.Log($"Did not find name for: {type}");
            return "NOT_FOUND_TYPE";
        }

        public void DoPreview()
        {
            EditorOp.Resolve<BehaviourPreview>().Preview(this);
        }

        public virtual BehaviourPreviewUI.PreviewData GetPreviewData()
        {
            return default;
        }
    }
}