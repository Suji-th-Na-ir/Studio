using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Terra.Studio
{
    public class EditorSystem : MonoBehaviour, ISubsystem
    {
        public event Action<bool> OnIncognitoEnabled;
        public bool IsIncognitoEnabled { get; private set; }
        public ComponentIconsPreset ComponentIconsPreset { get { return componentIconsPreset; } }

        private Scene scene;
        private ComponentIconsPreset componentIconsPreset;
        public bool IsPreviewEnabled { get; private set; }
        public Action<bool> OnPreviewEnabled;

        private void Awake()
        {
            SystemOp.Register(this as ISubsystem);
            EditorOp.Register(this);
        }

        public void Initialize(Scene scene)
        {
            this.scene = scene;
            GetComponentData();
            EditorOp.Register(new DataProvider());
            EditorOp.Register(new Atom());
            EditorOp.Register(new SceneDataHandler());
            EditorOp.Register(new UndoRedoSystem() as IURCommand);
            EditorOp.Register(new FocusFieldsSystem());
            EditorOp.Register(new Recorder());
            EditorOp.Register(new CopyPasteSystem());
            EditorOp.Register(new BehaviourPreview());
            EditorOp.Resolve<SelectionHandler>().Init();
            EditorOp.Resolve<HierarchyView>().Init();
            EditorOp.Resolve<InspectorView>().Init();
            EditorOp.Resolve<ToolbarView>().Init();
            EditorOp.Resolve<NavigationToolbar>().Init();
            EditorOp.Resolve<SceneView>().Init();
            EditorOp.Resolve<AssetsView>().Init();
            EditorOp.Resolve<LeftToolbar>().Init();

            EditorOp.Resolve<UILogicDisplayProcessor>().Init();
            EditorOp.Resolve<SceneDataHandler>().LoadScene();
        }

        public void Dispose()
        {
            EditorOp.Unregister<FocusFieldsSystem>();
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
            EditorOp.Unregister<BehaviourPreview>();
        }

        public Scene GetScene()
        {
            return scene;
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

        public void RequestPreviewMode(bool enable)
        {
            IsPreviewEnabled = enable;
            OnPreviewEnabled?.Invoke(enable);
        }

        private void GetComponentData()
        {
            componentIconsPreset = EditorOp.Load<ComponentIconsPreset>("SOs/Component_Icon_SO");
        }
    }
}
