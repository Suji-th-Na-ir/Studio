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

        protected virtual void OnToggleValueChanged(bool _input)
        {
            OnToggleValueSubmitted(_input);
            var data = (Atom.BasePlay)Value;
            var play = (PlayFXData)lastSubmittedValue;
            if (data.data.CanPlay != play.CanPlay)
            {
                EditorOp.Resolve<IURCommand>().Record(
                    lastSubmittedValue, data.data,
                    $"{CommentKey} toggle changed to: {_input}",
                    (value) =>
                    {
                        var structData = (PlayFXData)value;
                        Debug.Log($"Toggle is {structData.CanPlay}");
                        SetValue(structData.CanPlay);
                        OnToggleValueSubmitted(structData.CanPlay);
                        lastSubmittedValue = structData;
                    });
                lastSubmittedValue = data.data;
            }
        }

        protected virtual void OnToggleValueSubmitted(bool _input)
        {
            LoadClips();
            var data = (Atom.BasePlay)Value;
            data.data.CanPlay = _input;
            if (data.data.CanPlay)
            {
                OnDropdownValueChanged(data.data.ClipIndex);
            }
            ShowHideOptionsDropdown();
            if (Inspector) Inspector.RefreshDelayed();
        }

        protected virtual void OnDropdownValueChanged(int _input)
        {
            OnDropdownValueSubmitted(_input);
            var data = (Atom.BasePlay)Value;
            var play = (PlayFXData)lastSubmittedValue;
            if (_input != play.ClipIndex)
            {
                EditorOp.Resolve<IURCommand>().Record(
                    lastSubmittedValue, data.data,
                    $"{CommentKey} selection changed to: {_input}",
                    (value) =>
                    {
                        OnDropdownValueSubmitted(((PlayFXData)value).ClipIndex);
                        lastSubmittedValue = value;
                    });
                lastSubmittedValue = data.data;
            }
        }

        protected virtual void OnDropdownValueSubmitted(int index)
        {
            var data = (Atom.BasePlay)Value;
            data.data.ClipIndex = index;
            data.data.ClipName = GetClipNameByIndex(index);
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
                SetValue(data.data.CanPlay);
                SetValue(data.data.ClipIndex);
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
    }
}