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
    public class RTypeForever : MonoBehaviour
    {
        public Dropdown axisDropDown;
        public Dropdown dirDropDown;
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
            
            // direction
            dirDropDown.options.Clear();
            data.Clear();
            data = Enum.GetNames(typeof(Direction)).ToList();
            Helper.UpdateDropDown(dirDropDown, data);

            speedInput.text = "";
        }
    }
}
