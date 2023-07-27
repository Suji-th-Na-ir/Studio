using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Terra.Studio.RTEditor;
using TMPro;

namespace RuntimeInspectorNamespace
{
    public class PlayVFXField : InspectorField
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
            LoadVfxClips();
        }

        private void LoadVfxClips()
        {
            var vfxPrefabs  = Resources.LoadAll("vfx", typeof(GameObject)).Cast<GameObject>().ToArray();
            optionsDropdown.options.Clear();
            foreach (var prefab in vfxPrefabs)
            {
                optionsDropdown.options.Add(new Dropdown.OptionData(){
                    text =prefab.name
                });
            }
        }

        public override bool SupportsType( Type type)
        {
            return type == typeof( Atom.PlayVFX );
        }

        private void OnValueChanged( bool input )
        {
            LoadVfxClips();
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