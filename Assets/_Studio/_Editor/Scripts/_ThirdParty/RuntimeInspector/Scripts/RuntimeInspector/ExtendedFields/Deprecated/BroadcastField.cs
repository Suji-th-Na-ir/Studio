using System;
using Terra.Studio;
using System.Reflection;
using UnityEngine.UI;
using UnityEngine;
using System.Collections.Generic;

namespace RuntimeInspectorNamespace
{
    public class BroadcastField : ExpandableInspectorField
    {
        [SerializeField] Dropdown broadcastDropdown;
        InspectorField BroadcastFieldDrawer;
        private bool didCheckForExpand;
        protected override int Length { get { return 1; } }
        public override void Initialize()
        {
            base.Initialize();
        }

        public override bool SupportsType(Type type)
        {
            return type == typeof(Atom.Broadcast);
        }

        protected override void OnBound(MemberInfo variable)
        {
            base.OnBound(variable);

            var val = (Atom.Broadcast)Value;
            broadcastDropdown.onValueChanged.AddListener((val) => OnBroadcastDropdownValueChanged(val));
            FieldInfo fInfo = Value.GetType().GetField(nameof(val.broadcast));
            var attribute = fInfo.GetAttribute<OnValueChangedAttribute>();
            if (attribute != null)
            {
                if (val != null)
                {
                    onStringUpdated = attribute.OnValueUpdated(val.behaviour);
                }
            }
        }

        protected override void OnUnbound()
        {
            base.OnUnbound();
            onStringUpdated = null;
        }

        public override void Refresh()
        {
            if (!didCheckForExpand && Value != null)
            {
                didCheckForExpand = true;
                IsExpanded = true;
            }
            base.Refresh();
        }

        protected override void ClearElements()
        {
            base.ClearElements();
            didCheckForExpand = false;
        }

        public override void GenerateElements()
        {
            var val = (Atom.Broadcast)Value;
            BroadcastFieldDrawer = CreateDrawerForField(nameof(val.broadcast));
            BroadcastFieldDrawer.OnStringValueSubmitted +=OnBroadcastValueChanged;
            BroadcastFieldDrawer.SetInteractable(false, true);

            AddDropDownOptionsInOrder(SystemOp.Resolve<CrossSceneDataHolder>().BroadcastStrings);
            SetBroadcastDropdownCurrentValue(val.broadcast, true);
        }

        private void AddDropDownOptionsInOrder(IEnumerable<string>names)
        {
            broadcastDropdown.ClearOptions();
            List<string> options = new List<string>();
            if(names!=null)
            {
                foreach (var item in names)
                {
                    options.Add(item);
                }
            }

            options.Add("Custom");
            broadcastDropdown.AddOptions(options);
        }

        private void OnBroadcastDropdownValueChanged(int value)
        {
            var val = (Atom.Broadcast)Value;
            var oldValue = val.broadcast;
            if (broadcastDropdown.options[value].text == "None")
            {
                val.broadcast = String.Empty;
                BroadcastFieldDrawer.SetInteractable(false, true);
            }
            else if (broadcastDropdown.options[value].text == "Custom")
            {
                val.broadcast = String.Empty;
                BroadcastFieldDrawer?.SetInteractable(true, true);
            }
            else
            {
                val.broadcast = broadcastDropdown.options[value].text;
                BroadcastFieldDrawer.SetInteractable(false, true);
            }
            var newString = val.broadcast == null ? string.Empty : val.broadcast;
            var oldString = oldValue == null ? string.Empty : oldValue.ToString();
            onStringUpdated?.Invoke(newString, oldString);
            OnValueUpdated?.Invoke(newString);
        }

        private void OnBroadcastValueChanged(string newValue)
        {
            if (newValue == null)
                return;

            SystemOp.Resolve<CrossSceneDataHolder>().UpdateNewBroadcast(newValue);

            IEnumerable<string> names = SystemOp.Resolve<CrossSceneDataHolder>().BroadcastStrings;
            AddDropDownOptionsInOrder(names);
            SetBroadcastDropdownCurrentValue(newValue);
            BroadcastFieldDrawer.SetInteractable(false, true);
        }

        private void SetBroadcastDropdownCurrentValue(string newValue, bool withoutnotify = false)
        {
            if (string.IsNullOrEmpty(newValue))
                newValue = "None";

            for (int i = 0; i < broadcastDropdown.options.Count; i++)
            {
                if (broadcastDropdown.options[i].text == newValue)
                {
                    if (withoutnotify)
                    {
                        broadcastDropdown.SetValueWithoutNotify(i);
                    }
                    else
                    {
                        broadcastDropdown.value = i;
                    }
                    break;
                }
            }
        }

        protected override void OnSkinChanged()
        {
            base.OnSkinChanged();
            broadcastDropdown.SetSkinDropDownField(Skin);
        }
    }
}
