using UnityEngine;
using PlayShifu.Terra;

namespace Terra.Studio
{
    public class EditorSystem : MonoBehaviour, ISubsystem
    {
        private SaveSystem _saveSystem;
        public Vector3 PlayerSpawnPoint;
        private void Awake()
        {
            SystemOp.Register(this as ISubsystem);
            EditorOp.Register(this);
            if (!gameObject.TryGetComponent(out _saveSystem)) Debug.LogError("save system not attached.");
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
           
        }

        public void Dispose()
        {
            EditorOp.Resolve<HierarchyView>().Flush();
            EditorOp.Resolve<InspectorView>().Flush();
            EditorOp.Resolve<ToolbarView>().Flush();
            EditorOp.Resolve<SceneView>().Flush();
            EditorOp.Resolve<SelectionHandler>().Flush();
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

        public void RequestSaveScene()
        {
            _saveSystem.Save(Helper.GetCoreDataSavePath(), "core_data", ".data");
        }

        public void RequestLoadScene()
        {
            _saveSystem.Load(Helper.GetCoreDataSavePath(), "core_data", ".data");
        }

       
    }
}
