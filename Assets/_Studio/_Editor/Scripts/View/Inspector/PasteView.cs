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
        public bool PointerEntered { get; private set; }
        Text pastePosText, pasteRotText, pasteScaleText, pasteAllText;
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

            pastePosText = pastePos.GetComponentInChildren<Text>();
            pasteRotText = pasteRot.GetComponentInChildren<Text>();
            pasteScaleText = pasteScale.GetComponentInChildren<Text>();
            pasteAllText = pasteAll.GetComponentInChildren<Text>();

            pastePos.onClick.AddListener(() => Paste(TransFormCopyValues.Position));
            pasteRot.onClick.AddListener(() => Paste(TransFormCopyValues.Rotation));
            pasteScale.onClick.AddListener(() => Paste(TransFormCopyValues.Scale));
            pasteAll.onClick.AddListener(() => Paste(TransFormCopyValues.All));
            gameObject.SetActive(false);

            pointerEventListener = GetComponent<PointerEventListener>();
            pointerEventListener.PointerEnter += (PointerEventData data) => { PointerEntered = true; };
            pointerEventListener.PointerExit += (PointerEventData data) => { PointerEntered = false; gameObject.SetActive(false); };
        }

        private void Paste(TransFormCopyValues type)
        {
            OnValuePaste?.Invoke(type);
            gameObject.SetActive(false);
        }

        public override void Repaint()
        {
            var CopyPasteSystem = EditorOp.Resolve<CopyPasteSystem>();
            SetInteractable(pastePosText, pastePos, CopyPasteSystem.IsLastPositionData);
            SetInteractable(pasteRotText, pasteRot, CopyPasteSystem.IsLastRotationData);
            SetInteractable(pasteScaleText, pasteScale, CopyPasteSystem.IsLastScaleData);
            SetInteractable(pasteAllText, pasteAll, CopyPasteSystem.IsLastScaleData && CopyPasteSystem.IsLastRotationData && CopyPasteSystem.IsLastPositionData);
        }

        private void SetInteractable(Text text, Button button, bool interactable)
        {
            button.interactable = interactable;
            if (interactable)
            {
                text.color = Helper.GetColorFromHex("#FFFFFF");
            }
            else
            {
                var color = Helper.GetColorFromHex("#C8C8C8");
                color.a = 0.6f;
                text.color = color;
            }
        }
    }
}
