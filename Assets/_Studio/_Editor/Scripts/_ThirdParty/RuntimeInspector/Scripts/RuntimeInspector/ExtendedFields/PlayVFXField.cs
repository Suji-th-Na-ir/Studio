using System;
using System.Collections.Generic;
using PlayShifu.Terra;
using UnityEngine;
using UnityEngine.UI;
using Terra.Studio;

namespace RuntimeInspectorNamespace
{
    public class PlayVFXField : InspectorField
    {
#pragma warning disable 0649
        [SerializeField]
        private Image toggleBackground;

        [SerializeField]
        private Toggle toggleInput;

        [SerializeField]
        private Dropdown optionsDropdown;

#pragma warning restore 0649

        public override void Initialize()
        {
            base.Initialize();
            toggleInput.onValueChanged.AddListener(OnToggleValueChanged);
            optionsDropdown.onValueChanged.AddListener(OnDropdownValueChanged);
            LoadVfxClips();
            ShowHideOptionsDropdown();
        }

        private void LoadVfxClips()
        {
            optionsDropdown.options.Clear();

            foreach (string clipName in Helper.GetVfxClipNames())
            {
                optionsDropdown.options.Add(new Dropdown.OptionData()
                {
                    text = clipName
                });
            }
        }

        public override bool SupportsType(Type type)
        {
            return type == typeof(Atom.PlayVfx);
        }

        private void OnDropdownValueChanged(int index)
        {
            Atom.PlayVfx vfx = (Atom.PlayVfx)Value;
            vfx.data.clipName = Helper.GetVfxClipNameByIndex(index);
            vfx.data.clipIndex = index;
            UpdateData(vfx);
        }

        private void OnToggleValueChanged(bool input)
        {
            LoadVfxClips();
            if(Inspector) Inspector.RefreshDelayed();
            Atom.PlayVfx vfx = (Atom.PlayVfx)Value;
            vfx.data.canPlay = input;
            
            if (vfx.data.canPlay)
            {
                OnDropdownValueChanged(vfx.data.clipIndex);
            }
            ShowHideOptionsDropdown();
            UpdateData(vfx);
        }
        
        void ShowHideOptionsDropdown()
        {
            if (toggleInput.isOn)
            {
                optionsDropdown.gameObject.SetActive(true);
                optionsDropdown.value = 0;
            }
            else
            {
                optionsDropdown.gameObject.SetActive(false);
            }
        }

        private void SetOptionsDropdown(bool _value)
        {
            optionsDropdown.gameObject.SetActive(_value);
        }


        protected override void OnSkinChanged()
        {
            base.OnSkinChanged();

            toggleBackground.color = Skin.InputFieldNormalBackgroundColor;
            toggleInput.graphic.color = Skin.ToggleCheckmarkColor;

            Vector2 rightSideAnchorMin = new Vector2(Skin.LabelWidthPercentage, 0f);
            variableNameMask.rectTransform.anchorMin = rightSideAnchorMin;
            ((RectTransform)toggleInput.transform).anchorMin = rightSideAnchorMin;

            optionsDropdown.SetSkinDropDownField(Skin);
        }

        private void UpdateData(Atom.PlayVfx _vfx)
        {
            List<GameObject> selectedObjects = EditorOp.Resolve<SelectionHandler>().GetSelectedObjects();
            if (selectedObjects.Count <= 1) return;
            foreach (var obj in selectedObjects)
            {
                foreach (Atom.PlayVfx vfx in Atom.PlayVfx.AllInstances)
                {
                    if (obj.GetInstanceID() == vfx.target.GetInstanceID())
                    {
                        if (!string.IsNullOrEmpty(_vfx.fieldName) &&
                            !string.IsNullOrEmpty(vfx.fieldName) &&
                            !_vfx.fieldName.Equals(vfx.fieldName))
                        {
                            continue;
                        }
                        if (_vfx != null && _vfx.componentType != null && _vfx.componentType == vfx.componentType)
                        {
                            vfx.data = Helper.DeepCopy(_vfx.data);
                        }
                    }
                }
            }
        }

        public override void Refresh()
        {
            base.Refresh();
            LoadData();
        }
        
        private void LoadData()
        {
            Atom.PlayVfx vfx = (Atom.PlayVfx)Value;
            if (vfx != null)
            {
                toggleInput.isOn = vfx.data.canPlay;
                optionsDropdown.value = vfx.data.clipIndex;
            }
        }
    }
}