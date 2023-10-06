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
            EditorOp.Resolve<FocusFieldsSystem>().AddFocusedGameobjects(input.BackingField.gameObject);
        }

        protected override void OnUnbound()
        {
            base.OnUnbound();
            SetterMode = Mode.OnValueChange;
            EditorOp.Resolve<FocusFieldsSystem>().RemoveFocusedGameObjects(input.BackingField.gameObject);
        }

        protected virtual bool OnValueChanged(BoundInputField source, string input)
        {
            var oldValue = Value;
            if (m_setterMode == Mode.OnValueChange)
            {
                Value = input;
            }
            var newString = Value == null ? string.Empty : Value.ToString();
            var oldString = oldValue == null ? string.Empty : oldValue.ToString();
            onStringUpdated?.Invoke(newString, oldString);
            return true;
        }

        protected virtual bool OnValueSubmitted(BoundInputField source, string input)
        {
            if (m_setterMode == Mode.OnSubmit)
                Value = input;

            if (Value != lastSubmittedValue)
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