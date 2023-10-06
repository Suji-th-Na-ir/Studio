using System;
using Terra.Studio;
using UnityEngine;
using UnityEngine.UI;
using PlayShifu.Terra;
using System.Reflection;
using System.Collections.Generic;

namespace RuntimeInspectorNamespace
{
    public class RotateField : InspectorField
    {
#pragma warning disable 0649

        [SerializeField]
        private Dropdown rotateTypesDD;
        public RotateTypes[] allRotateTypes;

#pragma warning restore 0649

        private RotateTypes selectedRotateType;
        private RotateComponentData lastComponentData;

        public override void Initialize()
        {
            base.Initialize();
        }

        private void Setup()
        {
            foreach (var type in allRotateTypes)
            {
                type.field = this;
                type.Setup();
            }
            List<string> data = Helper.GetEnumWithAliasNames<RotationType>();
            rotateTypesDD.AddOptions(data);
            rotateTypesDD.onValueChanged.AddListener(OnRotateTypesValueChanged);
        }

        public override bool SupportsType(Type type)
        {
            return type == typeof(Atom.Rotate);
        }

        private void OnRotateTypesValueChanged(int _index)
        {
            OnRotateTypesValueSubmitted(_index);
            var value = (RotateComponentData)lastSubmittedValue;
            if (_index != value.rotateType)
            {
                var newValue = ((Atom.Rotate)Value).data;
                EditorOp.Resolve<IURCommand>().Record(
                    lastSubmittedValue, newValue,
                    $"Rotate type changed to: {_index}",
                    (value) =>
                    {
                        OnRotateTypesValueSubmitted(((RotateComponentData)value).rotateType);
                        lastSubmittedValue = value;
                    });
                lastSubmittedValue = newValue;
            }
        }

        private void OnRotateTypesValueSubmitted(int _index)
        {
            ShowRotateOptionsMenu(_index, true);
        }

        private void HideAllRotateOptionsMenus()
        {
            foreach (var type in allRotateTypes)
            {
                type.gameObject.SetActive(false);
            }
        }

        private void ShowRotateOptionsMenu(int _index, bool reset = false)
        {
            HideAllRotateOptionsMenus();
            bool lastData = false;
            lastComponentData = new();
            if (selectedRotateType)
            {
                if (selectedRotateType.ValuesPerAxis != null)
                {
                    var vector3 = new Vector3(
                        float.Parse(selectedRotateType.ValuesPerAxis[0].text),
                        float.Parse(selectedRotateType.ValuesPerAxis[1].text),
                        float.Parse(selectedRotateType.ValuesPerAxis[2].text));
                    lastComponentData.vector3?.Set(vector3);
                }

                if (selectedRotateType.dirDropDown)
                    lastComponentData.direction = (Direction)selectedRotateType.dirDropDown.value;

                if (selectedRotateType.speedInput)
                    lastComponentData.speed = float.Parse(selectedRotateType.speedInput.text);
                else
                    lastComponentData.speed = -1;

                if (selectedRotateType.repeatInput)
                    lastComponentData.repeat = int.Parse(selectedRotateType.repeatInput.text);
                else
                    lastComponentData.repeat = -1;

                if (selectedRotateType.pauseInput)
                    lastComponentData.pauseBetween = float.Parse(selectedRotateType.pauseInput.text);
                else
                    lastComponentData.pauseBetween = -1;

                if (selectedRotateType.customString)
                    lastComponentData.Broadcast = selectedRotateType.customString.text;
                else
                    lastComponentData.Broadcast = "";

                if (selectedRotateType.canListenMultipleTimesToggle)
                    lastComponentData.listen = selectedRotateType.canListenMultipleTimesToggle ? Listen.Always : Listen.Once;

                if (selectedRotateType.broadcastAt)
                    lastComponentData.broadcastAt = (BroadcastAt)selectedRotateType.broadcastAt.value;

                lastData = true;
            }
            Atom.Rotate rt = (Atom.Rotate)Value;
            rt.data.rotateType = _index;
            allRotateTypes[_index].gameObject.SetActive(true);
            selectedRotateType = allRotateTypes[_index];
            reset = reset || IsDataDefault();
            if (reset)
            {
                ResetValues(lastData);
            }
        }

        private void ResetValues(bool lastDataPresent)
        {
            var value = (Atom.Rotate)Value;
            var rotationType = (RotationType)value.data.rotateType;
            var finalPath = rotationType.GetPresetName("Rotate");
            var preset = ((RotatePreset)EditorOp.Load(ResourceTag.ComponentPresets, finalPath)).Value;
            if (lastDataPresent)
            {
                lastComponentData.rotateType = (int)rotationType;
                var tempValue = lastComponentData;
                if (!selectedRotateType.speedInput || lastComponentData.speed == -1)
                    tempValue.speed = preset.speed;
                if (!selectedRotateType.repeatInput || lastComponentData.repeat == -1)
                    tempValue.repeat = preset.repeat;
                if (!selectedRotateType.pauseInput || lastComponentData.pauseBetween == -1)
                    tempValue.pauseBetween = preset.pauseBetween;
                if (!selectedRotateType.customString)
                    tempValue.Broadcast = "";
                preset = tempValue;
                value.behaviour.OnBroadcastStringUpdated(preset.Broadcast, value.data.Broadcast);
            }
            else
            {
                value.behaviour.OnBroadcastStringUpdated(string.Empty, value.data.Broadcast);
            }
            preset.vector3 = value.data.vector3;
            LoadData(preset);
            UpdateDataInUI(preset);
            UpdateTypeForMultiselect(rotationType, preset);
        }

        private void UpdateTypeForMultiselect(RotationType _data, RotateComponentData? compData = null)
        {
            List<GameObject> selectedObjecs = EditorOp.Resolve<SelectionHandler>().GetSelectedObjects();

            if (selectedObjecs.Count > 1)
            {
                foreach (var obj in selectedObjecs)
                {
                    if (obj.GetComponent<Rotate>() != null)
                    {
                        Rotate rotate = obj.GetComponent<Rotate>();
                        rotate.Type.data.rotateType = (int)_data;
                        rotate.Type.data = compData.Value;
                    }
                }
            }
        }

        private bool IsDataDefault()
        {
            var data = ((Atom.Rotate)Value).data;
            return data.IsEmpty();
        }

        protected override void OnSkinChanged()
        {
            base.OnSkinChanged();
            Vector2 rightSideAnchorMin = new Vector2(Skin.LabelWidthPercentage, 0f);
            if (variableNameMask != null)
            {
                variableNameMask.rectTransform.anchorMin = rightSideAnchorMin;
                variableNameMask.color = Skin.InputFieldTextColor;
            }
            rotateTypesDD.SetSkinDropDownField(Skin);
            foreach (var types in allRotateTypes)
            {
                types.ApplySkin(Skin);
            }
        }

        public Atom.Rotate GetAtom()
        {
            return (Atom.Rotate)Value;
        }

        protected override void OnBound(MemberInfo variable)
        {
            base.OnBound(variable);
            Setup();
            Atom.Rotate rt = (Atom.Rotate)Value;
            int rotationTypeIndex = (int)Enum.Parse(typeof(RotationType), rt.data.rotateType.ToString());
            rotateTypesDD.SetValueWithoutNotify(rotationTypeIndex);
            ShowRotateOptionsMenu(rotationTypeIndex);
            selectedRotateType.SetData(rt.data);
            lastSubmittedValue = rt.data;
            rt.ForceRefreshData = () => { selectedRotateType.SetData(rt.data); };
        }

        public override void Refresh()
        {
            base.Refresh();
            LoadData();
        }

        private void LoadData(RotateComponentData? compData = null)
        {
            Atom.Rotate rt = (Atom.Rotate)Value;
            rotateTypesDD.SetValueWithoutNotify(rt.data.rotateType);
            if (compData != null && compData.HasValue)
            {
                rt.data = compData.Value;
            }
        }

        private void UpdateDataInUI(RotateComponentData? compData = null)
        {
            Atom.Rotate rt = (Atom.Rotate)Value;
            if (compData != null && compData.HasValue)
            {
                selectedRotateType.SetData(compData.Value);
            }
            else
            {
                selectedRotateType.SetData(rt.data);
            }
        }

        public object GetLastSubmittedValue()
        {
            return lastSubmittedValue;
        }

        public void SetLastSubmittedValue(object data)
        {
            lastSubmittedValue = data;
        }
    }
}
