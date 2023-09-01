using System;
using System.Collections.Generic;
using System.Linq;
using Terra.Studio;
using UnityEngine;
using UnityEngine.UI;
using System.Reflection;
using PlayShifu.Terra;

namespace RuntimeInspectorNamespace
{
    public class RotateField : InspectorField
    {
#pragma warning disable 0649
        [SerializeField] public Dropdown rotateTypesDD;
        public RotateTypes[] allRotateTypes;
#pragma warning restore 0649
        private RotateTypes selectedRotateType;

        public override void Initialize()
        {
            base.Initialize();
            Setup();
        }
        
        private void Setup()
        {
            foreach (var type in allRotateTypes)
            {
                type.field = this;
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
            ShowRotateOptionsMenu(_index, true);
        }

        private void HideAllRotateOptionsMenus()
        {
            foreach (var type in allRotateTypes)
            {
                type.gameObject.SetActive(false);
            }
        }

        private void ShowRotateOptionsMenu(int _index, bool reset = false)
        {
            HideAllRotateOptionsMenus();
            Atom.Rotate rt = (Atom.Rotate)Value;
            rt.data.rotateType = _index;
            allRotateTypes[_index].gameObject.SetActive(true);
            selectedRotateType = allRotateTypes[_index];
            reset = reset || IsDataDefault();
            if (reset)
            {
                ResetValues();
            }
        }

        private void ResetValues()
        {
            var rotationType = (RotationType)((Atom.Rotate)Value).data.rotateType;
            var finalPath = rotationType.GetPresetName("Rotate");
            var preset = ((RotatePreset)EditorOp.Load(ResourceTag.ComponentPresets, finalPath)).Value;
            LoadData(preset);
            UpdateTypeForMultiselect(rotationType,preset);
        }

        private void UpdateTypeForMultiselect(RotationType _data, RotateComponentData? compData = null)
        {
            List<GameObject> selectedObjecs = EditorOp.Resolve<SelectionHandler>().GetSelectedObjects();

            if (selectedObjecs.Count > 1)
            {
                foreach (var obj in selectedObjecs)
                {
                    if (obj.GetComponent<Rotate>() != null)
                    {
                        Rotate rotate = obj.GetComponent<Rotate>();
                        rotate.Type.data.rotateType = (int)_data;
                        rotate.Type.data = compData.Value;
                    }
                }
            }
        }

        private bool IsDataDefault()
        {
            var data = ((Atom.Rotate)Value).data;
            if (data.Equals(default(RotateComponentData)))
            {
                return true;
            }
            return false;
        }

        protected override void OnSkinChanged()
        {
            base.OnSkinChanged();
            Vector2 rightSideAnchorMin = new Vector2(Skin.LabelWidthPercentage, 0f);
            if (variableNameMask != null)
            {
                variableNameMask.rectTransform.anchorMin = rightSideAnchorMin;
                variableNameMask.color = Skin.InputFieldTextColor;
            }
            rotateTypesDD.SetSkinDropDownField(Skin);
            foreach (var types in allRotateTypes)
            {
                types.ApplySkin(Skin);
            }
        }

        public Atom.Rotate GetAtom()
        {
            return (Atom.Rotate)Value;
        }

        protected override void OnBound(MemberInfo variable)
        {
            base.OnBound(variable);
            Atom.Rotate rt = (Atom.Rotate)Value;
            // Debug.Log($"translate atom data x value {rt.data.Xaxis}");
            rotateTypesDD.onValueChanged.AddListener(OnRotateTypesValueChanged);
            int rotationTypeIndex = (int)Enum.Parse(typeof(RotationType), rt.data.rotateType.ToString());
            rotateTypesDD.SetValueWithoutNotify(rotationTypeIndex);
            ShowRotateOptionsMenu(rotationTypeIndex);
            selectedRotateType.SetData(rt.data);
        }

        public override void Refresh()
        {
            base.Refresh();
            LoadData();
        }

        private void LoadData(RotateComponentData? compData = null)
        {
            Atom.Rotate rt = (Atom.Rotate)Value;
            rotateTypesDD.SetValueWithoutNotify(rt.data.rotateType);
            if (compData != null && compData.HasValue)
            {
                selectedRotateType.SetData(compData.Value);
                rt.data = compData.Value;
            }
            else
            {
                selectedRotateType.SetData(rt.data);
            }
        }
    }
}
