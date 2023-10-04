using System;
using System.Collections;
using System.Collections.Generic;
using PlayShifu.Terra;
using RuntimeInspectorNamespace;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Terra.Studio
{
    public class CopyPasteView : View
    {
        private PointerEventListener copy, paste;
        CopyView copyView;
        PasteView pasteView;
        public override void Draw()
        {

        }

        public override void Flush()
        {
          
        }

        public override void Init()
        {
            copyView = Helper.FindDeepChild(transform.parent, "CopyView").GetComponent<CopyView>();
            pasteView = Helper.FindDeepChild(transform.parent, "PasteView").GetComponent<PasteView>();

            copyView.Init();
            pasteView.Init();
            copy = Helper.FindDeepChild(transform, "CopyButton").GetComponent<PointerEventListener>();
            paste = Helper.FindDeepChild(transform, "PasteButton").GetComponent<PointerEventListener>();
            copy.PointerEnter += OpenCopyPanel;
            paste.PointerEnter += OpenPastePanel;
            GetComponent<PointerEventListener>().PointerExit += CloseSubPanel;
            EditorOp.Resolve<SelectionHandler>().SelectionChanged += CloseAllPanels;
        }

        public override void Repaint()
        {
            copyView?.Repaint();
            pasteView?.Repaint();
        }

        private void OpenCopyPanel(PointerEventData eventData)
        {
            copyView.gameObject.SetActive(true);
            pasteView.gameObject.SetActive(false);
        }

        private void OpenPastePanel(PointerEventData eventData)
        {
            copyView.gameObject.SetActive(false);
            pasteView.gameObject.SetActive(true);
        }

        private void CloseSubPanel(PointerEventData eventData)
        {       
            StartCoroutine(ClosePanels());
        }

        IEnumerator ClosePanels()
        {
            yield return new WaitForEndOfFrame();
            if (copyView.pointerEntered ||pasteView.pointerEntered)
                yield break;
            copyView.gameObject.SetActive(false);
            pasteView.gameObject.SetActive(false);
        }

        private void CloseAllPanels(List<GameObject> gm)
        {
            gameObject.SetActive(false);
            copyView.gameObject.SetActive(false);
            pasteView.gameObject.SetActive(false);
        }

        public void ApplyCopyPasteAction(Action<TransFormCopyValues> OnCopy, Action<TransFormCopyValues> OnPaste)
        {
            copyView.OnValueCopy += OnCopy;
            pasteView.OnValuePaste += OnPaste;
        }
       

    }
}
