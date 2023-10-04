using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Terra.Studio
{
    struct ComponentCPData
    {
        public string Name;
        public object data;
    }
    struct TransformCPData
    {
        public string Name;
        public Vector3 data;
        public bool copied;
    }
    public class CopyPasteSystem
    {

        private TransformCPData clipboardPositionData;
        private TransformCPData clipboardRotationData;
        private TransformCPData clipboardScaleData;

        public bool IsLastPositionData { get { return clipboardPositionData.copied; } }
        public bool IsLastRotationData { get { return clipboardRotationData.copied; } }
        public bool IsLastScaleData { get { return clipboardScaleData.copied; } }

        private ComponentCPData clipboardComponentData;

        public void CopyTransformData(TransFormCopyValues valueType, Transform transform)
        {
            switch (valueType)
            {
                case TransFormCopyValues.Position:
                    clipboardPositionData = new TransformCPData { Name = "localPosition", data = transform.localPosition ,copied=true };
                    clipboardRotationData = new TransformCPData { Name = "localRotation", data = transform.localPosition ,copied=false };
                    clipboardScaleData = new TransformCPData { Name = "localScale", data = transform.localPosition ,copied=false };
                    break;
                case TransFormCopyValues.Rotation:
                    clipboardRotationData = new TransformCPData { Name = "localEulerAngles", data = transform.localEulerAngles, copied = true };
                    clipboardScaleData = new TransformCPData { Name = "localScale", data = transform.localPosition, copied = false };
                    clipboardPositionData = new TransformCPData { Name = "localPosition", data = transform.localPosition, copied = false };
                    break;
                case TransFormCopyValues.Scale:
                    clipboardScaleData = new TransformCPData { Name = "localScale", data = transform.localScale, copied = true };
                    clipboardRotationData = new TransformCPData { Name = "localEulerAngles", data = transform.localEulerAngles, copied = false };
                    clipboardPositionData = new TransformCPData { Name = "localPosition", data = transform.localPosition, copied = false };
                    break;
                case TransFormCopyValues.All:
                    clipboardPositionData = new TransformCPData { Name = "localPosition", data = transform.localPosition, copied = true };
                    clipboardRotationData = new TransformCPData { Name = "localEulerAngles", data = transform.localEulerAngles, copied = true };
                    clipboardScaleData = new TransformCPData { Name = "localScale", data = transform.localScale, copied = true };
                    break;
            }
        }

        public void PasteTransformData(TransFormCopyValues valueType, Transform targetTransform)
        {
            switch (valueType)
            {
                case TransFormCopyValues.Position:
                    targetTransform.localPosition = clipboardPositionData.data;
                    break;
                case TransFormCopyValues.Rotation:
                    targetTransform.localEulerAngles = clipboardRotationData.data;
                    break;
                case TransFormCopyValues.Scale:
                    targetTransform.localScale = clipboardScaleData.data;
                    break;
                case TransFormCopyValues.All:
                    targetTransform.localPosition = clipboardPositionData.data;
                    targetTransform.localEulerAngles = clipboardRotationData.data;
                    targetTransform.localScale = clipboardScaleData.data;
                    break;
            }
        }

    }
}
