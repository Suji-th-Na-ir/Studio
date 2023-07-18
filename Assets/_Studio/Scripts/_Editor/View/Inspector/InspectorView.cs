using UnityEngine;
using UnityEngine.UI;
using PlayShifu.Terra;
using System.Collections.Generic;

namespace Terra.Studio
{
    public class InspectorView : View
    {
        private const string INSPECTOR_CONTENT_LOC = "InspectorComponent";
        private const string INSPECTOR_PARENT_PANEL_LOC = "Content";
        private const string RESOURCE_TRSGROUP_COMP = "TRSGroup";
        private const string RESOURCE_ADD_FUNCTION_BUTTON = "AddFunction";
        private GameObject componentTRSGroup;
        private GameObject addFunctionGroup;
        private Button addFunctionButton;
        private Transform inspectorContentHolder;
        private StudioGameObject currentSelectedGameObject;
        private Dictionary<string, GameObject> inspectorComponents;

        private void Awake()
        {
            Interop<EditorInterop>.Current.Register(this);
        }

        public override void Init()
        {
            inspectorComponents = new();
            inspectorContentHolder = Helper.FindDeepChild(transform, INSPECTOR_CONTENT_LOC, true);
            var resObj = Resources.Load<GameObject>(RESOURCE_TRSGROUP_COMP);
            componentTRSGroup = Instantiate(resObj, inspectorContentHolder) as GameObject;
            componentTRSGroup.SetActive(false);
            Interop<EditorInterop>.Current.Resolve<SelectionManager>().onSelectionOccured += OnSelectedGameObject;
            var addFuncResObj = Resources.Load<GameObject>(RESOURCE_ADD_FUNCTION_BUTTON);
            addFunctionGroup = Instantiate(addFuncResObj, Helper.FindDeepChild(transform, INSPECTOR_PARENT_PANEL_LOC));
            addFunctionButton = addFunctionGroup.GetComponentInChildren<Button>();
            addFunctionGroup.SetActive(false);
        }

        private void OnSelectedGameObject(StudioGameObject go)
        {
            if (go == null)
            {
                Flush();
            }
            else
            {
                Flush();
                TRSComponent.Init(go.Ref, inspectorContentHolder);
                componentTRSGroup.SetActive(true);
                addFunctionGroup.SetActive(true);
                addFunctionButton.onClick.RemoveAllListeners();
                addFunctionButton.onClick.AddListener(() =>
                {
                    var comp = OscillateDrawComponent.Draw(inspectorContentHolder, go.Id);
                    inspectorComponents.Add(comp.Item1, comp.Item2);
                });
            }
            currentSelectedGameObject = go;
        }

        public override void Draw()
        {

        }

        public override void Flush()
        {
            foreach (var component in inspectorComponents)
            {
                Destroy(component.Value);
            }
            inspectorComponents.Clear();
            componentTRSGroup.SetActive(false);
            addFunctionGroup.SetActive(false);
            if (currentSelectedGameObject != null)
            {
                TRSComponent.Flush(currentSelectedGameObject.Ref);
            }
        }

        public override void Repaint()
        {
            Draw();
        }

        private void OnDestroy()
        {
            Interop<EditorInterop>.Current.Unregister(this);
        }
    }
}
