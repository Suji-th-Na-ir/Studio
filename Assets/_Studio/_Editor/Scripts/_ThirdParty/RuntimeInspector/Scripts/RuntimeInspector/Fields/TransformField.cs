using System;
using System.Collections.Generic;
using System.Reflection;
using PlayShifu.Terra;
using Terra.Studio;
using UnityEngine;
using UnityEngine.UI;

namespace RuntimeInspectorNamespace
{
    public class TransformField : ExpandableInspectorField
    {
        protected override int Length { get { return 3; } } // localPosition, localEulerAngles, localScale

        private PropertyInfo positionProp, rotationProp, scaleProp;
        private bool didCheckForExpand;
        [SerializeField] private Button openCopyPaste, copy, paste;
        [SerializeField] private GameObject copyPastePanel;
        CopyView copyView;
        PasteView pasteView;
        public override void Initialize()
        {
            base.Initialize();
          
            positionProp = typeof(Transform).GetProperty("localPosition");
            rotationProp = typeof(Transform).GetProperty("localEulerAngles");
            scaleProp = typeof(Transform).GetProperty("localScale");
            copyPastePanel.SetActive(false);
            openCopyPaste.onClick.AddListener(() => OpenCopyPastePanel());

            copyView = Helper.FindDeepChild(transform, "CopyView").GetComponent<CopyView>();
            pasteView = Helper.FindDeepChild(transform, "PasteView").GetComponent<PasteView>();

            copyView.OnValueCopy += Copy;
            pasteView.OnValuePaste += Paste;
            copyView.Init();
            pasteView.Init();
            copy.onClick.AddListener(() => OpenPanel(true));
            paste.onClick.AddListener(() => OpenPanel(false));

            EditorOp.Resolve<SelectionHandler>().SelectionChanged += CloseAllPanels;
        }

        private void CloseAllPanels(List<GameObject> gm)
        {
            copyPastePanel.SetActive(false);
            copyView.gameObject.SetActive(false);
            pasteView.gameObject.SetActive(false);
        }

        private void OpenCopyPastePanel()
        {
            var open =!copyPastePanel.activeSelf;
            copyPastePanel.SetActive(open);
            if(!open)
            {
                copyView.gameObject.SetActive(false);
                pasteView.gameObject.SetActive(false);
            }
        }



        private void OpenPanel(bool copy)
        {
            copyView.gameObject.SetActive(copy);
            pasteView.gameObject.SetActive(!copy);
        }

        private void Copy(TransFormCopyValues type)
        {
            var t = Value as Transform;
            EditorOp.Resolve<CopyPasteSystem>().CopyTransformData(type, t);
            copyPastePanel.SetActive(false);
            copyView.gameObject.SetActive(false);
            copyView?.Repaint();
            pasteView?.Repaint();
        }

        private void Paste(TransFormCopyValues type)
        {
            var selected = EditorOp.Resolve<SelectionHandler>().GetSelectedObjects();
            Snapshots.TransformsSnapshot.CreateSnapshot(selected);
            foreach (var s in selected)
            {
                var t = s.transform;
                EditorOp.Resolve<CopyPasteSystem>().PasteTransformData(type, t);
                copyPastePanel.SetActive(false);
                pasteView.gameObject.SetActive(false);
            }
        }

        public override bool SupportsType(Type type)
        {
            return type == typeof(Transform);
        }

        protected override void GenerateElements()
        {
            CreateDrawerForVariable(positionProp, "Position");
            CreateDrawerForVariable(rotationProp, "Rotation");
            CreateDrawerForVariable(scaleProp, "Scale");
        }

        protected override void ClearElements()
        {
            base.ClearElements();
            didCheckForExpand = false;
        }

        public override void Refresh()
        {
            if (!didCheckForExpand && Value != null)
            {
                didCheckForExpand = true;
                IsExpanded = true;

            }
            base.Refresh();
            copyView?.Repaint();
            pasteView?.Repaint();
        }


    }
}