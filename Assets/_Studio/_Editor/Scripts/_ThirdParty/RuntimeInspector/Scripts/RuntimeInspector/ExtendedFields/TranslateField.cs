using System;
using System.Linq;
using Terra.Studio;
using UnityEngine;
using UnityEngine.UI;
using System.Reflection;
using System.Collections.Generic;
using PlayShifu.Terra;

namespace RuntimeInspectorNamespace
{
    public class TranslateField : InspectorField
    {
#pragma warning disable 0649

        public Dropdown translateTypesDD;
        public TranslateTypes[] allTranslateTypes;

#pragma warning restore 0649

        private TranslateTypes selectedTranslateType;
        TranslateComponentData lastComponentData;
        public override void Initialize()
        {
            base.Initialize();
            Setup();
        }

        private void Setup()
        {
            foreach (var type in allTranslateTypes)
            {
                type.field = this;
                type.Setup();
            }
            List<string> data = Enum.GetNames(typeof(TranslateType)).ToList();
            translateTypesDD.AddOptions(data);
            translateTypesDD.onValueChanged.AddListener(OnTranslateTypesValueChanged);
        }

        public override bool SupportsType(Type type)
        {
            return type == typeof(Atom.Translate);
        }

        private void OnTranslateTypesValueChanged(int _index)
        {
            OnTranslateTypesValueSubmitted(_index);
            var value = (TranslateComponentData)lastSubmittedValue;
            if (_index != value.translateType)
            {
                var newValue = ((Atom.Translate)Value).data;
                EditorOp.Resolve<IURCommand>().Record(
                    lastSubmittedValue, newValue,
                    $"Translate type changed to: {_index}",
                    (value) =>
                    {
                        OnTranslateTypesValueSubmitted(((TranslateComponentData)value).translateType);
                        lastSubmittedValue = value;
                    });
                lastSubmittedValue = newValue;
            }
        }

        private void OnTranslateTypesValueSubmitted(int _index)
        {
            ShowTranslateOptionsMenu(_index, true);
        }

        private void HideAllTranslateOptionsMenus()
        {
            foreach (var type in allTranslateTypes)
            {
                type.gameObject.SetActive(false);
            }
        }

        private void ShowTranslateOptionsMenu(int _index, bool reset = false)
        {
            HideAllTranslateOptionsMenus();
            bool lastData = false;
            lastComponentData = new();
            if (selectedTranslateType)
            {


                if (selectedTranslateType.movebyInput != null)
                {
                    Debug.Log(selectedTranslateType.movebyInput[0].text);
                    lastComponentData.moveBy = new Vector3(float.Parse(selectedTranslateType.movebyInput[0].text),
                        float.Parse(selectedTranslateType.movebyInput[1].text), (float.Parse(selectedTranslateType.movebyInput[2].text))); ;
                }
                if (selectedTranslateType.speedInput)
                    lastComponentData.speed = float.Parse(selectedTranslateType.speedInput.text);
                else
                    lastComponentData.speed = -1;

                if (selectedTranslateType.repeatInput)
                    lastComponentData.repeat = int.Parse(selectedTranslateType.repeatInput.text);
                else
                    lastComponentData.repeat = -1;

                if (selectedTranslateType.pauseForInput)
                    lastComponentData.pauseFor = float.Parse(selectedTranslateType.pauseForInput.text);
                else
                    lastComponentData.pauseFor = -1;

                if (selectedTranslateType.customString)
                    lastComponentData.broadcast = selectedTranslateType.customString.text;
                else
                    lastComponentData.broadcast = "";

                if (selectedTranslateType.canListenMultipleTimesToggle)
                    lastComponentData.listen = selectedTranslateType.canListenMultipleTimesToggle ? Listen.Always : Listen.Once;

                if (selectedTranslateType.broadcastAt)
                    lastComponentData.broadcastAt = (BroadcastAt)selectedTranslateType.broadcastAt.value;
                lastData = true;

            }
            Atom.Translate rt = (Atom.Translate)Value;
            rt.data.translateType = _index;
            allTranslateTypes[_index].gameObject.SetActive(true);
            selectedTranslateType = allTranslateTypes[_index];
            reset = reset || IsDataDefault();
            if (reset)
            {
                ResetValues(lastData);
            }
        }

        private void ResetValues(bool lastDataValue)
        {
            var translateType = (TranslateType)((Atom.Translate)Value).data.translateType;
            var finalPath = translateType.GetPresetName("Translate");
            var preset = ((TranslatePreset)EditorOp.Load(ResourceTag.ComponentPresets, finalPath)).Value;
            if (lastDataValue)
            {
                lastComponentData.translateType = (int)translateType;
                var tempValue = lastComponentData;

                if (!selectedTranslateType.speedInput || lastComponentData.speed == -1)
                    tempValue.speed = preset.speed;

                if (!selectedTranslateType.repeatInput || lastComponentData.repeat == -1)
                    tempValue.repeat = preset.repeat;
                if (!selectedTranslateType.pauseForInput || lastComponentData.pauseFor == -1)
                    tempValue.pauseFor = preset.pauseFor;

                if (!selectedTranslateType.customString)
                    tempValue.broadcast = "";
                preset = tempValue;

                EditorOp.Resolve<UILogicDisplayProcessor>().UpdateBroadcastString(
                    preset.broadcast,
                ((Atom.Translate)Value).data.broadcast,
                    new ComponentDisplayDock() { componentGameObject = ((Atom.Translate)Value).target, componentType = typeof(Atom.Translate).Name });
            }
            else
            {
                EditorOp.Resolve<UILogicDisplayProcessor>().UpdateBroadcastString(
                string.Empty,
                ((Atom.Translate)Value).data.broadcast,
                new ComponentDisplayDock() { componentGameObject = ((Atom.Translate)Value).target, componentType = typeof(Atom.Translate).Name });
            }
            LoadData(preset);
            UpdateDataInUI(preset);
            Atom.Translate rt = (Atom.Translate)Value;
            UpdateTypeForMultiselect(translateType, preset);
        }

        private void UpdateTypeForMultiselect(TranslateType _data, TranslateComponentData? componentData = null)
        {
            List<GameObject> selectedObjecs = EditorOp.Resolve<SelectionHandler>().GetSelectedObjects();

            if (selectedObjecs.Count > 1)
            {
                foreach (var obj in selectedObjecs)
                {
                    if (obj.GetComponent<Translate>() != null)
                    {
                        Translate translate = obj.GetComponent<Translate>();
                        translate.Type.data.translateType = (int)_data;
                        translate.Type.data = componentData.Value;
                    }
                }
            }
        }

        private bool IsDataDefault()
        {
            var data = ((Atom.Translate)Value).data;
            if (data.Equals(default(TranslateComponentData)))
            {
                return true;
            }
            return false;
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
            translateTypesDD.SetSkinDropDownField(Skin);
            foreach (var types in allTranslateTypes)
            {
                types.ApplySkin(Skin);
            }
        }

        public Atom.Translate GetAtom()
        {
            return (Atom.Translate)Value;
        }

        protected override void OnBound(MemberInfo variable)
        {
            // Debug.Log("translate bound called "+this.gameObject.name);
            base.OnBound(variable);
            Atom.Translate rt = (Atom.Translate)Value;
            // Debug.Log($"translate atom data x value {rt.data.moveTo}");
            // translateTypesDD.onValueChanged.AddListener(OnTranslateTypesValueChanged);
            int translationTypeIndex = (((int)Enum.Parse(typeof(TranslateType), rt.data.translateType.ToString())));
            translateTypesDD.SetValueWithoutNotify(translationTypeIndex);
            ShowTranslateOptionsMenu(translationTypeIndex);
            selectedTranslateType.SetData(rt.data);
            lastSubmittedValue = rt.data;
        }


        public override void Refresh()
        {
            base.Refresh();
            LoadData();
        }

        private void LoadData(TranslateComponentData? componentData = null)
        {
            Atom.Translate rt = (Atom.Translate)Value;
            translateTypesDD.SetValueWithoutNotify(rt.data.translateType);
            if (componentData != null && componentData.HasValue)
            {
                //selectedTranslateType.SetData(componentData.Value);
                rt.data = componentData.Value;
            }
            else
            {
                // selectedTranslateType.SetData(rt.data);
            }
        }

        private void UpdateDataInUI(TranslateComponentData? componentData = null)
        {

            Atom.Translate rt = (Atom.Translate)Value;
            // translateTypesDD.SetValueWithoutNotify(rt.data.translateType);
            if (componentData != null && componentData.HasValue)
            {
                selectedTranslateType.SetData(componentData.Value);
                //rt.data = componentData.Value;
            }
            else
            {
                selectedTranslateType.SetData(rt.data);
            }
        }

        public object GetLastSubmittedValue()
        {
            return lastSubmittedValue;
        }

        public void SetLastSubmittedValue(object newValue)
        {
            lastSubmittedValue = newValue;
        }
    }
}
