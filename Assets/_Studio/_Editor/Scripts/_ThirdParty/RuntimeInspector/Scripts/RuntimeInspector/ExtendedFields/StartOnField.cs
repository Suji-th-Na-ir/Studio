using System;
using System.Collections.Generic;
using PlayShifu.Terra;
using UnityEngine;
using UnityEngine.UI;
using Terra.Studio;
using System.Reflection;

namespace RuntimeInspectorNamespace
{
    public class StartOnField : InspectorField
    {
#pragma warning disable 0649
        [SerializeField] private Dropdown startOn;

        [SerializeField] private InputField listenOn;
#pragma warning restore 0649

        private int prevListenOnValue = -1;
        public override void Initialize()
        {
            base.Initialize();
            startOn.onValueChanged.AddListener(OnStartValueChanged);
            listenOn.onValueChanged.AddListener(OnListenValueChanged);
        }

        // private void LoadListenTo()
        // {
        //     listenOn.options.Clear();
        //     // lets hide last entry which is "Custom"
        //     List<string> loadList = EditorOp.Resolve<DataProvider>().ListenToTypes;
        //     for (int i = 0; i < loadList.Count - 1; i++)
        //     {
        //         listenOn.options.Add(new Dropdown.OptionData()
        //         {
        //             text = loadList[i]
        //         });
        //     }
        // }

        protected override void OnBound(MemberInfo variable)
        {
            base.OnBound(variable);
            LoadStartOnOptions();
        }

        private void LoadStartOnOptions()
        {
            Atom.StartOn atom = (Atom.StartOn)Value;
            if (atom.StartList.Count > 0)
            {
                startOn.options.Clear();
                foreach (string item in atom.StartList)
                {
                    startOn.options.Add(new Dropdown.OptionData()
                    {
                        text = item
                    });
                }
            }
            startOn.captionText.text = atom.StartList[atom.data.startIndex];
        }

        public override bool SupportsType( Type type )
        {
            return type == typeof( Atom.StartOn );
        }

        public void ShowHideListenDD()
        {
            Atom.StartOn atom = (Atom.StartOn)Value;
            if (atom.StartList.Count > 0)
            {
                string startValue = (atom.StartList[startOn.value]).ToLower();
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
            if (Inspector) Inspector.RefreshDelayed();
            Atom.StartOn atom = (Atom.StartOn)Value;
            atom.data.startIndex = _index;
            atom.data.startName = atom.StartList[_index];
            atom.data.listenName = "";
            // reset the listen field to previous value
            // if(prevListenOnValue != -1)
            //     atom.data.listenIndex = prevListenOnValue;
            // var prevString = string.Empty;
            // var newString = string.Empty;

            // if (_index != 2)
            // {
            //     prevString = atom.data.listenName;
            // }
            // else
            // {
            //     newString = atom.data.listenName;
            // }
            //
            // EditorOp.Resolve<UILogicDisplayProcessor>().UpdateListenerString(newString, prevString,
            //         new ComponentDisplayDock() { componentGameObject = atom.target, componentType = atom.componentType });
            UpdateOtherCompData(atom);
        }

        private void OnListenValueChanged(string _newString)
        {
            Atom.StartOn atom = (Atom.StartOn)Value;
   
            
            EditorOp.Resolve<UILogicDisplayProcessor>().UpdateListenerString(_newString, atom.data.listenName, 
                new ComponentDisplayDock() { componentGameObject = atom.target, componentType = atom.componentType });
           
            if(Inspector)  Inspector.RefreshDelayed();
            atom.data.listenName = _newString;
            // atom.data.listenIndex = _index;
            UpdateOtherCompData(atom);
            // prevListenOnValue = _index;
        }

        protected override void OnSkinChanged()
        {
            base.OnSkinChanged();

            // toggleBackground.color = Skin.InputFieldNormalBackgroundColor;
            // toggleInput.graphic.color = Skin.ToggleCheckmarkColor;

            Vector2 rightSideAnchorMin = new Vector2( Skin.LabelWidthPercentage, 0f );
            startOn.SetSkinDropDownField(Skin);
            listenOn.SetupInputFieldSkin(Skin);
            // variableNameMask.rectTransform.anchorMin = rightSideAnchorMin;
            // ( (RectTransform) toggleInput.transform ).anchorMin = rightSideAnchorMin;
        }
        
        private void UpdateOtherCompData(Atom.StartOn _atom)
        {
            List<GameObject> selectedObjects = EditorOp.Resolve<SelectionHandler>().GetSelectedObjects();
            if (selectedObjects.Count <= 1) return;
         
            foreach (var obj in selectedObjects)
            {
                foreach (Atom.StartOn atom in Atom.StartOn.AllInstances)
                {
                    if (obj.GetInstanceID() == atom.target.GetInstanceID())
                    {
                        if (_atom.componentType.Equals(atom.componentType))
                        {
                            EditorOp.Resolve<UILogicDisplayProcessor>().UpdateListenerString(_atom.data.listenName, atom.data.listenName,
                            new ComponentDisplayDock() { componentGameObject = atom.target, componentType = atom.componentType });
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