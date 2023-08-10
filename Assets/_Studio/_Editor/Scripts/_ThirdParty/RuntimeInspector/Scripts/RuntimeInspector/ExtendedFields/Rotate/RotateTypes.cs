using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Terra.Studio;
using PlayShifu.Terra;

namespace RuntimeInspectorNamespace
{
    public class RotateTypes : MonoBehaviour
    {
        public Dropdown axisDropDown;
        public Dropdown dirDropDown;
        
        public TMP_InputField degreesInput = null;
        public TMP_InputField speedInput = null;
        public TMP_InputField pauseInput = null;
        public TMP_InputField repeatInput = null;

        public Dropdown broadcastAt;
        public TMP_InputField broadcastInput;
        public TMP_InputField listenInput;

        public RotateField rotateField = null;
        private RotateComponentData data = new RotateComponentData();
        
        public void Setup()
        {
            LoadDefaultValues();
            if (axisDropDown != null) axisDropDown.onValueChanged.AddListener((value) =>
            {
                data.axis = GetAxis(axisDropDown.options[value].text);
                UpdateData(data);
            });
            if (dirDropDown != null) dirDropDown.onValueChanged.AddListener((value) =>
            {
                data.direction = GetDirection(dirDropDown.options[value].text);
                UpdateData(data);
            });
            if (broadcastAt != null) broadcastAt.onValueChanged.AddListener((value) =>
            {
                data.broadcastAt = getBroadcastAt(broadcastAt.options[value].text);
                UpdateData(data);
            });
            
            if (degreesInput != null) degreesInput.onValueChanged.AddListener(
                (value) => { ;
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
                data.broadcast = value;
                UpdateData(data);
            });
            if (listenInput != null) listenInput.onValueChanged.AddListener((value) =>
            {
                data.listenTo = value;
                UpdateData(data);
            });
        }

        private void UpdateData(RotateComponentData _data)
        {
            rotateField.UpdateData(_data);
            UpdateOtherSelectedObjects(_data);
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

        private BroadcastAt getBroadcastAt(string _value)
        {
            if (_value == BroadcastAt.End.ToString()) return BroadcastAt.End;
            else if (_value == BroadcastAt.Never.ToString()) return BroadcastAt.Never;
            else if (_value == BroadcastAt.AtEveryInterval.ToString()) return BroadcastAt.AtEveryInterval;
            return BroadcastAt.Never;
        }
        
        public void LoadDefaultValues()
        {
            // axis 
            if (axisDropDown != null) { axisDropDown.AddOptions(Enum.GetNames(typeof(Axis)).ToList()); }

            // direction
            if (dirDropDown != null){ dirDropDown.AddOptions(Enum.GetNames(typeof(Direction)).ToList());}
            
            // broadcast at 
            if (broadcastAt != null){broadcastAt.AddOptions(Enum.GetNames(typeof(BroadcastAt)).ToList());}
        }

        public RotateComponentData GetData()
        {
            return data;
        }

        public void SetData(RotateComponentData _data)
        {
            data = _data;
            if (axisDropDown != null) axisDropDown.value = ((int)Enum.Parse(typeof(Axis), _data.axis.ToString()));
            if (dirDropDown != null) dirDropDown.value = ((int)Enum.Parse(typeof(Direction), _data.direction.ToString()));
            if(broadcastAt != null) broadcastAt.value = ((int)Enum.Parse(typeof(BroadcastAt), _data.broadcastAt.ToString()));
            if (degreesInput != null) degreesInput.text = _data.degrees.ToString();
            if (speedInput != null) speedInput.text = _data.speed.ToString();
            if (pauseInput != null) pauseInput.text = _data.pauseBetween.ToString();
            if (repeatInput != null) repeatInput.text = _data.repeat.ToString();
            if (broadcastInput != null) broadcastInput.text = _data.broadcast;
            if (listenInput != null) listenInput.text = _data.listenTo;
        }
    }
}