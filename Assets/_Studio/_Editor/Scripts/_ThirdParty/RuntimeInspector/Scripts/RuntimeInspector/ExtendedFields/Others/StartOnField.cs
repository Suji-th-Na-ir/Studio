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
        [SerializeField] private Dropdown listenOn;
#pragma warning restore 0649

        public override void Initialize()
        {
            base.Initialize();
            startOn.onValueChanged.RemoveAllListeners();
            startOn.onValueChanged.AddListener(OnStartValueChanged);
            listenOn.onValueChanged.RemoveAllListeners();
            listenOn.onValueChanged.AddListener(OnListenValueChanged);
        }

        protected override void OnBound(MemberInfo variable)
        {
            base.OnBound(variable);
            LoadStartOnOptions();
            LoadListenOptions();
            lastSubmittedValue = ((Atom.StartOn)Value).data;
            EditorOp.Resolve<FocusFieldsSystem>().AddFocusedGameobjects(startOn.gameObject,
                () => startOn.targetGraphic.color = Skin.SelectedItemBackgroundColor,
                () => startOn.targetGraphic.color = Skin.InputFieldNormalBackgroundColor);
            EditorOp.Resolve<FocusFieldsSystem>().AddFocusedGameobjects(listenOn.gameObject,
                () => listenOn.targetGraphic.color = Skin.SelectedItemBackgroundColor,
                () => listenOn.targetGraphic.color = Skin.InputFieldNormalBackgroundColor);
        }

        private void LoadListenOptions()
        {
            listenOn.options.Clear();
            listenOn.AddOptions(SystemOp.Resolve<CrossSceneDataHolder>().BroadcastStrings.FindAll(s=>(s!="Game Win"&& s != "Game Lose")));
            Atom.StartOn atom = (Atom.StartOn)Value;
            for (int i = 0; i < listenOn.options.Count; i++)
            {
                if (listenOn.options[i].text == atom.data.listenName)
                {
                    listenOn.SetValueWithoutNotify(i);
                    break;
                }
            }
        }

        protected override void OnUnbound()
        {
            base.OnUnbound();
            EditorOp.Resolve<FocusFieldsSystem>().RemoveFocusedGameObjects(startOn.gameObject);
            EditorOp.Resolve<FocusFieldsSystem>().RemoveFocusedGameObjects(listenOn.gameObject);
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
            UpdateListenValue(0);
           
            if (Inspector) Inspector.RefreshDelayed();
        }

        private void OnListenValueChanged(int value)
        {
            Atom.StartOn atom = (Atom.StartOn)Value;
            UpdateListenValue(value);
            if (listenOn.options[value].text != ((StartOnData)lastSubmittedValue).listenName)
            {
                EditorOp.Resolve<IURCommand>().Record(
                    lastSubmittedValue, atom.data,
                    $"Listen value changed to: {atom.data.listenName}",
                    (value) =>
                    {
                        for (int i = 0; i < listenOn.options.Count; i++)
                        {
                            var listenstring = ((StartOnData)value).listenName;
                            
                            if (string.IsNullOrEmpty(listenstring))
                                listenstring = "None";
                           
                            if (listenOn.options[i].text == listenstring)
                            {
                                UpdateListenValue(i);
                                break;
                            }
                        }
                       
                    });
            }
            lastSubmittedValue = atom.data;
        }

        private void UpdateListenValue(int value)
        {
            Atom.StartOn atom = (Atom.StartOn)Value;

            var oldstering = atom.data.listenName;
            var listenstring = listenOn.options[value].text;
            if (listenstring == "None")
                listenstring = string.Empty;
            atom.data.listenName = listenstring;
            UpdateOtherCompData(atom);
            atom.OnListenerUpdated?.Invoke(listenstring, oldstering);
        }


        protected override void OnSkinChanged()
        {
            base.OnSkinChanged();
            startOn.SetSkinDropDownField(Skin);
            listenOn.SetSkinDropDownField(Skin);
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
                if (SystemOp.Resolve<CrossSceneDataHolder>().BroadcastStrings.Count-2 > listenOn.options.Count)
                {
                    listenOn.ClearOptions();
                    listenOn.AddOptions(SystemOp.Resolve<CrossSceneDataHolder>().BroadcastStrings.FindAll(s => (s != "Game Win" && s != "Game Lose")));
                }

                var listenString = atom.data.listenName;
                if (atom.data.listenName == String.Empty)
                    listenString = "None";
                if (listenOn.options[listenOn.value].text != listenString)
                {
                    for (int i = 0; i < listenOn.options.Count; i++)
                    {
                        if (listenOn.options[i].text == listenString)
                        {
                            listenOn.SetValueWithoutNotify(i);
                            break;
                        }
                    }
                }
                ShowHideListenDD();
            }
        }

        public override void SetInteractable(bool on , bool disableAlso=false)
        {
            startOn.interactable = on;
            listenOn.interactable = on;
        }
    }
}