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

        private void Start()
        {
            //Some kind of data binder is needed here as well
            //Because we might load json to reload a saved scene
            Initialize();
        }

        private void Initialize()
        {
            //Prep all the data here
            Interop<EditorInterop>.Current.Resolve<HierarchyView>().Init();
            Interop<EditorInterop>.Current.Resolve<InspectorView>().Init();
            Interop<EditorInterop>.Current.Resolve<ToolbarView>().Init();
            Interop<EditorInterop>.Current.Resolve<SceneView>().Init();

            //Actually populate the UI list here
            Interop<EditorInterop>.Current.Resolve<HierarchyView>().Draw();
            Interop<EditorInterop>.Current.Resolve<InspectorView>().Draw();
            Interop<EditorInterop>.Current.Resolve<ToolbarView>().Draw();
            Interop<EditorInterop>.Current.Resolve<SceneView>().Draw();

            // Interop<EditorInterop>.Current.Register<StudioGameObjectsHolder>(StudioGameObjectsHolder.GetReference());
            
            // init selection handler 
            Interop<EditorInterop>.Current.Resolve<SelectionHandler>().Init();
        }

        public void Dispose()
        {
            Interop<EditorInterop>.Current.Resolve<HierarchyView>().Flush();
            Interop<EditorInterop>.Current.Resolve<InspectorView>().Flush();
            Interop<EditorInterop>.Current.Resolve<ToolbarView>().Flush();
            Interop<EditorInterop>.Current.Resolve<SceneView>().Flush();
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
