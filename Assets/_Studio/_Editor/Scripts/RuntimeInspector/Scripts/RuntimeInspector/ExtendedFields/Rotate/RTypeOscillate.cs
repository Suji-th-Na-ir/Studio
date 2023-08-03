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
    public class RTypeOscillate : MonoBehaviour
    {
        public Dropdown axisDropDown;
        public TMP_InputField degreesInput;
        public TMP_InputField speedInput;
        public TMP_InputField repeatInput;

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

            degreesInput.text = "";
            speedInput.text = "";
            repeatInput.text = "";
        }
    }
}
