using System;
using UnityEngine;
using Terra.Studio;
using UnityEngine.UI;
using System.Reflection;
using System.Globalization;
using System.Linq;
using System.Collections.Generic;

namespace RuntimeInspectorNamespace
{
    public class RepeatField : InspectorField
    {
        [SerializeField] Toggle repeatForeverToggle;
        [SerializeField] BoundInputField repeatField;
        [SerializeField] BoundInputField pauseForField;
        [SerializeField] Dropdown repeatTypeDropdown;
        [SerializeField] Dropdown broadcastTypeDropdown;
        [SerializeField] BoundInputField broadcastField;

        [Header("Labels"),Space(20)]
        [SerializeField] Text repeatForeverToggleLabel;
        [SerializeField] Text repeatFieldLabel;
        [SerializeField] Text pauseForLabel;
        [SerializeField] Text repeatTypeLabel;
        [SerializeField] Text broadcastTypeLabel;
        [SerializeField] Text broadcastLabel;

        [Header("Drawer"), Space(20)]
        [SerializeField] GameObject pauseForFieldDrawer;
        [SerializeField] GameObject repeatTypeFieldDrawer;
        [SerializeField] GameObject broadcastTypeFieldDrawer;
        [SerializeField] GameObject broadcastFieldDrawer;

        private Toggle[] toggles;


        private float elementheight;
        public override void Initialize()
        {
            base.Initialize();
            repeatField.Initialize();
            pauseForField.Initialize();
            broadcastField.Initialize();
            repeatTypeDropdown.ClearOptions();
            repeatTypeDropdown.AddOptions(Enum.GetNames(typeof(RepeatDirectionType)).ToList());
            broadcastTypeDropdown.ClearOptions();
            broadcastTypeDropdown.AddOptions(Enum.GetNames(typeof(BroadcastAt)).ToList());
            
            repeatForeverToggle.onValueChanged.AddListener(OnRepeatForeverValueChanged);
            repeatField.OnValueChanged += OnRepeatValueChanged;
            pauseForField.OnValueChanged += OnPauseForValueChanged;
            repeatTypeDropdown.onValueChanged.AddListener(OnRepeatTypeValueChanged);
            broadcastField.OnValueChanged += OnBroadcastValueChanged;
            broadcastTypeDropdown.onValueChanged.AddListener(OnBroadcastTypeValueChanged);
            toggles = broadcastTypeDropdown.gameObject.GetComponentsInChildren<Toggle>(true);
            elementheight = pauseForFieldDrawer.GetComponent<RectTransform>().rect.height+5;
        }

        private void OnBroadcastTypeValueChanged(int input)
        {
            var val = (Atom.Repeat)Value;
            if (Enum.IsDefined(typeof(BroadcastAt), input))
            {
                var valueText = broadcastTypeDropdown.options[input].text;
                BroadcastAt enumValue = (BroadcastAt)Enum.Parse(typeof(BroadcastAt), valueText);
                val.broadcastAt = enumValue;
            }
        }

        private bool OnBroadcastValueChanged(BoundInputField source, string input)
        {
            var val = (Atom.Repeat)Value;
            if (source == broadcastField)
            {
                var oldValue = val.broadcast;
                val.broadcast = input;
                onStringUpdated?.Invoke(val.broadcast,oldValue);
                return true;
            }
            return false;
        }

        private void OnRepeatTypeValueChanged(int input)
        {
            var val = (Atom.Repeat)Value;
            if (Enum.IsDefined(typeof(RepeatDirectionType), input))
            {
                val.repeatType = input;
            }
        }

        private bool OnPauseForValueChanged(BoundInputField source, string input)
        {
            if (int.TryParse(input, out int result))
            {
                if (source == pauseForField)
                {
                    var val = (Atom.Repeat)Value;
                    val.pauseFor = result;
                    UpdatebroadcastTypeDropDown();
                    Refresh();
                }
                return true;
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
                    var val = (Atom.Repeat)Value;
                    val.repeat = result;
                }
                UpdatebroadcastTypeDropDown();
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
                repeatField.BackingField.SetTextWithoutNotify($"{0}");
                repeatField.BackingField.interactable = false;
                val.repeat = int.MaxValue;
            }
            else
            {
                repeatField.BackingField.SetTextWithoutNotify($"{0}");
                repeatField.BackingField.interactable = true;
                pauseForField.BackingField.text=$"{0}";
                OnPauseForValueChanged(pauseForField, $"{0}");
                val.repeat = 0;
            }
            OnRepeatValueChanged(repeatField, val.repeat.ToString());
            UpdatebroadcastTypeDropDown();
            Refresh();
        }

        private void UpdatebroadcastTypeDropDown()
        {
            List<string> ignoreNames = new List<string>();
            var val = (Atom.Repeat)Value;

            if(val.repeat==int.MaxValue)
            {
                ignoreNames.Add(BroadcastAt.End.ToString());
                if(val.pauseFor==0)
                {
                    ignoreNames.Add(BroadcastAt.AtEveryInterval.ToString());
                }    
            }
            if(val.repeat<2)
            {
                ignoreNames.Add(BroadcastAt.AtEveryInterval.ToString());
            }
           
            broadcastTypeDropdown.ClearOptions();
            broadcastTypeDropdown.AddOptions(Enum.GetNames(typeof(BroadcastAt)).Where(name => !ignoreNames.Contains(name)).ToList());
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
                FieldInfo fieldInfo = val.GetType().GetField(nameof(val.broadcast));
                var attribute = fieldInfo.GetAttribute<OnValueChangedAttribute>();
                if (attribute!=null)
                {
                    onStringUpdated = attribute.OnValueUpdated(val.behaviour);
                }
                repeatForeverToggle.SetIsOnWithoutNotify(val.repeat == int.MaxValue);
                repeatTypeDropdown.SetValueWithoutNotify(val.repeatType);
                broadcastTypeDropdown.SetValueWithoutNotify((int)val.broadcastAt);
                pauseForField.BackingField.SetTextWithoutNotify(val.pauseFor.ToString());

                broadcastField.BackingField.SetTextWithoutNotify(string.IsNullOrEmpty(val.broadcast) ? "" : val.broadcast);
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
            repeatTypeDropdown.SetSkinDropDownField(Skin);
            repeatForeverToggle.SetupToggeleSkin(Skin);
            repeatFieldLabel.SetSkinText(Skin);
            pauseForLabel.SetSkinText(Skin);
            repeatForeverToggleLabel.SetSkinText(Skin);
            repeatTypeLabel.SetSkinText(Skin);
            broadcastTypeDropdown.SetSkinDropDownField(Skin);
            broadcastField.SetupBoundInputFieldSkin(Skin);
            broadcastLabel.SetSkinText(Skin);
            broadcastTypeLabel.SetSkinText(Skin);
        }

        public override void Refresh()
        {
            base.Refresh();

            var height = elementheight * 2f +
                (pauseForFieldDrawer.activeSelf ? elementheight : 0) +
                (repeatTypeFieldDrawer.activeSelf ? elementheight : 0) +
                (broadcastFieldDrawer.activeSelf ? elementheight : 0) +
                (broadcastTypeFieldDrawer.activeSelf ? elementheight : 0)+5f;

            layoutElement.minHeight = height;
        }
    }
}
