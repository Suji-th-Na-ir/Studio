using System;
using UnityEngine;
using Terra.Studio;
using UnityEngine.UI;
using System.Reflection;

namespace RuntimeInspectorNamespace
{
    public class StringField : InspectorField
    {
        public enum Mode { OnValueChange = 0, OnSubmit = 1 };

#pragma warning disable 0649
        [SerializeField]
        protected BoundInputField input;
#pragma warning restore 0649

        public override bool UseSubmitButton
        {
            get { return useSubmitButton; }
            set
            {
                var rectT = input.BackingField.GetComponent<RectTransform>();
                useSubmitButton = value;
                if (useSubmitButton)
                {
                    rectT.offsetMax = new Vector2(-30, rectT.offsetMax.y);
                    SubmmitButton.gameObject.SetActive(true);
                    SubmmitButton.onClick.RemoveAllListeners();
                    SubmmitButton.onClick.AddListener(() => { OnStringValueSubmitted?.Invoke(Value.ToString()); });
                }
                else
                {
                    rectT.offsetMax = new Vector2(0, rectT.offsetMax.y);
                    SubmmitButton.gameObject.SetActive(false);
                    SubmmitButton.onClick.RemoveAllListeners();
                }
            }
        }

        private Mode m_setterMode = Mode.OnValueChange;
        public Mode SetterMode
        {
            get { return m_setterMode; }
            set
            {
                m_setterMode = value;
                input.CacheTextOnValueChange = m_setterMode == Mode.OnValueChange;
            }
        }

        private int lineCount = 1;
        protected override float HeightMultiplier { get { return lineCount; } }
        public override void Initialize()
        {
            base.Initialize();
            input.Initialize();
            input.OnValueChanged += OnValueChanged;
            input.OnValueSubmitted += OnValueSubmitted;
            input.DefaultEmptyValue = string.Empty;
        }

        public override void SetInteractable(bool on, bool disableAlso = false)
        {
            base.SetInteractable(on, disableAlso);
            input.BackingField.interactable = on;
            input.BackingField.textComponent.color = SkinUtils.GetInteractableColor(on);
        }

        public override bool SupportsType(Type type)
        {
            return type == typeof(string);
        }

        protected override void OnBound(MemberInfo variable)
        {
            base.OnBound(variable);

            int prevLineCount = lineCount;
            if (variable == null)
                lineCount = 1;
            else
            {
                MultilineAttribute multilineAttribute = variable.GetAttribute<MultilineAttribute>();
                if (multilineAttribute != null)
                    lineCount = Mathf.Max(1, multilineAttribute.lines);
                else if (variable.HasAttribute<TextAreaAttribute>())
                    lineCount = 3;
                else
                    lineCount = 1;
            }

            if (prevLineCount != lineCount)
            {
                input.BackingField.lineType = lineCount > 1 ? InputField.LineType.MultiLineNewline : InputField.LineType.SingleLine;
                input.BackingField.textComponent.alignment = lineCount > 1 ? TextAnchor.UpperLeft : TextAnchor.MiddleLeft;
                OnSkinChanged();
            }

            lastSubmittedValue = Value;
            EditorOp.Resolve<FocusFieldsSystem>().AddFocusedGameobjects(input.gameObject,
                   () => input.BackingField.targetGraphic.color = Skin.SelectedItemBackgroundColor,
                   () => input.BackingField.targetGraphic.color = Skin.InputFieldNormalBackgroundColor);

        }

        protected override void OnUnbound()
        {
            base.OnUnbound();
            SetterMode = Mode.OnValueChange;
            EditorOp.Resolve<FocusFieldsSystem>().RemoveFocusedGameObjects(input.gameObject);
        }

        protected virtual bool OnValueChanged(BoundInputField source, string input)
        {
            var oldValue = Value;
            if (m_setterMode == Mode.OnValueChange)
            {
                Value = input;
            }

            OnValueUpdated?.Invoke(input);
            return true;
        }

        protected virtual bool OnValueSubmitted(BoundInputField source, string input)
        {
            if (m_setterMode == Mode.OnSubmit)
                Value = input;

            if (Value != lastSubmittedValue && useSubmitButton == false)
            {
                EditorOp.Resolve<IURCommand>().Record(
                    lastSubmittedValue, input,
                    $"String changed to: {input}",
                    (value) =>
                    {
                        OnValueChanged(source, (string)value);
                        lastSubmittedValue = value;
                    });
            }

            OnStringValueSubmitted?.Invoke(Value.ToString());
            
            lastSubmittedValue = Value;
            Inspector.RefreshDelayed();
            return true;
        }

        protected override void OnSkinChanged()
        {
            base.OnSkinChanged();
            input.Skin = Skin;

            Vector2 rightSideAnchorMin = new Vector2(Skin.LabelWidthPercentage, 0f);
            variableNameMask.rectTransform.anchorMin = rightSideAnchorMin;
            ((RectTransform)input.transform).anchorMin = rightSideAnchorMin;
        }

        public override void Refresh()
        {
            base.Refresh();

            if (Value == null)
                input.Text = string.Empty;
            else
                input.Text = (string)Value;
        }
    }
}