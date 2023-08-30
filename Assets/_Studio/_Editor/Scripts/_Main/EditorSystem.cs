using UnityEngine;

namespace Terra.Studio
{
    public class EditorSystem : MonoBehaviour, ISubsystem
    {
        [HideInInspector] public Vector3 PlayerSpawnPoint;

        private Camera editorCamera;

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
            EditorOp.Resolve<UILogicDisplayProcessor>().Init();
            EditorOp.Resolve<SelectionHandler>().Init();
            EditorOp.Resolve<SceneDataHandler>().LoadScene();
            new EditorEssentialsLoader().LoadEssentials();
            SetupScene();
        }

        public void Dispose()
        {
            EditorOp.Resolve<HierarchyView>().Flush();
            EditorOp.Resolve<InspectorView>().Flush();
            EditorOp.Resolve<ToolbarView>().Flush();
            EditorOp.Resolve<SceneView>().Flush();
            EditorOp.Resolve<SelectionHandler>().Flush();
            SaveQoFDetails();
        }

        private void OnDestroy()
        {
            EditorOp.Unregister<DataProvider>();
            EditorOp.Unregister<SceneDataHandler>();
            SystemOp.Unregister(this as ISubsystem);
            EditorOp.Unregister(this);
        }

        public void RequestSwitchState()
        {
            SystemOp.Resolve<System>().SwitchState();
        }

        private void SetupScene()
        {
            editorCamera = Camera.main;
            var isDataPresent = SystemOp.Resolve<CrossSceneDataHolder>().Get("CameraPos", out var data);
            if (isDataPresent)
            {
                editorCamera.transform.position = (Vector3)data;
            }
            isDataPresent = SystemOp.Resolve<CrossSceneDataHolder>().Get("CameraRot", out data);
            if (isDataPresent)
            {
                editorCamera.transform.rotation = Quaternion.Euler((Vector3)data);
            }
        }

        private void SaveQoFDetails()
        {
            SystemOp.Resolve<CrossSceneDataHolder>().Set("CameraPos", editorCamera.transform.position);
            SystemOp.Resolve<CrossSceneDataHolder>().Set("CameraRot", editorCamera.transform.eulerAngles);
        }
    }
}
