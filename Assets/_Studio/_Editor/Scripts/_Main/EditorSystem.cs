using System;
using UnityEngine;

namespace Terra.Studio
{
    public class EditorSystem : MonoBehaviour, ISubsystem
    {
        public event Action<bool> OnIncognitoEnabled;
        public bool IsIncognitoEnabled { get; private set; }

        private void Awake()
        {
            SystemOp.Register(this as ISubsystem);
            EditorOp.Register(this);
        }

        public void Initialize()
        {
            EditorOp.Resolve<SelectionHandler>().Init();
            EditorOp.Register(new DataProvider());
            EditorOp.Register(new Atom());
            EditorOp.Register(new SceneDataHandler());
            EditorOp.Register(new UndoRedoSystem() as IURCommand);
            EditorOp.Register(new Recorder());
            EditorOp.Resolve<HierarchyView>().Init();
            EditorOp.Resolve<InspectorView>().Init();
            EditorOp.Resolve<ToolbarView>().Init();
            EditorOp.Resolve<SceneView>().Init();
            EditorOp.Resolve<UILogicDisplayProcessor>().Init();
            EditorOp.Resolve<SceneDataHandler>().LoadScene();
        }

        public void Dispose()
        {
            EditorOp.Resolve<HierarchyView>().Flush();
            EditorOp.Resolve<InspectorView>().Flush();
            EditorOp.Resolve<ToolbarView>().Flush();
            EditorOp.Resolve<SceneView>().Flush();
            EditorOp.Resolve<SelectionHandler>().Flush();
            EditorOp.Resolve<SceneDataHandler>().SaveQoFDetails();
            EditorOp.Unregister<SceneDataHandler>();
            EditorOp.Unregister<IURCommand>();
            EditorOp.Unregister<Atom>();
            EditorOp.Unregister<Recorder>();
        }

        private void OnDestroy()
        {
            EditorOp.Unregister<DataProvider>();
            SystemOp.Unregister(this as ISubsystem);
            EditorOp.Unregister(this);
        }

        public void RequestSwitchState()
        {
            SystemOp.Resolve<System>().SwitchState();
        }

        public void RequestIncognitoMode(bool enable)
        {
            IsIncognitoEnabled = enable;
            OnIncognitoEnabled?.Invoke(enable);
        }
    }
}