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
            base.layoutElement.minHeight = 281.8f;
            Setup();
        }

        private void Setup()
        {
            rotateOnce.rotateField = this;
            rotateForever.rotateField = this;
            rotateOscillate.rotateField = this;
            rotateOscillateForever.rotateField = this;
            rotateIncremental.rotateField = this;
            rotateIncrementalForever.rotateField = this;
            
            rotateOnce.Setup();
            rotateForever.Setup();
            rotateOscillate.Setup();
            rotateOscillateForever.Setup();
            rotateIncremental.Setup();
            rotateIncrementalForever.Setup();
            
            List<string> data = Enum.GetNames(typeof(RotationType)).ToList();
            rotateTypesDD.AddOptions(data);
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
        }

        private void ShowRotateOptionsMenu(int _index)
        {
            HideAllRotateOptionsMenus();
            Atom.Rotate rt = (Atom.Rotate)Value;
            rt.data.rotateType = _index;
            switch (_index)
            {
                case 0:
                    rotateOnce.gameObject.SetActive(true);
                    selectedRotateType = rotateOnce;
                    break;
                case 1:
                    rotateForever.gameObject.SetActive(true);
                    selectedRotateType = rotateForever;
                    break;
                case 2:
                    rotateOscillate.gameObject.SetActive(true);
                    selectedRotateType = rotateOscillate;
                    break;
                case 3:
                    rotateOscillateForever.gameObject.SetActive(true);
                    selectedRotateType = rotateOscillateForever;
                    break;
                case 4:
                    rotateIncremental.gameObject.SetActive(true);
                    selectedRotateType = rotateIncremental;
                    break;
                case 5:
                    rotateIncrementalForever.gameObject.SetActive(true);
                    selectedRotateType = rotateIncrementalForever;
                    break;
            }
        }

        protected override void OnSkinChanged()
        {
            base.OnSkinChanged();
            Vector2 rightSideAnchorMin = new Vector2(Skin.LabelWidthPercentage, 0f);
            variableNameMask.rectTransform.anchorMin = rightSideAnchorMin;
        }

        public void UpdateData(RotateComponentData _rData)
        {
            Atom.Rotate rt = (Atom.Rotate)Value;
            // rotate type should come from rotatefield
            _rData.rotateType = rt.data.rotateType;
            rt.data = _rData;
        }

        protected override void OnBound(MemberInfo variable)
        {
            base.OnBound(variable);
            Atom.Rotate rt = (Atom.Rotate)Value;
            rotateTypesDD.onValueChanged.AddListener(OnRotateTypesValueChanged);
            int rotationTypeIndex = (((int)Enum.Parse(typeof(RotationType), rt.data.rotateType.ToString())));
            rotateTypesDD.value = rotationTypeIndex;
            ShowRotateOptionsMenu(rotationTypeIndex);
            selectedRotateType.SetData(rt.data);
        }
    }
}
