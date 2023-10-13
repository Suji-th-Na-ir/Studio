using System;
using UnityEngine;
using Terra.Studio;
using System.Reflection;
using System.Globalization;
using System.Linq;
using System.Collections.Generic;
using PlayShifu.Terra;

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

        enum RepeatData
        {
            RepeatFor,
            RepeatForever,
            RepeatType,
            BroadcastType,
            PauseFor,
            Broadcast
        }

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
            var val = (Atom.Repeat)Value;
            UpdateOtherCompData(val, RepeatData.BroadcastType);
        }


        private void OnPauseForValueChanged(object value)
        {
            UpdatebroadcastTypeDropDown();
            UpdateOtherCompData((Atom.Repeat)Value, RepeatData.PauseFor);
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
            UpdateOtherCompData(val, RepeatData.RepeatFor);
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
                ToggelPauseForAndRepeat(true);  
            }
            else
            {
                repeatForFieldDrawer.SetInteractable(true);
                OnRepeatValueChanged(val.repeat);
            }
            UpdatebroadcastTypeDropDown();
            UpdateOtherCompData(val, RepeatData.RepeatForever);
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
            UpdateOtherCompData(val, RepeatData.BroadcastType);
        }

        private void UpdateOtherCompData(Atom.Repeat _atom, RepeatData data)
        {
            List<GameObject> selectedObjects = EditorOp.Resolve<SelectionHandler>().GetSelectedObjects();
            if (selectedObjects.Count <= 1) return;
            foreach (var obj in selectedObjects)
            {
                var allInstances = EditorOp.Resolve<Atom>().AllRepeats;
                foreach (Atom.Repeat atom in allInstances)
                {
                    if (obj.GetInstanceID() == atom.target.GetInstanceID() &&
                        _atom.behaviour.GetType().Equals(atom.behaviour.GetType()))
                    {
                        switch (data)
                        {
                            case RepeatData.RepeatFor:
                                atom.repeat = _atom.repeat;
                                break;
                            case RepeatData.RepeatForever:
                                atom.repeatForever = _atom.repeatForever;
                                break;
                            case RepeatData.RepeatType:
                                atom.repeatType = _atom.repeatType;
                                break;
                            case RepeatData.PauseFor:
                                if(!atom.repeatForever &&atom.repeat>1)
                                {
                                    _atom.pauseFor = atom.pauseFor;
                                }
                                break;
                            case RepeatData.BroadcastType:
                                atom.broadcastAt = _atom.broadcastAt;
                                break;
                            case RepeatData.Broadcast:
                                if (atom.broadcastAt != BroadcastAt.Never)
                                {
                                    atom.broadcast = _atom.broadcast;
                                }
                                break;
                        }
                    }
                }
            }
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
            broadcastFieldDrawer.OnValueUpdated -= OnBroadcastValueChanged;
        }

        private void OnBroadcastValueChanged(object obj)
        {
            UpdateOtherCompData((Atom.Repeat)Value, RepeatData.Broadcast);
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
            broadcastFieldDrawer.OnValueUpdated += OnBroadcastValueChanged;

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
