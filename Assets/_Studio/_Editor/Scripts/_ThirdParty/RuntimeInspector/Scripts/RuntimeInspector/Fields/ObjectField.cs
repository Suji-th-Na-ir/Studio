using System;
using System.Collections.Generic;
using System.Reflection;
using PlayShifu.Terra;
using Terra.Studio;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RuntimeInspectorNamespace
{
    public class ObjectField : ExpandableInspectorField
    {
#pragma warning disable 0649
        [SerializeField]
        private Button initializeObjectButton;
#pragma warning restore 0649

        private bool elementsInitialized = false;
        private IRuntimeInspectorCustomEditor customEditor;
        private bool didCheckForExpand;
        Button copyButton, pasteButton;
        GameObject copyPastePanel;
        private Button openCopyPaste;
        Text pasteText;
        bool isPointerOverGameObject = false;
        protected override int Length
        {
            get
            {
                if (Value.IsNull())
                {
                    if (!initializeObjectButton.gameObject.activeSelf)
                        return -1;

                    return 0;
                }

                if (initializeObjectButton.gameObject.activeSelf)
                    return -1;

                if (!elementsInitialized)
                {
                    elementsInitialized = true;
                    return -1;
                }

                return elements.Count;
            }
        }

        public override void Initialize()
        {
            base.Initialize();
            initializeObjectButton.onClick.AddListener(InitializeObject);
            copyPastePanel = Helper.FindDeepChild(transform, "CopyPastePanel").gameObject;
            copyButton = Helper.FindDeepChild(transform, "CopyButton").GetComponent<Button>();
            pasteButton = Helper.FindDeepChild(transform, "PasteButton").GetComponent<Button>();


            copyPastePanel.SetActive(false);
            copyButton.onClick.RemoveAllListeners();
            copyButton.onClick.AddListener(() => CopyBehaviour());
            pasteButton.onClick.RemoveAllListeners();
            pasteButton.onClick.AddListener(() => PasteBehaviour());
            pasteText = pasteButton.GetComponentInChildren<Text>();

            openCopyPaste = Helper.FindDeepChild(transform, "OpenCopyPasteBtn").GetComponent<Button>();
            openCopyPaste.onClick.RemoveAllListeners();
            openCopyPaste.onClick.AddListener(() => OpenCopyPastePanel());
            copyPastePanel.GetComponent<PointerEventListener>().PointerEnter += (PointerEventData data) => { isPointerOverGameObject = true; };
            copyPastePanel.GetComponent<PointerEventListener>().PointerExit += (PointerEventData data) => { isPointerOverGameObject = false; };

        }

        private void Update()
        {
            if(Input.GetMouseButtonDown(0) &&!isPointerOverGameObject)
            {
                copyPastePanel.SetActive(false);
            }
        }

        private void OpenCopyPastePanel()
        {
            var open = !copyPastePanel.activeSelf;
            copyPastePanel.SetActive(open);
        }

        private void CopyBehaviour()
        {
            if (Value as BaseBehaviour != null)
            {
                EditorOp.Resolve<CopyPasteSystem>().CopyBehaviorData(Value as BaseBehaviour);
                copyPastePanel.SetActive(false);
            }
        }

        private void PasteBehaviour()
        {
            var selected = EditorOp.Resolve<SelectionHandler>().GetSelectedObjects();
            List<BaseBehaviour> sameComponents = new List<BaseBehaviour>();
            for (int i = 0; i < selected.Count; i++)
            {
                var components = selected[i].GetComponents<BaseBehaviour>();
                for (int j = 0; j < components.Length; j++)
                {
                    if (components[j] != null)
                    {
                        var exportedData = components[j].Export();
                        if (EditorOp.Resolve<CopyPasteSystem>().IsLastBehaviourDataSame(exportedData.type))
                        {
                            sameComponents.Add(components[j]);
                        }
                    }
                }
            }
            EditorOp.Resolve<CopyPasteSystem>().PasteBehaviourData(sameComponents);
            copyPastePanel.SetActive(false);
        }

        public override bool SupportsType(Type type)
        {
            return true;
        }

        protected override void OnBound(MemberInfo variable)
        {
            elementsInitialized = false;
            base.OnBound(variable);
        }

        protected override void GenerateElements()
        {
            if (Value.IsNull())
            {
                initializeObjectButton.gameObject.SetActive(CanInitializeNewObject());
                return;
            }

            initializeObjectButton.gameObject.SetActive(false);

            if ((customEditor = RuntimeInspectorUtils.GetCustomEditor(Value.GetType())) != null)
                customEditor.GenerateElements(this);
            else
                CreateDrawersForVariables();
        }

        protected override void ClearElements()
        {
            base.ClearElements();

            if (customEditor != null)
            {
                customEditor.Cleanup();
                customEditor = null;
            }

            didCheckForExpand = false;
        }

        protected override void OnSkinChanged()
        {
            base.OnSkinChanged();
            initializeObjectButton.SetSkinButton(Skin);
        }

        public override void Refresh()
        {
            base.Refresh();
            customEditor?.Refresh();
            if (!didCheckForExpand && Value != null)
            {
                didCheckForExpand = true;
                IsExpanded = true;
            }

            var behaviour = Value as BaseBehaviour;
            if (behaviour != null)
            {
                var data = behaviour.Export();
                bool activate = EditorOp.Resolve<CopyPasteSystem>().IsLastBehaviourDataSame(data.type);
                pasteButton.interactable = activate;
                if (!activate)
                {
                    var color = Helper.GetColorFromHex("#C8C8C8");
                    color.a = 0.6f;
                    pasteText.color = color;
                }
                else
                {
                    pasteText.color = Helper.GetColorFromHex("#FFFFFF");
                }
            }
        }

        public void CreateDrawersForVariables(params string[] variables)
        {
            if (variables == null || variables.Length == 0)
            {
                foreach (MemberInfo variable in Inspector.GetExposedVariablesForType(Value.GetType()))
                    CreateDrawerForVariable(variable);
            }
            else
            {
                foreach (MemberInfo variable in Inspector.GetExposedVariablesForType(Value.GetType()))
                {
                    if (Array.IndexOf(variables, variable.Name) >= 0)
                        CreateDrawerForVariable(variable);
                }
            }
        }

        public void CreateDrawersForVariablesExcluding(params string[] variablesToExclude)
        {
            if (variablesToExclude == null || variablesToExclude.Length == 0)
            {
                foreach (MemberInfo variable in Inspector.GetExposedVariablesForType(Value.GetType()))
                    CreateDrawerForVariable(variable);
            }
            else
            {
                foreach (MemberInfo variable in Inspector.GetExposedVariablesForType(Value.GetType()))
                {
                    if (Array.IndexOf(variablesToExclude, variable.Name) < 0)
                        CreateDrawerForVariable(variable);
                }
            }
        }

        private bool CanInitializeNewObject()
        {
#if UNITY_EDITOR || !NETFX_CORE
            if (BoundVariableType.IsAbstract || BoundVariableType.IsInterface)
#else
			if( BoundVariableType.GetTypeInfo().IsAbstract || BoundVariableType.GetTypeInfo().IsInterface )
#endif
                return false;

            if (typeof(ScriptableObject).IsAssignableFrom(BoundVariableType))
                return true;

            if (typeof(UnityEngine.Object).IsAssignableFrom(BoundVariableType))
                return false;

            if (BoundVariableType.IsArray)
                return false;

#if UNITY_EDITOR || !NETFX_CORE
            if (BoundVariableType.IsGenericType && BoundVariableType.GetGenericTypeDefinition() == typeof(List<>))
#else
			if( BoundVariableType.GetTypeInfo().IsGenericType && BoundVariableType.GetGenericTypeDefinition() == typeof( List<> ) )
#endif
                return false;

            return true;
        }

        private void InitializeObject()
        {
            if (CanInitializeNewObject())
            {
                Value = BoundVariableType.Instantiate();
                RegenerateElements();
                IsExpanded = true;
            }
        }
    }
}