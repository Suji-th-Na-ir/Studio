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
            Broadcast,
            LastData,
            RevertToOldValues
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
                broadcastFieldDrawer.SetInteractable(false, true);
                val.Broadcast = string.Empty;
                UpdateOtherCompData(val, RepeatData.Broadcast);
            }
            else
            {
                if(!string.IsNullOrEmpty( val.lastEnteredBroadcast))
                {
                    val.Broadcast = val.lastEnteredBroadcast;
                }
                broadcastFieldDrawer.SetInteractable(true, true);
                UpdateOtherCompData(val, RepeatData.RevertToOldValues);
            }
        }

        private void OnRepeatValueChanged(object value)
        {
            Atom.Repeat val = ValidateRepeatValue();
            UpdateOtherCompData(val, RepeatData.RepeatFor);
            UpdatebroadcastTypeDropDown();
            val.behaviour.GhostDescription.UpdateSelectionGhostsRepeatCount?.Invoke();
        }

        private void OnRepeatTypeValueChanged(object obj)
        {
            UpdateOtherCompData((Atom.Repeat)Value, RepeatData.RepeatType);
            ((Atom.Repeat)Value).behaviour.GhostDescription.UpdateSelectionGhostsRepeatCount?.Invoke();
        }

        private void OnRepeatForeverValueChanged(object value)
        {
            Atom.Repeat val = ValidateRepeatForeverValue(value);
            UpdateOtherCompData(val, RepeatData.RepeatForever);
            ((Atom.Repeat)Value).behaviour.GhostDescription.UpdateSelectionGhostsRepeatCount?.Invoke();
        }

        private void OnPauseForValueChanged(object value)
        {
            UpdateOtherCompData((Atom.Repeat)Value, RepeatData.PauseFor);
            UpdatebroadcastTypeDropDown();
        }

        private void OnBroadcastTypeValueChanged(object value)
        {
            UpdateOtherCompData((Atom.Repeat)Value, RepeatData.BroadcastType);
            ToggleBroadcastField();
        }

        private void OnBroadcastValueChanged(object obj)
        {
            var val = (Atom.Repeat)Value;
            val.lastEnteredBroadcast = val.Broadcast;
            UpdateOtherCompData(val, RepeatData.LastData);
            UpdateOtherCompData(val, RepeatData.Broadcast);
           
        }

        private Atom.Repeat ValidateRepeatValue()
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
            return val;
        }

        private void ToggelPauseForAndRepeat(bool on)
        {
            pauseForFieldDrawer.SetInteractable(on,true);
            repeatTypeFieldDrawer.SetInteractable(on, true);
        }


        private Atom.Repeat ValidateRepeatForeverValue(object value)
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
                ValidateRepeatValue(); 
            }
            UpdatebroadcastTypeDropDown();

            return val;
        }

        private void UpdatebroadcastTypeDropDown()
        {
            List<string> ignoreNames = new List<string>();
            var val = (Atom.Repeat)Value;
            ignoreNames = GetIgnoredNamesOfDropDown(val);

            broadcastTypeFieldDrawer.InvokeUpdateDropdown(Enum.GetNames(typeof(BroadcastAt)).Where(name => !ignoreNames.Contains(name)).ToList());
            ToggleBroadcastField();
        }

        private List<String> GetIgnoredNamesOfDropDown( Atom.Repeat val)
        {
            List<string> ignore = new List<string>();
            if (val.repeatForever)
            {
                ignore.Add(BroadcastAt.End.ToString());
                if (val.pauseFor == 0)
                {
                    ignore.Add(BroadcastAt.AtEveryPause.ToString());
                }
            }
            else if (val.repeat < 2)
            {
                ignore.Add(BroadcastAt.AtEveryPause.ToString());
            }
            return ignore;
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
                    if (atom == _atom)
                        continue;
                    if (obj == atom.target &&
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
                                if (!atom.repeatForever && atom.repeat > 1)
                                {
                                    atom.pauseFor = _atom.pauseFor;
                                }
                                else if (atom.repeatForever)
                                {
                                    atom.pauseFor = _atom.pauseFor;
                                }
                                break;
                            case RepeatData.BroadcastType:
                                var ignore = GetIgnoredNamesOfDropDown(atom);
                                var name = Enum.GetName(typeof(BroadcastAt), _atom.broadcastAt);
                                if (!ignore.Contains(name))
                                {
                                    atom.broadcastAt = _atom.broadcastAt;
                                }
                                if (atom.broadcastAt == BroadcastAt.Never)
                                {
                                    atom.Broadcast = String.Empty;
                                }
                                break;

                            case RepeatData.Broadcast:
                                atom.Broadcast = _atom.Broadcast;
                                break;
                            case RepeatData.LastData:
                                atom.lastEnteredBroadcast = atom.Broadcast;
                                break;
                            case RepeatData.RevertToOldValues:
                                if (!string.IsNullOrEmpty(atom.lastEnteredBroadcast))
                                {
                                    atom.Broadcast = string.Empty;
                                    atom.Broadcast = atom.lastEnteredBroadcast;
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
            repeatTypeFieldDrawer.OnValueUpdated -= OnRepeatTypeValueChanged;
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
            if ((bool)repeatFoeverFieldDrawer.Value != ((Atom.Repeat)Value).repeatForever)
            {
                ValidateRepeatForeverValue(((Atom.Repeat)Value).repeatForever);
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
            var val = (Atom.Repeat)Value;
            repeatForFieldDrawer = CreateDrawerForField(nameof(val.repeat));
            repeatForFieldDrawer.OnValueUpdated += OnRepeatValueChanged;

            repeatFoeverFieldDrawer = CreateDrawerForField(nameof(val.repeatForever));
            repeatFoeverFieldDrawer.OnValueUpdated += OnRepeatForeverValueChanged; 

            pauseForFieldDrawer = CreateDrawerForField(nameof(val.pauseFor));
            pauseForFieldDrawer.OnValueUpdated += OnPauseForValueChanged;

            repeatTypeFieldDrawer = CreateDrawerForField(nameof(val.repeatType));
            repeatTypeFieldDrawer.OnValueUpdated += OnRepeatTypeValueChanged;

            broadcastTypeFieldDrawer = CreateDrawerForField(nameof(val.broadcastAt));
            broadcastTypeFieldDrawer.OnValueUpdated += OnBroadcastTypeValueChanged;

            broadcastFieldDrawer = CreateDrawerForField(nameof(val.broadcastData));
            broadcastFieldDrawer.OnValueUpdated += OnBroadcastValueChanged;

            ValidateRepeatValue();
            ValidateRepeatForeverValue(val.repeatForever);
        }
    }
}
