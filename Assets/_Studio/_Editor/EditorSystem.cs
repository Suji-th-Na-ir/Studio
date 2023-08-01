using System;
using UnityEngine;
using PlayShifu.Terra;

namespace Terra.Studio
{
    public class EditorSystem : MonoBehaviour, ISubsystem
    {
        private SaveSystem _saveSystem;
        private void Awake()
        {
            SystemOp.Register(this as ISubsystem);
            EditorOp.Register(this);
            _saveSystem = gameObject.GetComponent<SaveSystem>();
            if (_saveSystem == null) Debug.LogError("save system not attached.");
        }

        public void Initialize()
        {
            EditorOp.Register(new DataProvider());
            EditorOp.Resolve<HierarchyView>().Init();
            EditorOp.Resolve<InspectorView>().Init();
            EditorOp.Resolve<ToolbarView>().Init();
            EditorOp.Resolve<SceneView>().Init();
            EditorOp.Resolve<SelectionHandler>().Init();
            EditorOp.Resolve<SceneExporter>().Init();
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
            SystemOp.Unregister(this as ISubsystem);
            EditorOp.Unregister(this);
        }

        public void RequestSwitchState()
        {
            //Check for busy state of the system, if there is any switch state already in progress
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
