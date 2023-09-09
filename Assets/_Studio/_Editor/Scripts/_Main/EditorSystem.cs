using UnityEngine;

namespace Terra.Studio
{
    public class EditorSystem : MonoBehaviour, ISubsystem
    {
        private void Awake()
        {
            SystemOp.Register(this as ISubsystem);
            EditorOp.Register(this);
        }

        public void Initialize()
        {
            EditorOp.Register(new DataProvider());
            EditorOp.Register(new SceneDataHandler());
            EditorOp.Resolve<HierarchyView>().Init();
            EditorOp.Resolve<InspectorView>().Init();
            EditorOp.Resolve<ToolbarView>().Init();
            EditorOp.Resolve<SceneView>().Init();
            EditorOp.Resolve<SelectionHandler>().Init();
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
    }
}
