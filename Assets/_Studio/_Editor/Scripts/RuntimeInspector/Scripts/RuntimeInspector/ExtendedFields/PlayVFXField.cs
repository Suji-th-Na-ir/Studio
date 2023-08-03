using System;
using PlayShifu.Terra;
using UnityEngine;
using UnityEngine.UI;
using Terra.Studio.RTEditor;

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
            vfx.clipName = Helper.GetVfxClipNameByIndex(index);
            vfx.clipIndex = index;
        }

        private void OnToggleValueChanged(bool input)
        {
            LoadVfxClips();
            if(Inspector) Inspector.RefreshDelayed();
            Atom.PlayVfx vfx = (Atom.PlayVfx)Value;
            vfx.canPlay = input;
            
            ShowHideOptionsDropdown();
        }
        
        void ShowHideOptionsDropdown()
        {
            if(toggleInput.isOn)
                optionsDropdown.gameObject.SetActive(true);
            else 
                optionsDropdown.gameObject.SetActive(false);
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
                toggleInput.isOn = vfx.canPlay;
                optionsDropdown.value = vfx.clipIndex;
            }
        }
    }
}