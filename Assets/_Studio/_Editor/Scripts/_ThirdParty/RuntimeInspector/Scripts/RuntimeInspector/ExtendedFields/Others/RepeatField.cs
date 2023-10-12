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
    public class RepeatField : ExpandableInspectorField
    {
        InspectorField repeatForFieldDrawer;
        InspectorField pauseForFieldDrawer;
        InspectorField repeatTypeFieldDrawer;
        InspectorField broadcastTypeFieldDrawer;
        InspectorField broadcastFieldDrawer;
        InspectorField repeatFoeverFieldDrawer;

        private FieldInfo[] allFields;
        protected FieldInfo[] FieldInfos
        {
            get
            {
                if (allFields == null)
                {
                    Type type = typeof(Atom.Repeat);
                    var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    allFields = fields.Where(field => !field.IsDefined(typeof(HideInInspector), false)).ToArray();
                }
                return allFields;
            }
        }

        protected override int Length { get { return 6; } }
        private bool didCheckForExpand;

        public override void Initialize()
        {
            base.Initialize();
        }

        private void ToggleBroadcastField()
        {
            var val = (Atom.Repeat)Value;
            if (val.broadcastAt == BroadcastAt.Never)
            { 
                broadcastFieldDrawer.gameObject.SetActive(false);
                broadcastFieldDrawer.InvokeChangeValueExternal(string.Empty);
            }
            else
            {
                broadcastFieldDrawer.gameObject.SetActive(true);
            }
        }

        private void OnBroadcastTypeValueChanged(object value)
        {
            ToggleBroadcastField();
        }


        private void OnPauseForValueChanged(object value)
        {
            UpdatebroadcastTypeDropDown();
        }

        private void OnRepeatValueChanged(object value)
        {
            var val = (Atom.Repeat)Value;

            if (val.repeat < 1)
                val.repeat = 1;

            if (val.repeat < 2)
            {
                ToggelPauseForAndRepeat(false);
            }
            else
            {
                ToggelPauseForAndRepeat(true);
            }
            UpdatebroadcastTypeDropDown();
        }

        private void ToggelPauseForAndRepeat(bool on)
        {
            pauseForFieldDrawer.gameObject.SetActive(on);
            repeatTypeFieldDrawer.gameObject.SetActive(on);
        }

        private void OnRepeatForeverValueChanged(object value)
        {
            var val = (Atom.Repeat)Value;
            var isOn = (bool)value;
            if (isOn)
            {
                repeatForFieldDrawer.SetInteractable(false);
                val.repeat = 1;
                ToggelPauseForAndRepeat(true);  
            }
            else
            {
                repeatForFieldDrawer.SetInteractable(true);
                OnRepeatValueChanged(val.repeat);
            }
            UpdatebroadcastTypeDropDown();
        }

        private void UpdatebroadcastTypeDropDown()
        {
            List<string> ignoreNames = new List<string>();
            var val = (Atom.Repeat)Value;

            if (val.repeatForever)
            {
                ignoreNames.Add(BroadcastAt.End.ToString());
                if (val.pauseFor == 0)
                {
                    ignoreNames.Add(BroadcastAt.AtEveryPause.ToString());
                }
            }
            else if (val.repeat < 2)
            {
                ignoreNames.Add(BroadcastAt.AtEveryPause.ToString());
            }
           
            broadcastTypeFieldDrawer.InvokeUpdateDropdown(Enum.GetNames(typeof(BroadcastAt)).Where(name => !ignoreNames.Contains(name)).ToList());
            ToggleBroadcastField();
        }

        public override bool SupportsType(Type type)
        {
            return type == typeof(Atom.Repeat);
        }

        protected override void OnBound(MemberInfo variable)
        {
            base.OnBound(variable);
            var val = (Atom.Repeat)Value;
        }

        protected override void OnUnbound()
        {
            base.OnUnbound();
            repeatForFieldDrawer.OnValueUpdated -= OnRepeatValueChanged;
            repeatFoeverFieldDrawer.OnValueUpdated -= OnRepeatForeverValueChanged;
            pauseForFieldDrawer.OnValueUpdated -= OnPauseForValueChanged;
            broadcastTypeFieldDrawer.OnValueUpdated -= OnBroadcastTypeValueChanged;

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

        protected override void GenerateElements()
        {
            var val = (Atom.Repeat)Value;
            repeatForFieldDrawer = CreateDrawerForField(nameof(val.repeat));
            repeatForFieldDrawer.OnValueUpdated += OnRepeatValueChanged;

            repeatFoeverFieldDrawer = CreateDrawerForField(nameof(val.repeatForever));
            repeatFoeverFieldDrawer.OnValueUpdated += OnRepeatForeverValueChanged; 

            pauseForFieldDrawer = CreateDrawerForField(nameof(val.pauseFor));
            pauseForFieldDrawer.OnValueUpdated += OnPauseForValueChanged;

            repeatTypeFieldDrawer = CreateDrawerForField(nameof(val.repeatType));

            broadcastTypeFieldDrawer = CreateDrawerForField(nameof(val.broadcastAt));
            broadcastTypeFieldDrawer.OnValueUpdated += OnBroadcastTypeValueChanged;
            

            broadcastFieldDrawer = CreateDrawerForField(nameof(val.broadcast));
           
            OnRepeatValueChanged(val.repeat);
            OnRepeatForeverValueChanged(val.repeatForever);
            ToggleBroadcastField();
        }

        private InspectorField CreateDrawerForField(string name)
        {
            Type type = Value.GetType();
            FieldInfo fieldInfo = type.GetField(name);
            return CreateDrawerForVariable(fieldInfo, fieldInfo.Name, true);
        }

        public override void SetInteractable(bool on)
        {
            
        }
    }
}
