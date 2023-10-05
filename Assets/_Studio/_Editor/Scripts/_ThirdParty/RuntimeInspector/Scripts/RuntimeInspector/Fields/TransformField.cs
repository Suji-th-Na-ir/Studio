using System;
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
        private Button openCopyPaste;
        private CopyPasteView copyPasteView;
      
        public override void Initialize()
        {
            base.Initialize();

            positionProp = typeof(Transform).GetProperty("localPosition");
            rotationProp = typeof(Transform).GetProperty("localEulerAngles");
            scaleProp = typeof(Transform).GetProperty("localScale");

            openCopyPaste = Helper.FindDeepChild(transform, "OpenCopyPasteBtn").GetComponent<Button>();
            openCopyPaste.onClick.AddListener(() => OpenCopyPastePanel());
            copyPasteView = Helper.FindDeepChild(transform, "CopyPastePanel").GetComponent<CopyPasteView>();
            copyPasteView.gameObject.SetActive(false);
            copyPasteView.Init();
            copyPasteView.ApplyCopyPasteAction(Copy, Paste);
        }

       

        private void OpenCopyPastePanel()
        {
            var open =!copyPasteView.gameObject.activeSelf;
            copyPasteView.gameObject.SetActive(open);
        }

        private void Copy(TransFormCopyValues type)
        {
            var t = Value as Transform;
            EditorOp.Resolve<CopyPasteSystem>().CopyTransformData(type, t);
            copyPasteView.gameObject.SetActive(false);
        }

        private void Paste(TransFormCopyValues type)
        {
            var selected = EditorOp.Resolve<SelectionHandler>().GetSelectedObjects();
            Snapshots.TransformsSnapshot.CreateSnapshot(selected);
            foreach (var s in selected)
            {
                var t = s.transform;
                EditorOp.Resolve<CopyPasteSystem>().PasteTransformData(type, t);
                copyPasteView.gameObject.SetActive(false);
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
            copyPasteView.Repaint();
        }


    }
}