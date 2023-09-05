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
            Atom.Translate rt = (Atom.Translate)Value;
            if (rt.data.moveTo == default)
            {
                rt.data.moveTo = rt.target.transform.localPosition;
            }
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
                        translate.Type.data.moveTo = translate.Type.target.transform.localPosition;
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
            translateTypesDD.onValueChanged.AddListener(OnTranslateTypesValueChanged);
            int translationTypeIndex = (((int)Enum.Parse(typeof(TranslateType), rt.data.translateType.ToString())));
            translateTypesDD.SetValueWithoutNotify(translationTypeIndex);
            ShowTranslateOptionsMenu(translationTypeIndex);
            selectedTranslateType.SetData(rt.data);
            
            selectedTranslateType.RefreshUI();
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
    }
}
