using System;
using PlayShifu.Terra;
using UnityEngine;
using UnityEngine.UI;
using Terra.Studio.RTEditor;

namespace RuntimeInspectorNamespace
{
    public class RotateField : InspectorField
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
            toggleInput.onValueChanged.AddListener( OnToggleValueChanged );
            optionsDropdown.onValueChanged.AddListener(OnDropdownValueChanged);
            ShowHideOptionsDropdown();
        }
        

        public override bool SupportsType( Type type )
        {
            return type == typeof( Atom.Rotate );
        }

        private void OnDropdownValueChanged(int index)
        {
            Atom.Rotate sfx = (Atom.Rotate)Value;
            // sfx.clipName = Helper.GetSfxClipNameByIndex(index);
            // sfx.clipIndex = index;
        }

        private void OnToggleValueChanged( bool _input )
        {
            if(Inspector)  Inspector.RefreshDelayed();
            Atom.Rotate sfx = (Atom.Rotate)Value;
            // sfx.canPlay = _input;
            ShowHideOptionsDropdown();
        }

        void ShowHideOptionsDropdown()
        {
            if(toggleInput.isOn)
                optionsDropdown.gameObject.SetActive(true);
            else 
                optionsDropdown.gameObject.SetActive(false);
        }

        protected override void OnSkinChanged()
        {
            base.OnSkinChanged();

            toggleBackground.color = Skin.InputFieldNormalBackgroundColor;
            toggleInput.graphic.color = Skin.ToggleCheckmarkColor;

            Vector2 rightSideAnchorMin = new Vector2( Skin.LabelWidthPercentage, 0f );
            variableNameMask.rectTransform.anchorMin = rightSideAnchorMin;
            ( (RectTransform) toggleInput.transform ).anchorMin = rightSideAnchorMin;
        }

        public override void Refresh()
        {
            base.Refresh();
            LoadData();
        }
        
        private void LoadData()
        {
            Atom.Rotate sfx = (Atom.Rotate)Value;
            if (sfx != null)
            {
                // toggleInput.isOn = sfx.canPlay;
                // optionsDropdown.value = sfx.clipIndex;
            }
        }
    }
}
