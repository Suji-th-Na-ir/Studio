using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace Terra.Studio
{
    public class Icon : MonoBehaviour
    {
        [SerializeField] Image behaviourTypeImage;
        [SerializeField] Image backgoundImage;
        private ComponentIconsPreset iconsPreset;

        private RectTransform rectTransform;
        public RectTransform RectTransform
        {
            get
            {
                if (rectTransform == null)
                {
                    rectTransform = GetComponent<RectTransform>();
                }
                return rectTransform;
            }
        }

        public void Setup(Vector2 sizeDelta, string bgName, string iconName, Transform parent, bool raycast = false)
        {
            if (!iconsPreset)
                iconsPreset = EditorOp.Load<ComponentIconsPreset>("SOs/Component_Icon_SO");

            backgoundImage.sprite = iconsPreset.GetIcon(bgName);
            transform.SetParent(parent, false);
            behaviourTypeImage.sprite = iconsPreset.GetIcon(iconName);
            RectTransform.sizeDelta = sizeDelta;
            rectTransform.anchoredPosition=Vector2.zero;
            backgoundImage.raycastTarget = behaviourTypeImage.raycastTarget = raycast;
           

        }

        public void SetBckgroundImage(string bgName)
        {
            backgoundImage.sprite = iconsPreset.GetIcon(bgName);
        }

        public void SetIconImage(string Name)
        {
            behaviourTypeImage.sprite = iconsPreset.GetIcon(Name);
        }

        public void Hide()
        {
            backgoundImage.enabled = false;
            behaviourTypeImage.enabled = false;
        }

        public void Show()
        {
            backgoundImage.enabled = true;
            behaviourTypeImage.enabled = true;
        }

        public void SetBackgroundColor(Color color)
        {
            backgoundImage.color = color;
        }
    }
}
