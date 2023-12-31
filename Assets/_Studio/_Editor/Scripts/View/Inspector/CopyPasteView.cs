using System;
using System.Collections;
using System.Collections.Generic;
using PlayShifu.Terra;
using RuntimeInspectorNamespace;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Terra.Studio
{
    public class CopyPasteView : View
    {
        private PointerEventListener copy, paste;

        CopyView copyView;
        PasteView pasteView;
        Text copyText;
        Button copyButton;
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
            copyButton = copy.GetComponent<Button>();
            copyText = copy.GetComponentInChildren<Text>();
            paste = Helper.FindDeepChild(transform, "PasteButton").GetComponent<PointerEventListener>();
            copy.PointerEnter += OpenCopyPanel;
            paste.PointerEnter += OpenPastePanel;
            GetComponent<PointerEventListener>().PointerExit += CloseSubPanels;
        }

        public void Update()
        {
            if ((Input.GetMouseButtonDown(0) && !IsMouseInsidePanel(gameObject) && !IsMouseInsidePanel(copyView.gameObject) && !IsMouseInsidePanel(pasteView.gameObject))
                 || Input.GetKeyDown(KeyCode.Escape))
            {
                CloseAll(null);
            }
        }

        private bool IsMouseInsidePanel(GameObject panel)
        {
            if (panel.activeSelf)
            {
                if (!RectTransformUtility.RectangleContainsScreenPoint(
                        panel.GetComponent<RectTransform>(),
                        Input.mousePosition))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return false;
            }
        }

        public override void Repaint()
        {
            if (EditorOp.Resolve<SelectionHandler>().GetSelectedObjects().Count > 1)
            {
                copyButton.interactable = false;
                var color = Helper.GetColorFromHex("#C8C8C8");
                color.a = 0.6f;
                copyText.color = color;
            }
            else
            {
                copyButton.interactable = true;
                copyText.color = Helper.GetColorFromHex("#FFFFFF");
            }
            copyView?.Repaint();
            pasteView?.Repaint();
        }

        private void OpenCopyPanel(PointerEventData eventData)
        {
            if (EditorOp.Resolve<SelectionHandler>().GetSelectedObjects().Count > 1)
                return;
            copyView.gameObject.SetActive(true);
            pasteView.gameObject.SetActive(false);
        }

        private void OpenPastePanel(PointerEventData eventData)
        {
            copyView.gameObject.SetActive(false);
            pasteView.gameObject.SetActive(true);
        }

        private void CloseSubPanels(PointerEventData eventData)
        {
            StartCoroutine(ClosePanels());
        }

        IEnumerator ClosePanels()
        {
            yield return new WaitForEndOfFrame();
            if (copyView.PointerEntered || pasteView.PointerEntered)
                yield break;
            copyView.gameObject.SetActive(false);
            pasteView.gameObject.SetActive(false);
        }

        private void CloseAll(PointerEventData eventData)
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
