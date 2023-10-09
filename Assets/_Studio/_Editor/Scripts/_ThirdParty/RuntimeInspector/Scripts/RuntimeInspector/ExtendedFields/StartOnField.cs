using System;
using UnityEngine;
using Terra.Studio;
using UnityEngine.UI;
using PlayShifu.Terra;
using System.Reflection;
using System.Collections.Generic;

namespace RuntimeInspectorNamespace
{
    public class StartOnField : InspectorField
    {
#pragma warning disable 0649
        [SerializeField] private Dropdown startOn;
        [SerializeField] private InputField listenOn;
#pragma warning restore 0649

        public override void Initialize()
        {
            base.Initialize();
            startOn.onValueChanged.AddListener(OnStartValueChanged);
            listenOn.onValueChanged.AddListener(OnListenValueChanged);
            listenOn.onEndEdit.AddListener(OnListenValueSubmitted);
        }

        protected override void OnBound(MemberInfo variable)
        {
            base.OnBound(variable);
            LoadStartOnOptions();
            lastSubmittedValue = ((Atom.StartOn)lastSubmittedValue).data;
            EditorOp.Resolve<FocusFieldsSystem>().AddFocusedGameobjects(startOn.gameObject,
                  () => startOn.targetGraphic.color = Skin.SelectedItemBackgroundColor,
                () => startOn.targetGraphic.color = Skin.InputFieldNormalBackgroundColor);
        }

        protected override void OnUnbound()
        {
            base.OnUnbound();
            EditorOp.Resolve<FocusFieldsSystem>().RemoveFocusedGameObjects(startOn.gameObject);
        }


        private void LoadStartOnOptions()
        {
            Atom.StartOn atom = (Atom.StartOn)Value;
            if (atom.aliasNameList.Count > 0)
            {
                startOn.options.Clear();
                foreach (string item in atom.aliasNameList)
                {
                    startOn.options.Add(new Dropdown.OptionData()
                    {
                        text = item
                    });
                }
            }
            startOn.captionText.text = atom.aliasNameList[atom.data.startIndex];
        }

        public override bool SupportsType(Type type)
        {
            return type == typeof(Atom.StartOn);
        }

        public void ShowHideListenDD()
        {
            Atom.StartOn atom = (Atom.StartOn)Value;
            if (atom.startList.Count > 0)
            {
                string startValue = (atom.startList[startOn.value]).ToLower();
                if (startValue.Contains("listen"))
                {
                    if (!listenOn.gameObject.activeSelf)
                    {
                        listenOn.gameObject.SetActive(true);
                    }
                }
                else if (listenOn.gameObject.activeSelf)
                {
                    listenOn.gameObject.SetActive(false);
                }
            }
        }

        private void OnStartValueChanged(int _index)
        {
            OnStartOnValueSubmitted(_index);
            Atom.StartOn atom = (Atom.StartOn)Value;
            if (_index != ((StartOnData)lastSubmittedValue).startIndex)
            {
                EditorOp.Resolve<IURCommand>().Record(
                    lastSubmittedValue, atom.data,
                    $"Start on changed to: {atom.data.startName}",
                    (value) =>
                    {
                        OnStartOnValueSubmitted(((StartOnData)value).startIndex);
                    });
            }
            lastSubmittedValue = atom.data;
        }

        private void OnStartOnValueSubmitted(int _index)
        {
            Atom.StartOn atom = (Atom.StartOn)Value;
            atom.data.startIndex = _index;
            atom.data.startName = atom.startList[_index];
            atom.data.listenName = "";
            UpdateOtherCompData(atom);
            if (Inspector) Inspector.RefreshDelayed();
        }

        private void OnListenValueChanged(string _newString)
        {
            Atom.StartOn atom = (Atom.StartOn)Value;
            if (Inspector)
            {
                Inspector.RefreshDelayed();
            }
            atom.OnListenerUpdated?.Invoke(_newString, atom.data.listenName);
            atom.data.listenName = _newString;
            UpdateOtherCompData(atom);
        }

        public void OnListenValueSubmitted(string _newString)
        {
            Atom.StartOn atom = (Atom.StartOn)Value;
            if (_newString != ((StartOnData)lastSubmittedValue).listenName)
            {
                EditorOp.Resolve<IURCommand>().Record(
                    lastSubmittedValue, atom.data,
                    $"Listen value changed to: {atom.data.startName}",
                    (value) =>
                    {
                        OnListenValueChanged(((StartOnData)value).listenName);
                    });
            }
            lastSubmittedValue = atom.data;
        }

        protected override void OnSkinChanged()
        {
            base.OnSkinChanged();
            startOn.SetSkinDropDownField(Skin);
            listenOn.SetupInputFieldSkin(Skin);
        }

        private void UpdateOtherCompData(Atom.StartOn _atom)
        {
            List<GameObject> selectedObjects = EditorOp.Resolve<SelectionHandler>().GetSelectedObjects();
            if (selectedObjects.Count <= 1) return;
            foreach (var obj in selectedObjects)
            {
                var allInstances = EditorOp.Resolve<Atom>().AllStartOns;
                foreach (Atom.StartOn atom in allInstances)
                {
                    if (obj.GetInstanceID() == atom.target.GetInstanceID())
                    {
                        if (_atom.componentType.Equals(atom.componentType))
                        {
                            atom.data = Helper.DeepCopy(_atom.data);
                        }
                    }
                }
            }
        }

        public override void Refresh()
        {
            base.Refresh();
            LoadData();
        }

        private void LoadData()
        {
            Atom.StartOn atom = (Atom.StartOn)Value;
            if (atom != null)
            {
                startOn.SetValueWithoutNotify(atom.data.startIndex);
                listenOn.SetTextWithoutNotify(atom.data.listenName);
                ShowHideListenDD();
            }
        }
    }
}