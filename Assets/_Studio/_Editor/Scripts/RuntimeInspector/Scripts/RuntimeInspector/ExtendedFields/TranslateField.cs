using System;
using System.Collections.Generic;
using System.Linq;
using Terra.Studio;
using UnityEngine;
using UnityEngine.UI;
using Terra.Studio.RTEditor;
using PlayShifu.Terra;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace RuntimeInspectorNamespace
{
    public class TranslateField : InspectorField
    {
#pragma warning disable 0649
        [SerializeField] public Dropdown translateTypesDD;
        public TranslateTypes targetDirectionType;
        public TranslateTypes oscillateType;
   
#pragma warning restore 0649

        private TranslateTypes selectedTranslateType;

        public override void Initialize()
        {
            base.Initialize();
            base.layoutElement.minHeight = 281.8f;
            Setup();
        }

        private void Setup()
        {
            targetDirectionType.translateField = this;
            oscillateType.translateField = this;
      
            
            targetDirectionType.Setup();
            oscillateType.Setup();

            List<string> data = Enum.GetNames(typeof(TranslateType)).ToList();
            translateTypesDD.AddOptions(data);
        }

        public override bool SupportsType(Type type)
        {
            return type == typeof(Atom.Translate);
        }

        private void OnTranslateTypesValueChanged(int _index)
        {
            ShowTranslateOptionsMenu(_index);
        }

        private void HideAllTranslateOptionsMenus()
        {
            targetDirectionType.gameObject.SetActive(false);
            oscillateType.gameObject.SetActive(false);
        }

        private void ShowTranslateOptionsMenu(int _index)
        {
            HideAllTranslateOptionsMenus();
            Atom.Translate rt = (Atom.Translate)Value;
            rt.data.translateType = _index;
            switch (_index)
            {
                case 0:
                    targetDirectionType.gameObject.SetActive(true);
                    selectedTranslateType = targetDirectionType;
                    break;
                case 1:
                    oscillateType.gameObject.SetActive(true);
                    selectedTranslateType = oscillateType;
                    break;
            }
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
            translateTypesDD.value = translationTypeIndex;
            ShowTranslateOptionsMenu(translationTypeIndex);
            selectedTranslateType.SetData(rt.data);
        }
    }
}
