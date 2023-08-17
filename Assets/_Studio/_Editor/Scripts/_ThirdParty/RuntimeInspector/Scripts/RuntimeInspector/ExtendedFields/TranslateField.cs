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
        public Vector3 targetPosition = Vector3.zero;

#pragma warning restore 0649

        private TranslateTypes selectedTranslateType;

        public override void Initialize()
        {
            base.Initialize();
            base.layoutElement.minHeight = 253.9f;
            Setup();
        }

        private void Setup()
        {
            foreach (var type in allTranslateTypes)
            {
                type.translateField = this;
                type.Setup();
            }
            List<string> data = Enum.GetNames(typeof(TranslateType)).ToList();
            translateTypesDD.AddOptions(data);
        }

        public override bool SupportsType(Type type)
        {
            return type == typeof(Atom.Translate);
        }

        private void OnTranslateTypesValueChanged(int _index)
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
            Atom.Translate rt = (Atom.Translate)Value;
            rt.data.translateType = _index;
            allTranslateTypes[_index].gameObject.SetActive(true);
            selectedTranslateType = allTranslateTypes[_index];
            reset = reset || IsDataDefault();
            if (reset)
            {
                ResetValues();
            }
        }

        private void ResetValues()
        {
            var translateType = (TranslateType)((Atom.Translate)Value).data.translateType;
            var finalPath = translateType.GetPresetName("Translate");
            var preset = ((TranslatePreset)EditorOp.Load(ResourceTag.ComponentPresets, finalPath)).Value;
            LoadData(preset);
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
            variableNameMask.rectTransform.anchorMin = rightSideAnchorMin;
        }

        public void UpdateData(TranslateComponentData _rData)
        {
            Atom.Translate rt = (Atom.Translate)Value;
            _rData.translateType = rt.data.translateType;
            rt.data = _rData;
        }

        protected override void OnBound(MemberInfo variable)
        {
            base.OnBound(variable);
            Atom.Translate rt = (Atom.Translate)Value;
            translateTypesDD.onValueChanged.AddListener(OnTranslateTypesValueChanged);
            int translationTypeIndex = (((int)Enum.Parse(typeof(TranslateType), rt.data.translateType.ToString())));
            translateTypesDD.SetValueWithoutNotify(translationTypeIndex);
            ShowTranslateOptionsMenu(translationTypeIndex);
            selectedTranslateType.SetData(rt.data);
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
                selectedTranslateType.SetData(componentData.Value);
                rt.data = componentData.Value;
            }
            else
            {
                selectedTranslateType.SetData(rt.data);
            }
        }

        public TranslateComponentData GetAtomTranslateData()
        {
            Atom.Translate rt = (Atom.Translate)Value;
            return rt.data;
        }
    }
}
