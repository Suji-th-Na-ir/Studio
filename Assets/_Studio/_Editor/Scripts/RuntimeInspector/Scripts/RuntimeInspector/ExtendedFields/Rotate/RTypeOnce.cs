using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Terra.Studio;

namespace RuntimeInspectorNamespace
{
    public class RTypeOnce : MonoBehaviour
    {
        public Dropdown typeDropDown;
        public Dropdown axisDropDown;
        public Dropdown dirDropDown;
        public TMP_InputField speedInput;
        public TMP_InputField incrementInput;
        public TMP_InputField pauseBetweenInput;

        private void Start()
        {
            UpdateFields();
        }

        private void UpdateFields()
        {
            // types 
            typeDropDown.options.Clear();
            List<string> data = Enum.GetNames(typeof(GlobalEnums.RotationType)).ToList();
            UpdateDropDown(typeDropDown, data);
            
            // axis 
            axisDropDown.options.Clear();
            data.Clear();
            data = Enum.GetNames(typeof(GlobalEnums.Axis)).ToList();
            UpdateDropDown(axisDropDown, data);
            
            // direction
            dirDropDown.options.Clear();
            data.Clear();
            data = Enum.GetNames(typeof(GlobalEnums.Direction)).ToList();
            UpdateDropDown(dirDropDown, data);

            speedInput.text = "";
            incrementInput.text = "";
            pauseBetweenInput.text = "";
        }

        private void UpdateDropDown(Dropdown _ddown, List<string> _data)
        {
            foreach (var name in _data)
            {
                Dropdown.OptionData od = new Dropdown.OptionData()
                {
                    text = name
                };
                _ddown.options.Add(od);
            }
        }
    }
}
