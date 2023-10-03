using System;
using System.Collections;
using System.Collections.Generic;
using PlayShifu.Terra;
using UnityEngine;
using UnityEngine.UI;

namespace Terra.Studio
{
    public class CopyView : View
    {
        public Action<TransFormCopyValues> OnValueCopy;
        private Button copyPos, copyRot, copyScale, copyAll;
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

            copyPos.onClick.AddListener(()=>OnValueCopy?.Invoke(TransFormCopyValues.Position));
            copyRot.onClick.AddListener(()=>OnValueCopy?.Invoke(TransFormCopyValues.Rotation));
            copyScale.onClick.AddListener(()=>OnValueCopy?.Invoke(TransFormCopyValues.Scale));
            copyAll.onClick.AddListener(()=>OnValueCopy?.Invoke(TransFormCopyValues.All));
            gameObject.SetActive(false);
        }

        public override void Repaint()
        {
           
        }

    }
}
