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
            broadcastDropdown.onValueChanged.AddListener((val) => OnBroadcastDropdownValueChanged(val));
        }

        public override bool SupportsType(Type type)
        {
            return type == typeof(Atom.Broadcast);
        }

        protected override void OnBound(MemberInfo variable)
        {
            base.OnBound(variable);

            var val = (Atom.Broadcast)Value;

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
            
            AddDropDownOptionsInOrder(EditorOp.Resolve<BroadcastListenStringValidator>().BroadcastStrings);
            OnBroadcastDropdownValueChanged(broadcastDropdown.value);
        }

        private void AddDropDownOptionsInOrder(List<string>names)
        {
            broadcastDropdown.ClearOptions();
            List<string> options = new List<string>();
            if(names!=null)
            {
                for (int i = 0; i < names.Count; i++)
                {
                    options.Add(names[i]);
                }
            }

            options.Add("Custom");
            broadcastDropdown.AddOptions(options);
        }

        private void OnBroadcastValueChanged(string newValue)
        {
            if (newValue == null)
                return;

            EditorOp.Resolve<BroadcastListenStringValidator>().UpdateNewBroadcast(newValue);


            List<string> names = EditorOp.Resolve<BroadcastListenStringValidator>().BroadcastStrings;
            AddDropDownOptionsInOrder(names);

            for (int i = 0; i < broadcastDropdown.options.Count; i++)
            {
                if (broadcastDropdown.options[i].text == newValue)
                {
                    broadcastDropdown.value = i;
                    break;
                }
            }
            OnBroadcastDropdownValueChanged(broadcastDropdown.value);
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
                BroadcastFieldDrawer.SetInteractable(true, true);
            }
            else
            {
                val.broadcast = broadcastDropdown.options[value].text;
                BroadcastFieldDrawer.SetInteractable(false, true);
            }
            var newString = val.broadcast == null ? string.Empty : val.broadcast;
            var oldString = oldValue == null ? string.Empty : oldValue.ToString();
            onStringUpdated?.Invoke(newString, oldString);
        }
        protected override void OnSkinChanged()
        {
            base.OnSkinChanged();
            broadcastDropdown.SetSkinDropDownField(Skin);
        }
    }
}
