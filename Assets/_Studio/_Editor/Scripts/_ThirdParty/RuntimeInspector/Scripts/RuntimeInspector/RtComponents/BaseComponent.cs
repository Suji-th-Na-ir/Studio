using System;
using UnityEngine;
using RuntimeInspectorNamespace;
using System.Collections.Generic;

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
        public virtual Atom.RecordedVector3 RecordedVector3 { get; }

        protected abstract bool CanBroadcast { get; }
        protected abstract bool CanListen { get; }
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
                OnListenerUpdated = (newString, oldString) =>
                {
                    OnListenerStringUpdated(newString, oldString);
                    UpdateListenerForMultiSelected(newString, oldString);
                };
            }
        }

        protected virtual void OnEnable()
        {
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
        }

        private void RegisterBroadcastToDisplayDock(string newString)
        {
            EditorOp.Resolve<UILogicDisplayProcessor>().UpdateBroadcastString(newString, string.Empty, DisplayDock);
        }

        private void RegisterListenerToDisplayDock(string newString)
        {
            EditorOp.Resolve<UILogicDisplayProcessor>().UpdateListenerString(newString, string.Empty, DisplayDock);
        }

        public void OnBroadcastStringUpdated(string newString, string oldString)
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
            if (CanListen && ListenerRefs != null)
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
            var type = GetType();
            var isFound = SystemOp.Resolve<System>().SystemData.TryGetSystemDisplayName(type.FullName, out var displayName);
            if (isFound)
            {
                return displayName;
            }
            return "NOT_FOUND_TYPE";
        }

        public virtual string GetEventConditionalName()
        {
            return default;
        }

        public virtual Dictionary<string, object> GetPreviewProperties()
        {
            return default;
        }

        protected virtual void Update()
        {
            if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.P) && CanPerformShortcut())
            {
                EditorOp.Resolve<BehaviourPreview>().Preview(this);
            }
        }

        private bool CanPerformShortcut()
        {
            var selections = EditorOp.Resolve<SelectionHandler>().GetSelectedObjects();
            if (selections.Count != 1)
            {
                return false;
            }
            if (selections[0].gameObject != gameObject)
            {
                return false;
            }
            var components = gameObject.GetComponents<BaseBehaviour>();
            if (components.Length != 1)
            {
                return false;
            }
            return true;
        }
    }
}