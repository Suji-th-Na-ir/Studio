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
    public class PasteView : View
    {
        public Action<TransFormCopyValues> OnValuePaste;
        private Button pastePos, pasteRot, pasteScale, pasteAll;
        PointerEventListener pointerEventListener;
        public bool pointerEntered {  get; private set; }
        public override void Draw()
        {

        }

        public override void Flush()
        {
            Destroy(this);
        }

        public override void Init()
        {
            pastePos = Helper.FindDeepChild(transform, "PastePos").GetComponent<Button>();
            pasteRot = Helper.FindDeepChild(transform, "PasteRot").GetComponent<Button>();
            pasteScale = Helper.FindDeepChild(transform, "PasteScale").GetComponent<Button>();
            pasteAll = Helper.FindDeepChild(transform, "PasteAll").GetComponent<Button>();

            pastePos.onClick.AddListener(() => OnValuePaste?.Invoke(TransFormCopyValues.Position));
            pasteRot.onClick.AddListener(() => OnValuePaste?.Invoke(TransFormCopyValues.Rotation));
            pasteScale.onClick.AddListener(() => OnValuePaste?.Invoke(TransFormCopyValues.Scale));
            pasteAll.onClick.AddListener(() => OnValuePaste?.Invoke(TransFormCopyValues.All));
            gameObject.SetActive(false);

            pointerEventListener = GetComponent<PointerEventListener>();
            pointerEventListener.PointerEnter += (PointerEventData data) => { pointerEntered = true; };
            pointerEventListener.PointerExit += (PointerEventData data) => { pointerEntered = false; gameObject.SetActive(false); };
        }

        public override void Repaint()
        {
            var CopyPasteSystem = EditorOp.Resolve<CopyPasteSystem>();

            pastePos.interactable = CopyPasteSystem.IsLastPositionData;
            pasteRot.interactable = CopyPasteSystem.IsLastRotationData;
            pasteScale.interactable = CopyPasteSystem.IsLastScaleData;
            pasteAll.interactable = CopyPasteSystem.IsLastScaleData && CopyPasteSystem.IsLastRotationData && CopyPasteSystem.IsLastPositionData;
        }
    }
}
