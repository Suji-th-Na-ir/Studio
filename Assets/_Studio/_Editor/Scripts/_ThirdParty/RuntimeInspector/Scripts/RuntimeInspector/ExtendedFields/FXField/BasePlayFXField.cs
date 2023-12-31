using System;
using UnityEngine;
using UnityEngine.UI;
using System.Reflection;
using RuntimeInspectorNamespace;

namespace Terra.Studio
{
    public class BasePlayFXField : InspectorField
    {
#pragma warning disable 0649

        [SerializeField]
        protected Image toggleBackground;

        [SerializeField]
        protected Toggle toggleInput;

        [SerializeField]
        protected Dropdown optionsDropdown;

#pragma warning restore 0649

        protected virtual string CommentKey { get; }
        protected virtual Type DerivedType { get; }

        public override bool SupportsType(Type type)
        {
            return type.BaseType == typeof(Atom.BasePlay);
        }

        public override void Refresh()
        {
            base.Refresh();
            LoadData();
        }

        protected override void OnBound(MemberInfo variable)
        {
            base.OnBound(variable);
            lastSubmittedValue = ((Atom.BasePlay)lastSubmittedValue).data;
            EditorOp.Resolve<FocusFieldsSystem>().AddFocusedGameobjects(toggleInput.gameObject,
                () => toggleInput.targetGraphic.color = Skin.SelectedItemBackgroundColor,
                () => toggleInput.targetGraphic.color = Skin.InputFieldNormalBackgroundColor);

            EditorOp.Resolve<FocusFieldsSystem>().AddFocusedGameobjects(optionsDropdown.gameObject,
            () => optionsDropdown.targetGraphic.color = Skin.SelectedItemBackgroundColor,
            () => optionsDropdown.targetGraphic.color = Skin.InputFieldNormalBackgroundColor);

        }

        protected override void OnUnbound()
        {
            EditorOp.Resolve<FocusFieldsSystem>().RemoveFocusedGameObjects(toggleInput.gameObject);
            EditorOp.Resolve<FocusFieldsSystem>().RemoveFocusedGameObjects(optionsDropdown.gameObject);
        }

        public override void Initialize()
        {
            base.Initialize();
            toggleInput.onValueChanged.AddListener(OnToggleValueChanged);
            optionsDropdown.onValueChanged.AddListener(OnDropdownValueChanged);
            LoadClips();
            ShowHideOptionsDropdown();
        }

        protected virtual void LoadClips()
        {
            optionsDropdown.options.Clear();
            foreach (string clipName in GetAllClipNames())
            {
                optionsDropdown.options.Add(new Dropdown.OptionData()
                {
                    text = clipName
                });
            }
        }

        protected virtual void ShowHideOptionsDropdown()
        {
            var isToggledOn = toggleInput.isOn;
            optionsDropdown.gameObject.SetActive(isToggledOn);
        }

        protected virtual void OnToggleValueChanged(bool _input)
        {
            OnToggleValueSubmitted(_input);
            var data = (Atom.BasePlay)Value;
            var play = (PlayFXData)lastSubmittedValue;
            if (data.data.canPlay != play.canPlay)
            {
                var message = $"{CommentKey} toggle changed to: {_input}";
                GenerateSnapshot(message);
            }
        }

        protected virtual void OnToggleValueSubmitted(bool _input)
        {
            LoadClips();
            var data = (Atom.BasePlay)Value;
            data.data.canPlay = _input;
            if (data.data.canPlay)
            {
                OnDropdownValueChanged(data.data.clipIndex);
            }
            ShowHideOptionsDropdown();
            if (Inspector) Inspector.RefreshDelayed();
        }

        protected virtual void OnDropdownValueChanged(int _input)
        {
            OnDropdownValueSubmitted(_input);
            var play = (PlayFXData)lastSubmittedValue;
            if (_input != play.clipIndex)
            {
                var message = $"{CommentKey} selection changed to: {_input}";
                GenerateSnapshot(message);
            }
        }

        private void GenerateSnapshot(string message)
        {
            var data = (Atom.BasePlay)Value;
            var play = (PlayFXData)lastSubmittedValue;
            Snapshots.FXModificationSnapshot.CreateSnapshot(
                data,
                DerivedType,
                play,
                data.data,
                message,
                UpdateData
                );
            lastSubmittedValue = data.data;
        }

        protected virtual void OnDropdownValueSubmitted(int index)
        {
            var data = (Atom.BasePlay)Value;
            data.data.clipIndex = index;
            data.data.clipName = GetClipNameByIndex(index);
        }

        protected virtual string[] GetAllClipNames()
        {
            return null;
        }

        protected virtual string GetClipNameByIndex(int index)
        {
            return string.Empty;
        }

        protected override void OnSkinChanged()
        {
            base.OnSkinChanged();
            toggleBackground.color = Skin.InputFieldNormalBackgroundColor;
            toggleInput.graphic.color = Skin.ToggleCheckmarkColor;
            var rightSideAnchorMin = new Vector2(Skin.LabelWidthPercentage, 0f);
            variableNameMask.rectTransform.anchorMin = rightSideAnchorMin;
            ((RectTransform)toggleInput.transform).anchorMin = rightSideAnchorMin;
            optionsDropdown.SetSkinDropDownField(Skin);
        }

        private void LoadData()
        {
            var data = (Atom.BasePlay)Value;
            if (data != null)
            {
                SetValue(data.data.canPlay);
                SetValue(data.data.clipIndex);
                if ((data.data.canPlay && !optionsDropdown.gameObject.activeSelf) ||
                    (!data.data.canPlay && optionsDropdown.gameObject.activeSelf))
                {
                    ShowHideOptionsDropdown();
                }
            }
        }

        private void SetValue(bool _input)
        {
            toggleInput.SetIsOnWithoutNotify(_input);
        }

        private void SetValue(int _input)
        {
            optionsDropdown.SetValueWithoutNotify(_input);
        }

        protected virtual void UpdateData() { }

        public override void SetInteractable(bool on , bool disableAlso=false)
        {
            base.SetInteractable(on, disableAlso);
            toggleInput.interactable = on;
            optionsDropdown.interactable = on;
        }
    }
}