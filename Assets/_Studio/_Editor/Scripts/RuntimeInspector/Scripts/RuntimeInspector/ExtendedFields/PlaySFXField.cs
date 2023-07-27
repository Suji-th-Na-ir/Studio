using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Terra.Studio.RTEditor;
using TMPro;

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

        public override void Initialize()
        {
            base.Initialize();
            input.onValueChanged.AddListener( OnValueChanged );
            LoadSfxClips();
            
        }

        private void LoadSfxClips()
        {
            var sfxClips = Resources.LoadAll("sfx", typeof(AudioClip)).Cast<AudioClip>().ToArray();
            optionsDropdown.options.Clear();
            foreach (var clip in sfxClips)
            {
                optionsDropdown.options.Add(new Dropdown.OptionData() {
                    text =clip.name
                });
            }
        }

        public override bool SupportsType( Type type )
        {
            return type == typeof( Atom.PlaySFX );
        }

        private void OnValueChanged( bool input )
        {
            // Debug.Log($"goo {Inspector.InspectedObject == null} {(Inspector.InspectedObject as GameObject)?.name}");
            LoadSfxClips();
            Value = input;
            Inspector.RefreshDelayed();
            SetOptionsDropdown(input);
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
            // input.isOn = (Atom.PlaySFX) Value;
        }

        public bool IsOn()
        {
            return input.isOn;
        }

        public string GetOption()
        {
            return optionsDropdown.options[optionsDropdown.value].ToString();
        } 
    }
}