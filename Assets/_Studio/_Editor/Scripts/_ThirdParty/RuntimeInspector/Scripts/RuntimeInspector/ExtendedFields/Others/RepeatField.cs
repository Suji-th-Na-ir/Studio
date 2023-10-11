using System;
using UnityEngine;
using Terra.Studio;
using UnityEngine.UI;
using PlayShifu.Terra;
using System.Reflection;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine.Windows;
using RTG;

namespace RuntimeInspectorNamespace
{
    public class RepeatField : InspectorField
    {
        [SerializeField] Toggle repeatForeverToggle;
        [SerializeField] BoundInputField repeatField;
        [SerializeField] BoundInputField pauseForField;
        [SerializeField] Dropdown repeatType;

        [SerializeField] Text repeatForeverToggleLabel;
        [SerializeField] Text repeatFieldLabel;
        [SerializeField] Text pauseForLabel;
        [SerializeField] Text repeatTypeLabel;

        [SerializeField] GameObject pauseForFieldDrawer;
        [SerializeField] GameObject repeatTypeFieldDrawer;

        private float elementheight;
        public override void Initialize()
        {
            base.Initialize();
            repeatField.Initialize();
            pauseForField.Initialize();
            
            repeatForeverToggle.onValueChanged.AddListener(OnRepeatForeverValueChanged);
            repeatField.OnValueChanged += OnRepeatValueChanged;
            pauseForField.OnValueChanged += OnPauseForValueChanged;

            elementheight = pauseForFieldDrawer.GetComponent<RectTransform>().rect.height+5;
        }

        private bool OnPauseForValueChanged(BoundInputField source, string input)
        {
            if (int.TryParse(input, out int result))
            {
                if (source == pauseForField)
                {
                    var val = (Atom.Repeat)Value;
                    val.pauseFor = result;
                    Refresh();
                    return true;
                }
            }
            return false;
        }

        private bool OnRepeatValueChanged(BoundInputField source, string input)
        {
            if (int.TryParse(input, out int result))
            {
                if (source == repeatField)
                {
                    pauseForFieldDrawer.SetActive(result > 1);
                    repeatTypeFieldDrawer.SetActive(result > 1);
                }
                var val = (Atom.Repeat)Value;
                val.repeat = result;
                Refresh();
                return true;
              
            }
            return false;
        }

        private void OnRepeatForeverValueChanged(bool isOn)
        {
            var val = (Atom.Repeat)Value;
            if (isOn)
            {
                repeatField.BackingField.text = $"{0}";
                repeatField.BackingField.interactable = false;
                val.repeat = int.MaxValue;
                pauseForField.BackingField.text = $"{0}";
            }
            else
            {
                repeatField.BackingField.text = string.Empty;
                repeatField.BackingField.interactable = true;
                val.repeat = 0;
                Debug.Log(val.repeat);
            }
            OnRepeatValueChanged(repeatField, val.repeat.ToString());
            Refresh();
        }

        public override bool SupportsType(Type type)
        {
            return type == typeof(Atom.Repeat);
        }

        protected override void OnBound(MemberInfo variable)
        {
            base.OnBound(variable);
            var val = (Atom.Repeat)Value;
            if (val != null)
            {
                repeatForeverToggle.SetIsOnWithoutNotify(val.repeat == int.MaxValue);  
                if (!repeatForeverToggle.isOn)
                {
                    repeatField.BackingField.SetTextWithoutNotify(val.repeat.ToString());
                    OnRepeatValueChanged(repeatField, val.repeat.ToString());
                }
                else
                {
                    OnRepeatForeverValueChanged(repeatForeverToggle.isOn);
                }
            }
            else
            {
                Debug.Log("Bounding Value is not of type Repeat");
            }
        }

        protected virtual bool OnValueChanged(string input)
        {
            if (int.TryParse(input, NumberStyles.Integer, RuntimeInspectorUtils.numberFormat, out int value))
            {
                var val = (Atom.Repeat)Value;
                val.repeat = value;
                Value = val;
                return true;
            }
            Inspector.RefreshDelayed();
            return false;
        }

        protected override void OnSkinChanged()
        {
            base.OnSkinChanged();
           
            repeatField.SetupBoundInputFieldSkin(Skin);
            pauseForField.SetupBoundInputFieldSkin(Skin);
            repeatType.SetSkinDropDownField(Skin);
            repeatForeverToggle.SetupToggeleSkin(Skin);
            repeatFieldLabel.SetSkinText(Skin);
            pauseForLabel.SetSkinText(Skin);
            repeatForeverToggleLabel.SetSkinText(Skin);
            repeatTypeLabel.SetSkinText(Skin);

        }

        public override void Refresh()
        {
            base.Refresh();

            var height = elementheight * 2f +
                (pauseForFieldDrawer.activeSelf ? elementheight : 0) +
                (repeatTypeFieldDrawer.activeSelf ? elementheight : 0);

            layoutElement.minHeight = height;
        }
    }
}
