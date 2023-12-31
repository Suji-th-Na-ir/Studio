﻿using System;
using System.Collections.Generic;
using System.Reflection;
using Terra.Studio;
using UnityEngine;
using UnityEngine.UI;
using PlayShifu.Terra;
namespace RuntimeInspectorNamespace
{
    public class EnumField : InspectorField
    {
#pragma warning disable 0649
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

        [SerializeField]
        private Dropdown input;
#pragma warning restore 0649

		private static readonly Dictionary<Type, List<string>> enumNames = new Dictionary<Type, List<string>>();
		private static readonly Dictionary<Type, List<object>> enumValues = new Dictionary<Type, List<object>>();

		private List<string> currEnumNames;
		private List<object> currEnumValues;

		public override void Initialize()
		{
			base.Initialize();
			input.onValueChanged.AddListener( OnValueChanged );

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
			// On new Input System, scroll sensitivity is much higher than legacy Input system
			templateRoot.GetComponent<ScrollRect>().scrollSensitivity *= 0.25f;
#endif

#if UNITY_WEBGL && !UNITY_EDITOR
			ScrollRect[] a= this.GetComponentsInChildren<ScrollRect> ();
            foreach (var item in a) {
				item.scrollSensitivity *= 0.05f;
			}
#endif
		}

		public override bool SupportsType( Type type )
		{
#if UNITY_EDITOR || !NETFX_CORE
            return type.IsEnum;
#else
			return type.GetTypeInfo().IsEnum;
#endif
        }

        protected override void OnBound(MemberInfo variable)
        {
            base.OnBound(variable);

            if (!enumNames.TryGetValue(BoundVariableType, out currEnumNames) || !enumValues.TryGetValue(BoundVariableType, out currEnumValues))
            {
                string[] names = Helper.GetEnumWithAliasNames(BoundVariableType).ToArray();
                Array values = Enum.GetValues(BoundVariableType);

                currEnumNames = new List<string>(names.Length);
                currEnumValues = new List<object>(names.Length);

                for (int i = 0; i < names.Length; i++)
                {
                    currEnumNames.Add(names[i]);
                    currEnumValues.Add(values.GetValue(i));
                }

                enumNames[BoundVariableType] = currEnumNames;
                enumValues[BoundVariableType] = currEnumValues;
            }

            input.ClearOptions();
            input.AddOptions(currEnumNames);
            lastSubmittedValue = currEnumNames.IndexOf($"{lastSubmittedValue}");
            EditorOp.Resolve<FocusFieldsSystem>().AddFocusedGameobjects(input.gameObject,
            () => input.targetGraphic.color = Skin.SelectedItemBackgroundColor,
            () => input.targetGraphic.color = Skin.InputFieldNormalBackgroundColor);
         
        }

        protected override void OnUnbound()
        {
            base.OnUnbound();
            EditorOp.Resolve<FocusFieldsSystem>().RemoveFocusedGameObjects(input.gameObject);
        }


        protected override void OnInspectorChanged()
        {
            base.OnInspectorChanged();
            OnTransformParentChanged();
        }

        private void OnTransformParentChanged()
        {
            if (Inspector && Skin)
            {
                // Dropdown's list should be able to expand as much as possible when necessary
                Vector2 templateRootSizeDelta = templateRoot.sizeDelta;
                templateRootSizeDelta.y = (((RectTransform)Inspector.Canvas.transform).rect.height - Skin.LineHeight) * 0.5f;
                templateRoot.sizeDelta = templateRootSizeDelta;
            }
        }

        private void OnValueChanged(int input)
        {
            if (input != (int)lastSubmittedValue && shouldPopulateIntoUndoRedoStack)
            {
                EditorOp.Resolve<IURCommand>().Record(
                    lastSubmittedValue, input,
                    $"Enum changed to: {currEnumNames[input]}",
                    (value) =>
                    {
                        OnValueSubmitted((int)value);
                        lastSubmittedValue = value;
                    });
            }
            OnValueSubmitted(input);
            OnValueUpdated?.Invoke(Value);
        }

        private void OnValueSubmitted(int input)
        {
            Value = currEnumValues[input];
            lastSubmittedValue = input;
            Inspector.RefreshDelayed();
        }

        protected override void OnSkinChanged()
        {
            base.OnSkinChanged();
            OnTransformParentChanged();

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
            Vector2 dropdownTextSizeDelta = input.captionText.rectTransform.sizeDelta;
            dropdownTextSizeDelta.x -= templateCheckmarkSize - dropdownArrow.rectTransform.sizeDelta.x;
            input.captionText.rectTransform.sizeDelta = dropdownTextSizeDelta;
            dropdownArrow.rectTransform.sizeDelta = new Vector2(templateCheckmarkSize, templateCheckmarkSize);

            background.color = Skin.InputFieldNormalBackgroundColor;
            dropdownArrow.color = Skin.TextColor.Tint(0.1f);

            input.captionText.SetSkinInputFieldText(Skin);
            templateText.SetSkinInputFieldText(Skin);

            templateBackground.color = Skin.DropDownItemColor;
            templateCheckmark.color = Skin.ToggleCheckmarkColor;

            Vector2 rightSideAnchorMin = new Vector2(Skin.LabelWidthPercentage, 0f);
            variableNameMask.rectTransform.anchorMin = rightSideAnchorMin;
            ((RectTransform)input.transform).anchorMin = rightSideAnchorMin;
            variableNameText.fontSize = Skin.HeadingFontSize;
        }

        public override void Refresh()
        {
            base.Refresh();

            int valueIndex = currEnumValues.IndexOf(Value);
            if (valueIndex != -1)
                input.value = valueIndex;
        }

        public override void SetInteractable(bool on , bool disableAlso=false)
        {
            base.SetInteractable(on, disableAlso);
            input.interactable = on;
        }

        public override void InvokeUpdateDropdown(List<string> names)
        {
            base.InvokeUpdateDropdown(names);
            var lastValue = currEnumNames[input.value];
            currEnumNames = new List<string>(names.Count);
            currEnumValues = new List<object>(names.Count);

            for (int i = 0; i < names.Count; i++)
            {
                currEnumNames.Add(names[i]);
                currEnumValues.Add(Enum.Parse(BoundVariableType, names[i]));
            }
            enumNames[BoundVariableType] = currEnumNames;
            enumValues[BoundVariableType] = currEnumValues;
            
            input.ClearOptions();
            input.AddOptions(names);
            if (names.Contains(lastValue))
            {
                input.value = currEnumNames.IndexOf(lastValue);
            }
            else
            {
                OnValueChanged(input.value);
            }
        }
    }
}