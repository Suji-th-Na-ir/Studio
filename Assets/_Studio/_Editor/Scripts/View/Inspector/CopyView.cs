using System;

using PlayShifu.Terra;
using RuntimeInspectorNamespace;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Terra.Studio
{
    public class CopyView : View
    {
        public Action<TransFormCopyValues> OnValueCopy;
        private Button copyPos, copyRot, copyScale, copyAll;
        PointerEventListener pointerEventListener;
        public bool pointerEntered;
        public override void Draw()
        {
           
        }

        public override void Flush()
        {
            Destroy(this);  
        }

        public override void Init()
        {
            copyPos = Helper.FindDeepChild(transform, "CopyPos").GetComponent<Button>();
            copyRot = Helper.FindDeepChild(transform, "CopyRot").GetComponent<Button>();
            copyScale = Helper.FindDeepChild(transform, "CopyScale").GetComponent<Button>();
            copyAll = Helper.FindDeepChild(transform, "CopyAll").GetComponent<Button>();

            copyPos.onClick.AddListener(() => Copy(TransFormCopyValues.Position));
            copyRot.onClick.AddListener(() => Copy(TransFormCopyValues.Rotation));
            copyScale.onClick.AddListener(() => Copy(TransFormCopyValues.Scale));
            copyAll.onClick.AddListener(() => Copy(TransFormCopyValues.All));
            gameObject.SetActive(false);

            pointerEventListener = GetComponent<PointerEventListener>();
            pointerEventListener.PointerEnter += (PointerEventData data) => { pointerEntered = true; };
            pointerEventListener.PointerExit += (PointerEventData data) => { pointerEntered = false; gameObject.SetActive(false); };
        }

        private void Copy(TransFormCopyValues type)
        {
            OnValueCopy?.Invoke(type);
            gameObject.SetActive(false);
        }

        public override void Repaint()
        {
           
        }

        private void OnEnable()
        {
            pointerEntered = false;
        }

    }
}
