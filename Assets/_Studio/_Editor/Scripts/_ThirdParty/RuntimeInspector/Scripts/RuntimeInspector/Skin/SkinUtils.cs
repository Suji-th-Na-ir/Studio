using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PlayShifu.Terra;
using UnityEngine.InputSystem;

namespace RuntimeInspectorNamespace
{
    public static class SkinUtils
    {
        public static void SetSkinText(this Text text, UISkin skin)
        {
            text.color = skin.TextColor;
            text.font = skin.Font;
            text.fontSize = skin.FontSize;
        }

        public static void SetSkinInputFieldText(this Text text, UISkin skin)
        {
            text.color = skin.InputFieldTextColor;
            text.font = skin.Font;
            text.fontSize = skin.FontSize;
        }

        public static void SetSkinDropDownField(this Dropdown input, UISkin Skin)
        {
            var dropdownArrow = Helper.FindDeepChild(input.transform, "Arrow").GetComponent<Image>();
            var templateContentTransform = Helper.FindDeepChild(input.transform, "Content").GetComponent<RectTransform>();
            var templateItemTransform = Helper.FindDeepChild(input.transform, "Item").GetComponent<RectTransform>();
            var templateText = input.itemText;
            var background = input.GetComponent<Image>();
            var templateCheckmark = Helper.FindDeepChild(input.transform, "Item Checkmark").GetComponent<Image>();
            var templateBackground = input.template.GetComponent<Image>();
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
        }

        public static void SetSkinButtonText(this Text text, UISkin skin)
        {
            text.color = skin.ButtonTextColor;
            text.font = skin.Font;
            text.fontSize = skin.FontSize;
        }

        public static void SetSkinButton(this Button button, UISkin skin)
        {
            button.targetGraphic.color = skin.ButtonBackgroundColor;
            button.GetComponentInChildren<Text>().SetSkinButtonText(skin);
        }

        public static void SetWidth(this LayoutElement layoutElement, float width)
        {
            layoutElement.minWidth = width;
            layoutElement.preferredWidth = width;
        }

        public static void SetHeight(this LayoutElement layoutElement, float height)
        {
            layoutElement.minHeight = height;
            layoutElement.preferredHeight = height;
        }

        public static void SetAnchorMinMaxInputField(this RectTransform inputField, RectTransform label, Vector2 anchorMin, Vector2 anchorMax)
        {
            inputField.anchorMin = anchorMin;
            inputField.anchorMax = anchorMax;
            label.anchorMin = anchorMin;
            label.anchorMax = new Vector2(anchorMin.x, anchorMax.y);
        }

        public static void SetupToggeleSkin(this Toggle toggle, UISkin Skin)
        {
            if (!toggle)
                return;
            toggle.transform.GetChild(0).GetComponent<Image>().color = Skin.InputFieldNormalBackgroundColor;
            // toggle.graphic.color = Skin.ToggleCheckmarkColor;
            Vector2 rightSideAnchorMin = new Vector2(Skin.LabelWidthPercentage, 0f);
            Text label = toggle.GetComponentInChildren<Text>();
            if (label != null) label.rectTransform.anchorMin = rightSideAnchorMin;
        }

        public static void SetupBoundInputFieldSkin(this BoundInputField inputField, UISkin Skin)
        {
            inputField.Skin = Skin;
            SetupInputFieldSkin(inputField.BackingField, Skin);
        }

        public static void SetupInputFieldSkin(this InputField inputField, UISkin Skin)
        {
            if (!inputField)
                return;
            inputField.textComponent.color = Skin.InputFieldTextColor;
            //  inputFieldBackground.color = Skin.InputFieldNormalBackgroundColor;

            Text placeholder = inputField.placeholder as Text;
            if (inputField.targetGraphic)
            {
                inputField.targetGraphic.color = Skin.InputFieldNormalBackgroundColor;
            }
            if (placeholder != null)
            {
                float placeholderAlpha = placeholder.color.a;
                placeholder.color = Skin.InputFieldTextColor;

                Color placeholderColor = placeholder.color;
                placeholderColor.a = placeholderAlpha;
                placeholder.color = placeholderColor;
            }
        }

        public static Color GetInteractableColor(bool isInteractable)
        {
            if (isInteractable)
            {
                return Color.white;
            }
            else
            {
                var color = Helper.GetColorFromHex("#C8C8C8");
                color.a = 0.5f;
                return color;
            }
           
        }

        public static void SetInteractableButtonText(this Button button, bool isInteractable)
        {
            var text = button.GetComponentInChildren<Text>();
            if (!text)
                return;
            text.color = GetInteractableColor(isInteractable);
        }
    }
}