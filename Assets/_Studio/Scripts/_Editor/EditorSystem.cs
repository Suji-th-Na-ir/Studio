using System;
using UnityEngine;

namespace Terra.Studio
{
    public class EditorSystem : MonoBehaviour, ISubsystem
    {
        private void Awake()
        {
            Interop<SystemInterop>.Current.Register(this as ISubsystem);
            Interop<EditorInterop>.Current.Register(this);
        }

        public void Initialize()
        {
            Interop<EditorInterop>.Current.Resolve<HierarchyView>().Init();
            Interop<EditorInterop>.Current.Resolve<InspectorView>().Init();
            Interop<EditorInterop>.Current.Resolve<ToolbarView>().Init();
            Interop<EditorInterop>.Current.Resolve<SceneView>().Init();
            Interop<EditorInterop>.Current.Resolve<SelectionHandler>().Init();
        }

        public void Dispose()
        {
            Interop<EditorInterop>.Current.Resolve<HierarchyView>().Flush();
            Interop<EditorInterop>.Current.Resolve<InspectorView>().Flush();
            Interop<EditorInterop>.Current.Resolve<ToolbarView>().Flush();
            Interop<EditorInterop>.Current.Resolve<SceneView>().Flush();
            Interop<EditorInterop>.Current.Resolve<SelectionHandler>().Flush();
        }

        private void OnDestroy()
        {
            Interop<SystemInterop>.Current.Unregister(this as ISubsystem);
            Interop<EditorInterop>.Current.Unregister(this);
        }

        public void RequestSwitchState()
        {
            //Check for busy state of the system, if there is any switch state already in progress
            Interop<SystemInterop>.Current.Resolve<System>().SwitchState();
        }
    }
}
