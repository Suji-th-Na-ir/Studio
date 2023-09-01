using System;
using System.Linq;
using UnityEngine;
using Terra.Studio;
using UnityEngine.UI;
using PlayShifu.Terra;
using System.Collections.Generic;

namespace RuntimeInspectorNamespace
{
    public class RotateTypes : MonoBehaviour
    {
        public RotationType myType;
        // public Dropdown axisDropDown;
        public Dropdown dirDropDown;

        public Toggle xAxis;
        public Toggle yAxis;
        public Toggle zAxis;

        public InputField degreesInput = null;
        public InputField speedInput = null;
        public InputField pauseInput = null;
        public InputField repeatInput = null;

        public Dropdown broadcastAt;
        public InputField broadcastInput;
        public InputField listenInput;
        public Toggle canListenMultipleTimesToggle;

        [HideInInspector]
        public RotateField rotateField = null;
        private RotateComponentData data = new();

        public void Setup()
        {
            LoadDefaultValues();
            if (xAxis != null) xAxis.onValueChanged.AddListener((value) =>
            {
                data.Xaxis = value;
                UpdateData(data);
            });
            if (yAxis != null) yAxis.onValueChanged.AddListener((value) =>
            {
                data.Yaxis = value;
                UpdateData(data);
            });

            if (zAxis != null) zAxis.onValueChanged.AddListener((value) =>
            {
                data.Zaxis = value;
                UpdateData(data);
            });
            //if (axisDropDown != null) axisDropDown.onValueChanged.AddListener((value) =>
            //{
            //    data.axis = GetAxis(axisDropDown.options[value].text);
            //    UpdateData(data);
            //});
            if (dirDropDown != null) dirDropDown.onValueChanged.AddListener((value) =>
            {
                data.direction = GetDirection(dirDropDown.options[value].text);
                UpdateData(data);
            });
            if (broadcastAt != null) broadcastAt.onValueChanged.AddListener((value) =>
            {
                data.broadcastAt = GetBroadcastAt(broadcastAt.options[value].text);
                UpdateData(data);
            });
            if (degreesInput != null) degreesInput.onValueChanged.AddListener((value) =>
            {
                data.degrees = Helper.StringToFloat(value);
                UpdateData(data);
            });
            if (speedInput != null) speedInput.onValueChanged.AddListener((value) =>
            {
                data.speed = Helper.StringToFloat(value);
                UpdateData(data);
            });
            if (pauseInput != null) pauseInput.onValueChanged.AddListener((value) =>
            {
                data.pauseBetween = Helper.StringToFloat(value);
                UpdateData(data);
            });
            if (repeatInput != null) repeatInput.onValueChanged.AddListener((value) =>
            {
                data.repeat = Helper.StringInInt(value);
                UpdateData(data);
            });
            if (broadcastInput != null) broadcastInput.onValueChanged.AddListener((value) =>
            {
                EditorOp.Resolve<UILogicDisplayProcessor>().UpdateBroadcastString(value, data.broadcast, new ComponentDisplayDock() { componentGameObject = ((Atom.Rotate)rotateField.Value).referenceGO, componentType = typeof(Atom.Rotate).Name });
                data.broadcast = value;
                UpdateData(data);
            });
            if (listenInput != null) listenInput.onValueChanged.AddListener((value) =>
            {
                EditorOp.Resolve<UILogicDisplayProcessor>().UpdateListnerString(value, data.listenTo, new ComponentDisplayDock() { componentGameObject = ((Atom.Rotate)rotateField.Value).referenceGO, componentType = typeof(Atom.Rotate).Name });
                data.listenTo = value;
                UpdateData(data);
            });
            if (canListenMultipleTimesToggle != null) canListenMultipleTimesToggle.onValueChanged.AddListener((value) =>
            {
                data.listen = value ? Listen.Always : Listen.Once;
                UpdateData(data);
            });
        }

        private void UpdateData(RotateComponentData _data)
        {
            rotateField.UpdateData(_data);
            UpdateOtherSelectedObjects(_data);
            // ComponentMessenger.UpdateCompData(_data);
        }

        private void UpdateOtherSelectedObjects(RotateComponentData _data)
        {
            List<GameObject> selectedObjecs = EditorOp.Resolve<SelectionHandler>().GetSelectedObjects();

            if (selectedObjecs.Count > 1)
            {
                foreach (var obj in selectedObjecs)
                {
                    if (obj.GetComponent<Rotate>() != null)
                    {
                        Rotate rotate = obj.GetComponent<Rotate>();
                        rotate.Type.data = Helper.DeepCopy(_data);
                    }
                }
            }
        }

        private Axis GetAxis(string _value)
        {
            if (_value == Axis.X.ToString()) return Axis.X;
            else if (_value == Axis.Y.ToString()) return Axis.Y;
            else if (_value == Axis.Z.ToString()) return Axis.Z;
            return Axis.X;
        }

        private Direction GetDirection(string _value)
        {
            if (_value == Direction.Clockwise.ToString()) return Direction.Clockwise;
            else if (_value == Direction.AntiClockwise.ToString()) return Direction.AntiClockwise;
            return Direction.Clockwise;
        }

        private BroadcastAt GetBroadcastAt(string _value)
        {
            if (_value == BroadcastAt.End.ToString()) return BroadcastAt.End;
            else if (_value == BroadcastAt.Never.ToString()) return BroadcastAt.Never;
            else if (_value == BroadcastAt.AtEveryInterval.ToString()) return BroadcastAt.AtEveryInterval;
            return BroadcastAt.Never;
        }

        public void LoadDefaultValues()
        {
            // axis 
            //  if (axisDropDown != null) { axisDropDown.AddOptions(Enum.GetNames(typeof(Axis)).ToList()); }

            // direction
            if (dirDropDown != null) { dirDropDown.AddOptions(Enum.GetNames(typeof(Direction)).ToList()); }

            // broadcast at 
            if (broadcastAt != null) { broadcastAt.AddOptions(Enum.GetNames(typeof(BroadcastAt)).ToList()); }

            if (myType == RotationType.OscillateForever || myType == RotationType.IncrementallyRotateForever)
            {
                broadcastAt.ClearOptions();
                broadcastAt.AddOptions(new List<string>()
                {
                    BroadcastAt.Never.ToString(),
                    BroadcastAt.AtEveryInterval.ToString()
                });
            }
        }

        public void SetData(RotateComponentData _data)
        {
            data = _data;
            // if (axisDropDown) axisDropDown.SetValueWithoutNotify((int)Enum.Parse(typeof(Axis), _data.axis.ToString()));
            if (xAxis) xAxis.SetIsOnWithoutNotify(_data.Xaxis);
            if (yAxis) yAxis.SetIsOnWithoutNotify(_data.Yaxis);
            if (zAxis) zAxis.SetIsOnWithoutNotify(_data.Zaxis);
            if (dirDropDown) dirDropDown.SetValueWithoutNotify((int)Enum.Parse(typeof(Direction), _data.direction.ToString()));
            if (broadcastAt) broadcastAt.SetValueWithoutNotify((int)Enum.Parse(typeof(BroadcastAt), _data.broadcastAt.ToString()));
            if (degreesInput) degreesInput.SetTextWithoutNotify(_data.degrees.ToString());
            if (speedInput) speedInput.SetTextWithoutNotify(_data.speed.ToString());
            if (pauseInput) pauseInput.SetTextWithoutNotify(_data.pauseBetween.ToString());
            if (repeatInput) repeatInput.SetTextWithoutNotify(_data.repeat.ToString());
            if (broadcastInput) broadcastInput.SetTextWithoutNotify(_data.broadcast);
            if (listenInput) listenInput.SetTextWithoutNotify(_data.listenTo);
            if (canListenMultipleTimesToggle) canListenMultipleTimesToggle.SetIsOnWithoutNotify(_data.listen == Listen.Always);
        }

        public void ApplySkin(UISkin Skin)
        {
            xAxis?.SetupToggeleSkin(Skin);
            yAxis?.SetupToggeleSkin(Skin);
            zAxis?.SetupToggeleSkin(Skin);
            canListenMultipleTimesToggle?.SetupToggeleSkin(Skin);
            degreesInput?.SetupInputFieldSkin(Skin);
            speedInput?.SetupInputFieldSkin(Skin);
            pauseInput?.SetupInputFieldSkin(Skin);
            repeatInput?.SetupInputFieldSkin(Skin);
            broadcastInput?.SetupInputFieldSkin(Skin);
            listenInput?.SetupInputFieldSkin(Skin);
            dirDropDown?.SetSkinDropDownField(Skin);
            broadcastAt?.SetSkinDropDownField(Skin);
        }
    }
}