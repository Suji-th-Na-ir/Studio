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

        [SerializeField]
        private Image background;

        [SerializeField]
        private Image dropdownArrow;

        [SerializeField]
        private RectTransform templateRoot;

        [SerializeField]
        private RectTransform templateContentTransform;

        [SerializeField]
        private RectTransform templateItemTransform;

        [SerializeField]
        private Image templateBackground;

        [SerializeField]
        private Image templateCheckmark;

        [SerializeField]
        private Text templateText;
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


            Vector2 templateContentSizeDelta = templateContentTransform.sizeDelta;
            templateContentSizeDelta.y = Skin.LineHeight + 6f; // Padding at top and bottom edges
            templateContentTransform.sizeDelta = templateContentSizeDelta;

            Vector2 templateItemSizeDelta = templateItemTransform.sizeDelta;
            templateItemSizeDelta.y = Skin.LineHeight;
            templateItemTransform.sizeDelta = templateItemSizeDelta;

            // Resize the checkmark icon
            float templateCheckmarkSize = Skin.LineHeight * 0.66f;
            Vector2 templateTextSizeDelta = templateText.rectTransform.sizeDelta;
            templateTextSizeDelta.x -= templateCheckmarkSize - templateCheckmark.rectTransform.sizeDelta.x;
            templateText.rectTransform.sizeDelta = templateTextSizeDelta;
            templateCheckmark.rectTransform.sizeDelta = new Vector2(templateCheckmarkSize, templateCheckmarkSize);

            // Resize the dropdown arrow
            Vector2 dropdownTextSizeDelta = optionsDropdown.captionText.rectTransform.sizeDelta;
            dropdownTextSizeDelta.x -= templateCheckmarkSize - dropdownArrow.rectTransform.sizeDelta.x;
            optionsDropdown.captionText.rectTransform.sizeDelta = dropdownTextSizeDelta;
            dropdownArrow.rectTransform.sizeDelta = new Vector2(templateCheckmarkSize, templateCheckmarkSize);

            background.color = Skin.InputFieldNormalBackgroundColor;
            dropdownArrow.color = Skin.TextColor.Tint(0.1f);

            optionsDropdown.captionText.SetSkinInputFieldText(Skin);
            templateText.SetSkinInputFieldText(Skin);

            templateBackground.color = Skin.InputFieldNormalBackgroundColor.Tint(0.075f);
            templateCheckmark.color = Skin.ToggleCheckmarkColor;

             rightSideAnchorMin = new Vector2(Skin.LabelWidthPercentage, 0f);
            variableNameMask.rectTransform.anchorMin = rightSideAnchorMin;
            ((RectTransform)optionsDropdown.transform).anchorMin = rightSideAnchorMin;
        }
        
        private void UpdateData(Atom.PlaySfx _sfx)
        {
            List<GameObject> selectedObjects = EditorOp.Resolve<SelectionHandler>().GetSelectedObjects();
            foreach (var obj in selectedObjects)
            {
                foreach (Atom.PlaySfx sfx in Atom.PlaySfx.AllInstances)
                {
                    if (obj.GetInstanceID() == sfx.target.GetInstanceID())
                    {
                        sfx.data = Helper.DeepCopy(_sfx.data);
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