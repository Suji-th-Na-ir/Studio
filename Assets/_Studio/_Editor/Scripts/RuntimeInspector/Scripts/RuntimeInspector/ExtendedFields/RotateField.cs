using System;
using System.Collections.Generic;
using System.Linq;
using Terra.Studio;
using UnityEngine;
using UnityEngine.UI;
using Terra.Studio.RTEditor;
using PlayShifu.Terra;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace RuntimeInspectorNamespace
{
    public class RotateField : InspectorField
    {
#pragma warning disable 0649
        [SerializeField] public Dropdown rotateTypesDD;
        public RotateTypes rotateOnce;
        public RotateTypes rotateForever;
        public RotateTypes rotateOscillate;
        public RotateTypes rotateOscillateForever;
        public RotateTypes rotateIncremental;
        public RotateTypes rotateIncrementalForever;
#pragma warning restore 0649

        private RotateTypes selectedRotateType;

        public override void Initialize()
        {
            base.Initialize();
            rotateTypesDD.onValueChanged.AddListener(OnRotateTypesValueChanged);
            rotateTypesDD.value = 0;

            rotateTypesDD.options.Clear();
            List<string> data = Enum.GetNames(typeof(RotationType)).ToList();
            Helper.UpdateDropDown(rotateTypesDD, data);

            base.layoutElement.minHeight = 220f;
            Setup();

            LoadData();
        }

        private void Setup()
        {
            rotateOnce.rotateField = this;
            rotateForever.rotateField = this;
            rotateOscillate.rotateField = this;
            rotateOscillateForever.rotateField = this;
            rotateIncremental.rotateField = this;
            rotateIncrementalForever.rotateField = this;
        }

        public override bool SupportsType(Type type)
        {
            return type == typeof(Atom.Rotate);
        }

        private void OnRotateTypesValueChanged(int _index)
        {
            ShowRotateOptionsMenu(_index);
        }

        private void HideAllRotateOptionsMenus()
        {
            rotateOnce.gameObject.SetActive(false);
            rotateForever.gameObject.SetActive(false);
            rotateOscillate.gameObject.SetActive(false);
            rotateOscillateForever.gameObject.SetActive(false);
            rotateIncremental.gameObject.SetActive(false);
            rotateIncrementalForever.gameObject.SetActive(false);

            Atom.Rotate rt = (Atom.Rotate)Value;
            rt.rData = new RotateComponentData();
        }

        private void ShowRotateOptionsMenu(int _index)
        {
            HideAllRotateOptionsMenus();
            Atom.Rotate rt = (Atom.Rotate)Value;

            Debug.Log("atom rotate " + rt);

            switch (_index)
            {
                case 0:
                    rotateOnce.gameObject.SetActive(true);
                    selectedRotateType = rotateOnce;
                    rt.rData.rotateType = RotationType.RotateOnce;
                    break;
                case 1:
                    rotateForever.gameObject.SetActive(true);
                    selectedRotateType = rotateForever;
                    rt.rData.rotateType = RotationType.RotateForever;
                    break;
                case 2:
                    rotateOscillate.gameObject.SetActive(true);
                    selectedRotateType = rotateOscillate;
                    rt.rData.rotateType = RotationType.Oscillate;
                    break;
                case 3:
                    rotateOscillateForever.gameObject.SetActive(true);
                    selectedRotateType = rotateOscillateForever;
                    rt.rData.rotateType = RotationType.OscillateForever;
                    break;
                case 4:
                    rotateIncremental.gameObject.SetActive(true);
                    selectedRotateType = rotateIncremental;
                    rt.rData.rotateType = RotationType.IncrementallyRotate;
                    break;
                case 5:
                    rotateIncrementalForever.gameObject.SetActive(true);
                    selectedRotateType = rotateIncrementalForever;
                    rt.rData.rotateType = RotationType.IncrementallyRotateForever;
                    break;
                default:
                    rotateOnce.gameObject.SetActive(true);
                    selectedRotateType = rotateOnce;
                    rt.rData.rotateType = RotationType.RotateOnce;
                    break;
            }
        }

        protected override void OnSkinChanged()
        {
            base.OnSkinChanged();
            Vector2 rightSideAnchorMin = new Vector2(Skin.LabelWidthPercentage, 0f);
            variableNameMask.rectTransform.anchorMin = rightSideAnchorMin;
        }

        public override void Refresh()
        {
            base.Refresh();
        }

        private void LoadData()
        {
            if (selectedRotateType == null) selectedRotateType = rotateOnce;
            Atom.Rotate rt = (Atom.Rotate)Value;
            if (rt != null && rt.rData != null)
            {
                rotateTypesDD.SetValueWithoutNotify((int)Enum.Parse(typeof(RotationType), rt.rData.rotateType.ToString()));
                if(selectedRotateType.axisDropDown != null) 
                    selectedRotateType.axisDropDown.SetValueWithoutNotify((int)Enum.Parse(typeof(Axis), rt.rData.axis.ToString()));
                
                if(selectedRotateType.dirDropDown != null)
                    selectedRotateType.dirDropDown.SetValueWithoutNotify((int)Enum.Parse(typeof(Direction), rt.rData.direction.ToString()));

                if (selectedRotateType.degreesInput != null)
                    selectedRotateType.degreesInput.text = rt.rData.degrees.ToString();
                
                if( selectedRotateType.speedInput != null)
                    selectedRotateType.speedInput.text = rt.rData.speed.ToString();

                if( selectedRotateType.pauseInput != null)
                    selectedRotateType.pauseInput.text = rt.rData.pauseBetween.ToString();
                
                if( selectedRotateType.repeatInput != null)
                    selectedRotateType.repeatInput.text = rt.rData.repeat.ToString();
                Debug.Log("roortae");
            }
        }

        

        public void UpdateData(RotateComponentData _rData)
        {
            Atom.Rotate rt = (Atom.Rotate)Value;
            rt.rData = _rData;
        }
    }
}
