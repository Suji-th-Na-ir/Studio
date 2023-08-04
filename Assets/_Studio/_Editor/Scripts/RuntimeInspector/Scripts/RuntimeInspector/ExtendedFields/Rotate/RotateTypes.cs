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
        public RotationType myRotateType = RotationType.RotateOnce;
        public Dropdown axisDropDown;
        public Dropdown dirDropDown;
        
        public TMP_InputField degreesInput = null;
        public TMP_InputField speedInput = null;
        public TMP_InputField pauseInput = null;
        public TMP_InputField repeatInput = null;

        public Dropdown broadcastAt;
        public TMP_InputField broadcastInput;

        public RotateField rotateField = null;
        private RotateComponentData rdata = new RotateComponentData();

        private void Start()
        {
            LoadDefaultValues();

            if (axisDropDown != null) axisDropDown.onValueChanged.AddListener((value) => { rdata.axis = GetAxis(axisDropDown.options[value].text); rotateField.UpdateData(rdata);});
            if (dirDropDown != null) dirDropDown.onValueChanged.AddListener((value) => { rdata.direction = GetDirection(dirDropDown.options[value].text); rotateField.UpdateData(rdata);});
            
            if (degreesInput != null) degreesInput.onValueChanged.AddListener(
                (value) => { 
                    rdata.degrees = Helper.StringToFloat(value); 
                    rotateField.UpdateData(rdata);
                });
            if (speedInput != null) speedInput.onValueChanged.AddListener((value) => { rdata.speed = Helper.StringToFloat(value); rotateField.UpdateData(rdata);});
            if (pauseInput != null) pauseInput.onValueChanged.AddListener((value) => { rdata.pauseBetween = Helper.StringToFloat(value); rotateField.UpdateData(rdata);});
            if (repeatInput != null) repeatInput.onValueChanged.AddListener((value) => { rdata.repeat = Helper.StringInInt(value); rotateField.UpdateData(rdata);});
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

        private void LoadDefaultValues()
        {
            List<string> data = new List<string>();
            // axis 
            if (axisDropDown != null)
            {
                axisDropDown.options.Clear();
                data.Clear();
                data = Enum.GetNames(typeof(Axis)).ToList();
                Helper.UpdateDropDown(axisDropDown, data);
            }

            // direction
            if (dirDropDown != null)
            {
                dirDropDown.options.Clear();
                data.Clear();
                data = Enum.GetNames(typeof(Direction)).ToList();
                Helper.UpdateDropDown(dirDropDown, data);
            }
            
            // broadcast at 
            if (broadcastAt != null)
            {
                broadcastAt.options.Clear();
                data.Clear();
                Helper.UpdateDropDown(broadcastAt, new List<string>()
                {
                    "Never", "AtEveryInterval", "End"
                });
            }

            switch (myRotateType)
            {
                case RotationType.RotateOnce:
                    if (speedInput != null) speedInput.text = "0";
                    if (degreesInput != null) degreesInput.text = "0";
                    if (pauseInput != null) pauseInput.text = "0";
                    if (repeatInput != null) repeatInput.text = "0";
                    if (broadcastInput != null) broadcastInput.text = "";
                    break;
                case RotationType.RotateForever:
                    if (speedInput != null) speedInput.text = "0";
                    if (degreesInput != null) degreesInput.text = "0";
                    if (pauseInput != null) pauseInput.text = "0";
                    if (repeatInput != null) repeatInput.text = "0";
                    if (broadcastInput != null) broadcastInput.text = "";
                    break;
                case RotationType.Oscillate :
                    if (speedInput != null) speedInput.text = "0";
                    if (degreesInput != null) degreesInput.text = "0";
                    if (pauseInput != null) pauseInput.text = "0";
                    if (repeatInput != null) repeatInput.text = "0";
                    if (broadcastInput != null) broadcastInput.text = "";
                    break;
                case RotationType.OscillateForever:
                    if (speedInput != null) speedInput.text = "0";
                    if (degreesInput != null) degreesInput.text = "0";
                    if (pauseInput != null) pauseInput.text = "0";
                    if (repeatInput != null) repeatInput.text = "0";
                    if (broadcastInput != null) broadcastInput.text = "";
                    break;
                case RotationType.IncrementallyRotate:
                    if (speedInput != null) speedInput.text = "0";
                    if (degreesInput != null) degreesInput.text = "0";
                    if (pauseInput != null) pauseInput.text = "0";
                    if (repeatInput != null) repeatInput.text = "0";
                    if (broadcastInput != null) broadcastInput.text = "";
                    break;
                case RotationType.IncrementallyRotateForever:
                    if (speedInput != null) speedInput.text = "0";
                    if (degreesInput != null) degreesInput.text = "0";
                    if (pauseInput != null) pauseInput.text = "0";
                    if (repeatInput != null) repeatInput.text = "0";
                    if (broadcastInput != null) broadcastInput.text = "";
                    break;
            }
        }
    }
}