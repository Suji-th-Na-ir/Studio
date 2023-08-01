using System;
using System.Linq;
using PlayShifu.Terra;
using UnityEngine;
using UnityEngine.UI;
using Terra.Studio.RTEditor;
using TMPro;
using UnityEngine.Serialization;

namespace RuntimeInspectorNamespace
{
    public class PlaySFXField : InspectorField
    {
#pragma warning disable 0649
        [SerializeField]
        private Image toggleBackground;

        [SerializeField]
        private Toggle input;

        [SerializeField] 
        private Dropdown optionsDropdown;
#pragma warning restore 0649

        public const string sfxToggleKey = "sfx_toggle";
        public const string sfxDropdownkey  = "sfx_dropdown";
        public const string resourceFolder = "sfx";

        public override void Initialize()
        {
            Debug.Log("created");
            base.Initialize();
            input.onValueChanged.AddListener( OnToggleValueChanged );
            optionsDropdown.onValueChanged.AddListener(OnDropdownValueChanged);
            LoadSfxClips();
            LoadItems();
        }
        
        public void LoadItems()
        {
            base.StateManagerSetup();
            if (stateManager != null)
            {
                input.isOn = stateManager.GetItem<bool>(sfxToggleKey);
                optionsDropdown.value = stateManager.GetItem<int>(sfxDropdownkey);
            }
        }

        private void LoadSfxClips()
        {
            optionsDropdown.options.Clear();
            foreach (string clipName in Helper.GetSfxClipNames())
            {
                optionsDropdown.options.Add(new Dropdown.OptionData()
                {
                    text = clipName
                });
            }
        }

        public override bool SupportsType( Type type )
        {
            return type == typeof( Atom.PlaySFX );
        }

        private void OnDropdownValueChanged(int index)
        {
            base.StateManagerSetup();
            stateManager.SetItem(sfxDropdownkey, index);
        }

        private void OnToggleValueChanged( bool input )
        {
            LoadSfxClips();
            Value = input;
            if(Inspector)  Inspector.RefreshDelayed();
            SetOptionsDropdown(input);

            base.StateManagerSetup();
            stateManager.SetItem(sfxToggleKey, input);
        }

        private void SetOptionsDropdown(bool _value)
        {
            optionsDropdown.gameObject.SetActive(_value);
        }

        protected override void OnSkinChanged()
        {
            base.OnSkinChanged();

            toggleBackground.color = Skin.InputFieldNormalBackgroundColor;
            input.graphic.color = Skin.ToggleCheckmarkColor;

            Vector2 rightSideAnchorMin = new Vector2( Skin.LabelWidthPercentage, 0f );
            variableNameMask.rectTransform.anchorMin = rightSideAnchorMin;
            ( (RectTransform) input.transform ).anchorMin = rightSideAnchorMin;
        }

        public override void Refresh()
        {
            base.Refresh();
            SetOptionsDropdown(input.isOn);
        }
    }
}