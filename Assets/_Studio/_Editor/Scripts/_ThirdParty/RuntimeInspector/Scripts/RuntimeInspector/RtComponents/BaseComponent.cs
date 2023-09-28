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

        protected abstract string ComponentName { get; }
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
                OnBroadcastUpdated = OnBroadcastStringUpdated;
            }
            if (CanListen)
            {
                OnListenerUpdated = OnListenerStringUpdated;
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

        private void OnBroadcastStringUpdated(string newString, string oldString)
        {
            EditorOp.Resolve<UILogicDisplayProcessor>().UpdateBroadcastString(newString, oldString, DisplayDock);
        }

        private void OnListenerStringUpdated(string newString, string oldString)
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
    }
}