using System;
using Terra.Studio;
using UnityEngine;
using UnityEngine.UI;
using Terra.Studio.RTEditor;

namespace RuntimeInspectorNamespace
{
    public class RotateField : InspectorField
    {
#pragma warning disable 0649
        [SerializeField] public Dropdown rotateTypesDD;
        
        public RTypeOnce rotateOnce;
        public RTypeForever rotateForever;
        public RTypeOscillate rotateOscillate;
        public RTypeOscillateForever rotateOscillateForever;
        public RTypeIncremental rotateIncremental;
        public RTypeIncrementalForever rotateIncrementalForever;
#pragma warning restore 0649
        
        public override void Initialize()
        {
            base.Initialize();
            rotateTypesDD.onValueChanged.AddListener(OnRotateTypesValueChanged);
            rotateTypesDD.SetValueWithoutNotify(0);
        }
        

        public override bool SupportsType( Type type )
        {
            return type == typeof( Atom.Rotate );
        }

        private void OnRotateTypesValueChanged(int _index)
        {
            Atom.Rotate sfx = (Atom.Rotate)Value;
            ShowRotateOptionsMenu(_index);
            // sfx.clipName = Helper.GetSfxClipNameByIndex(index);
            // sfx.clipIndex = index;
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
            switch (_index)
            {
                case 0:
                    rotateOnce.gameObject.SetActive(true);
                    break;
                case 1:
                    rotateForever.gameObject.SetActive(true);
                    break;
                case 2:
                    rotateOscillate.gameObject.SetActive(true);
                    break;
                case 3:
                    rotateOscillateForever.gameObject.SetActive(true);
                    break;
                case 4:
                    rotateIncremental.gameObject.SetActive(true);
                    break;
                case 5:
                    rotateIncrementalForever.gameObject.SetActive(true);
                    break;
                default:
                    break;
            }
        }

        private void OnToggleValueChanged( bool _input )
        {
            if(Inspector)  Inspector.RefreshDelayed();
            Atom.Rotate sfx = (Atom.Rotate)Value;
            // sfx.canPlay = _input;
        }

        protected override void OnSkinChanged()
        {
            base.OnSkinChanged();

            Vector2 rightSideAnchorMin = new Vector2( Skin.LabelWidthPercentage, 0f );
            variableNameMask.rectTransform.anchorMin = rightSideAnchorMin;
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
