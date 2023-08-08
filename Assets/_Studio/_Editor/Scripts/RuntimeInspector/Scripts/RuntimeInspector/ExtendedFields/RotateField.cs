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
        public RotateTypes[] allRotateTypes;
        // public RotateTypes rotateOnce;
        // public RotateTypes rotateForever;
        // public RotateTypes rotateOscillate;
        // public RotateTypes rotateOscillateForever;
        // public RotateTypes rotateIncremental;
        // public RotateTypes rotateIncrementalForever;
#pragma warning restore 0649

        private RotateTypes selectedRotateType;

        public override void Initialize()
        {
            base.Initialize();
            base.layoutElement.minHeight = 316.9f;
            Setup();
        }

        private void Setup()
        {
            foreach (var type in allRotateTypes)
            {
                type.rotateField = this;
                type.Setup();
            }
            
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
            foreach (var type in allRotateTypes)
            {
                type.gameObject.SetActive(false);
            }
        }

        private void ShowRotateOptionsMenu(int _index)
        {
            HideAllRotateOptionsMenus();
            Atom.Rotate rt = (Atom.Rotate)Value;
            rt.data.rotateType = _index;
            allRotateTypes[_index].gameObject.SetActive(true);
            selectedRotateType = allRotateTypes[_index];
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
        
        public override void Refresh()
        {
            base.Refresh();
            LoadData(); 
        }

        private void LoadData()
        {
            Atom.Rotate rt = (Atom.Rotate)Value;
            rotateTypesDD.SetValueWithoutNotify((int)rt.data.rotateType);
            foreach (RotateTypes type in allRotateTypes)
            {
                type.SetData(rt.data);
            }
        }
    }
}
