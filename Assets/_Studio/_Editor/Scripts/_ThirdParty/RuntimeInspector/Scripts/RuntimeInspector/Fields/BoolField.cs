using System;
using System.Reflection;
using Terra.Studio;
using UnityEngine;
using UnityEngine.UI;

namespace RuntimeInspectorNamespace
{
    public class BoolField : InspectorField
    {
#pragma warning disable 0649
        [SerializeField]
        private Image toggleBackground;

        [SerializeField]
        private Toggle input;
#pragma warning restore 0649

        public override void Initialize()
        {
            base.Initialize();
            input.onValueChanged.AddListener(OnValueChanged);
        }

        public override bool SupportsType(Type type)
        {
            return type == typeof(bool);
        }

        protected override void OnBound(MemberInfo variable)
        {
            base.OnBound(variable);
            EditorOp.Resolve<FocusFieldsSystem>().AddFocusedGameobjects(input.gameObject,
                () => input.targetGraphic.color = Skin.SelectedItemBackgroundColor,
                () => input.targetGraphic.color = Skin.InputFieldNormalBackgroundColor);
        }

        protected override void OnUnbound()
        {
            base.OnUnbound();
            EditorOp.Resolve<FocusFieldsSystem>().RemoveFocusedGameObjects(input.gameObject);
        }

        private void OnValueChanged(bool input)
        {
            if (input != (bool)lastSubmittedValue && shouldPopulateIntoUndoRedoStack)
            {
                EditorOp.Resolve<IURCommand>().Record(
                    lastSubmittedValue, input,
                    $"Bool changed to: {input}",
                    (value) =>
                    {
                        OnValueSubmitted((bool)value);
                        lastSubmittedValue = value;
                    });
            }
            lastSubmittedValue = input;
            OnValueSubmitted(input);
        }

        private void OnValueSubmitted(bool input)
        {
            Value = input;
            Inspector.RefreshDelayed();
            OnValueUpdated?.Invoke(input);
        }

        protected override void OnSkinChanged()
        {
            base.OnSkinChanged();
            toggleBackground.color = Skin.InputFieldNormalBackgroundColor;
            input.graphic.color = Skin.ToggleCheckmarkColor;
            var rightSideAnchorMin = new Vector2(Skin.LabelWidthPercentage, 0f);
            variableNameMask.rectTransform.anchorMin = rightSideAnchorMin;
            ((RectTransform)input.transform).anchorMin = rightSideAnchorMin;
        }

        public override void Refresh()
        {
            base.Refresh();
            input.SetIsOnWithoutNotify((bool)Value);
        }

        public override void SetInteractable(bool on , bool disableAlso=false)
        {
            base.SetInteractable(on, disableAlso);
        }
    }
}