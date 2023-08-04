using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Codice.Client.BaseCommands;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Terra.Studio;
using PlayShifu.Terra;
using UnityEditor;

namespace RuntimeInspectorNamespace
{
    public class RotateTypes : MonoBehaviour
    {
        public Dropdown axisDropDown;
        public Dropdown dirDropDown;
        
        public TMP_InputField degreesInput = null;
        public TMP_InputField speedInput = null;
        public TMP_InputField incrementInput = null;
        public TMP_InputField pauseInput = null;
        public TMP_InputField repeatInput = null;

        public RotateField rotateField = null;
        private RotateComponentData rdata = new RotateComponentData();

        private void Start()
        {
            UpdateFields();

            if (axisDropDown != null) axisDropDown.onValueChanged.AddListener((value) => { rdata.axis = axisDropDown.options[value].text; rotateField.UpdateData(rdata);});
            if (dirDropDown != null) dirDropDown.onValueChanged.AddListener((value) => { rdata.direction = dirDropDown.options[value].text; rotateField.UpdateData(rdata);});
            
            if (degreesInput != null) degreesInput.onValueChanged.AddListener(
                (value) => { 
                    rdata.degrees = Helper.StringToFloat(value); 
                    rotateField.UpdateData(rdata);
                });
            if (speedInput != null) speedInput.onValueChanged.AddListener((value) => { rdata.speed = Helper.StringToFloat(value); rotateField.UpdateData(rdata);});
            if (incrementInput != null) incrementInput.onValueChanged.AddListener((value) => { rdata.increment = Helper.StringToFloat(value); rotateField.UpdateData(rdata);});
            if (pauseInput != null) pauseInput.onValueChanged.AddListener((value) => { rdata.pauseBetween = Helper.StringToFloat(value); rotateField.UpdateData(rdata);});
            if (repeatInput != null) repeatInput.onValueChanged.AddListener((value) => { rdata.repeat = Helper.StringInInt(value); rotateField.UpdateData(rdata);});
        }

        private void UpdateFields()
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

            if(speedInput != null) speedInput.text = null;
            if(degreesInput != null) degreesInput.text = null;
            if(incrementInput != null) incrementInput.text = null;
            if(pauseInput != null) pauseInput.text = null;
            if(repeatInput != null) repeatInput.text = null;
        }
    }
}