using System;
using System.Collections.Generic;
using PlayShifu.Terra;
using UnityEngine;
using UnityEngine.UI;
using Terra.Studio;
using System.Reflection;
namespace RuntimeInspectorNamespace
{
    public class BroadcastField : InspectorField
    {
#pragma warning disable 0649
        [SerializeField] private Dropdown broadcastType;
        [SerializeField] private InputField customString;
#pragma warning restore 0649
        
        private int prevListenOnValue = -1;
        public override void Initialize()
        {
            base.Initialize();
            broadcastType.onValueChanged.AddListener(OnBroadcastTypeChanged);
            customString.onValueChanged.AddListener(OnCustomStringChanged);
            LoadBroadcastType();
            ShowCustomStringInputGroup();
            LoadDataFromAtom();
        }

        public override bool SupportsType(Type type)
        {
            return type == typeof(Atom.Broadcast);
        }

        private void LoadBroadcastType()
        {
            broadcastType.options.Clear();
            List<string> newList = EditorOp.Resolve<DataProvider>().ListenToTypes;
            foreach (string _name in newList)
            {
                broadcastType.options.Add(new Dropdown.OptionData()
                {
                    text = _name
                });
            }
        }
        
        protected override void OnBound(MemberInfo variable)
        {
            base.OnBound(variable);
            LoadBroadcastType();
            LoadDataFromAtom();
            ShowCustomStringInputGroup();
        }

        private void OnBroadcastTypeChanged(int _index)
        {
            Atom.Broadcast atom = (Atom.Broadcast)Value;
            atom.data.broadcastTypeIndex = _index;
            ShowCustomStringInputGroup();
            ResetCustomString();
        }

        private void ResetCustomString()
        {
            Atom.Broadcast atom = (Atom.Broadcast)Value;
            atom.data.broadcastName = "";
            customString.text = "";
        }

        private void ShowCustomStringInputGroup()
        {
            Atom.Broadcast atom = (Atom.Broadcast)Value;
            if (atom != null)
            {
                int index = atom.data.broadcastTypeIndex;
                string selectedString = "None";
                if(index < broadcastType.options.Count)
                    selectedString = broadcastType.options[index].text;


                if (selectedString.ToLower().Contains("custom"))
                    customString.gameObject.SetActive(true);
                else
                    customString.gameObject.SetActive(false);
            }
            else
            {
                customString.gameObject.SetActive(false);
            }
        }

        private void OnCustomStringChanged(string _newString)
        {
            SetCustomString(_newString);
        }
        
        private void SetCustomString(string _newString)
        {
            Atom.Broadcast atom = (Atom.Broadcast)Value;
            EditorOp.Resolve<UILogicDisplayProcessor>().UpdateBroadcastString(_newString, atom.data.broadcastName, new ComponentDisplayDock { componentGameObject = atom.target, componentType = atom.componentType });
            atom.data.broadcastName = _newString;
            
            EditorOp.Resolve<DataProvider>().UpdateToListenList(atom.data.id, _newString);
        }

        public override void Refresh()
        {
            base.Refresh();
            LoadDataFromAtom();
        }
        
        private void LoadDataFromAtom()
        {
            Atom.Broadcast atom = (Atom.Broadcast) Value;
            if (atom != null)
            {
                broadcastType.SetValueWithoutNotify(atom.data.broadcastTypeIndex);
                customString.SetTextWithoutNotify(atom.data.broadcastName);
            }
        }


        protected override void OnSkinChanged()
        {
            base.OnSkinChanged();
            broadcastType?.SetSkinDropDownField(Skin);
            customString?.SetupInputFieldSkin(Skin);
        }
    }
}
