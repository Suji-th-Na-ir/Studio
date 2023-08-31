using System;
using System.Collections.Generic;
using PlayShifu.Terra;
using UnityEngine;
using UnityEngine.UI;
using Terra.Studio;

namespace RuntimeInspectorNamespace
{
    public class PlaySFXField : InspectorField
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
            LoadSfxClips();
            ShowHideOptionsDropdown();
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
            return type == typeof( Atom.PlaySfx );
        }

        private void OnDropdownValueChanged(int index)
        {
            Atom.PlaySfx sfx = (Atom.PlaySfx)Value;
            sfx.data.clipName = Helper.GetSfxClipNameByIndex(index);
            sfx.data.clipIndex = index;
            UpdateData(sfx);
        }

        private void OnToggleValueChanged( bool _input )
        {
            LoadSfxClips();
            if(Inspector)  Inspector.RefreshDelayed();
            Atom.PlaySfx sfx = (Atom.PlaySfx)Value;
            sfx.data.canPlay = _input;
            if (sfx.data.canPlay)
            {
                OnDropdownValueChanged(sfx.data.clipIndex);
            }
            ShowHideOptionsDropdown();
            UpdateData(sfx);
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

        protected override void OnSkinChanged()
        {
            base.OnSkinChanged();

            toggleBackground.color = Skin.InputFieldNormalBackgroundColor;
            toggleInput.graphic.color = Skin.ToggleCheckmarkColor;

            Vector2 rightSideAnchorMin = new Vector2( Skin.LabelWidthPercentage, 0f );
            variableNameMask.rectTransform.anchorMin = rightSideAnchorMin;
            ( (RectTransform) toggleInput.transform ).anchorMin = rightSideAnchorMin;
        }
        
        private void UpdateData(Atom.PlaySfx _sfx)
        {
            List<GameObject> selectedObjects = EditorOp.Resolve<SelectionHandler>().GetSelectedObjects();
            if (selectedObjects.Count <= 1) return;
            foreach (var obj in selectedObjects)
            {
                foreach (Atom.PlaySfx sfx in Atom.PlaySfx.AllInstances)
                {
                    if (obj.GetInstanceID() == sfx.target.GetInstanceID())
                    {
                        if (_sfx.componentType.Equals(sfx.componentType))
                        {
                            sfx.data = Helper.DeepCopy(_sfx.data);
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
            Atom.PlaySfx sfx = (Atom.PlaySfx)Value;
            if (sfx != null)
            {
                toggleInput.isOn = sfx.data.canPlay;
                optionsDropdown.value = sfx.data.clipIndex;
            }
        }
    }
}