using System;
using System.Collections.Generic;
using System.Linq;
using Terra.Studio;
using UnityEngine;
using UnityEngine.UI;
using Terra.Studio.RTEditor;
using PlayShifu.Terra;

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
            rotateTypesDD.value = 0;
            
            rotateTypesDD.options.Clear();
            List<string> data = Enum.GetNames(typeof(RotationType)).ToList();
            Helper.UpdateDropDown(rotateTypesDD, data);
            base.layoutElement.minHeight = 220f;
            ShowRotateOptionsMenu(rotateTypesDD.value);
        }
        

        public override bool SupportsType( Type type )
        {
            return type == typeof( Atom.Rotate );
        }

        private void OnRotateTypesValueChanged(int _index)
        {
            Atom.Rotate sfx = (Atom.Rotate)Value;
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
                    rotateOnce.gameObject.SetActive(true);
                    break;
            }
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
            
        }
    }
}
