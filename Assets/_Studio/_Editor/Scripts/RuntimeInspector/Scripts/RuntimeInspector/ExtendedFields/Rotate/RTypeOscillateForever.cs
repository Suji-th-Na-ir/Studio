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
    public class RTypeOscillateForever : MonoBehaviour
    {
        public Dropdown axisDropDown;
        public TMP_InputField degreesInput;
        public TMP_InputField speedInput;

        private void Start()
        {
            UpdateFields();
        }

        private void UpdateFields()
        {
            List<string> data = new List<string>();
            
            // axis 
            axisDropDown.options.Clear();
            data.Clear();
            data = Enum.GetNames(typeof(Axis)).ToList();
            Helper.UpdateDropDown(axisDropDown, data);
            
            speedInput.text = "";
            degreesInput.text = "";
        }
         
    }
}
