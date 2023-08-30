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

        [SerializeField] private Dropdown listenOn;
#pragma warning restore 0649

        public override void Initialize()
        {
            base.Initialize();
            startOn.onValueChanged.AddListener(OnStartValueChanged);
            listenOn.onValueChanged.AddListener(OnListenValueChanged);
        }

        private void LoadListenTo()
        {
            // Debug.Log("listen to loaded");
            listenOn.options.Clear();
            foreach (string _name in EditorOp.Resolve<DataProvider>().ListenToTypes)
            {
                // Debug.Log($"name {_name}");
                listenOn.options.Add(new Dropdown.OptionData()
                {
                    text = _name
                });
            }
        }

        protected override void OnBound(MemberInfo variable)
        {
            base.OnBound(variable);
            LoadStartOnOptions();
            LoadListenTo();
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
                        LoadListenTo();
                        listenOn.gameObject.SetActive(true);
                    }
                }
                else if (listenOn.gameObject.activeSelf)
                {
                    listenOn.gameObject.SetActive(false);
                }
            }
        }

        private bool resetListenIndex = false;
        private void OnStartValueChanged(int _index)
        {
            if(Inspector)  Inspector.RefreshDelayed();
            Atom.StartOn atom = (Atom.StartOn)Value;
            atom.data.startIndex = _index;
            atom.data.startName = atom.StartList[_index];
            // rest the listen field
            // if (resetListenIndex) {
            //     atom.data.listenIndex = 0;
            // }
            // resetListenIndex = true;

            UpdateData(atom);
        }

        private void OnListenValueChanged(int _index)
        {
            if(Inspector)  Inspector.RefreshDelayed();
            Atom.StartOn atom = (Atom.StartOn)Value;
            atom.data.listenName = EditorOp.Resolve<DataProvider>().ListenToTypes[_index];
            atom.data.listenIndex = _index;
            UpdateData(atom);
        }

        protected override void OnSkinChanged()
        {
            base.OnSkinChanged();

            // toggleBackground.color = Skin.InputFieldNormalBackgroundColor;
            // toggleInput.graphic.color = Skin.ToggleCheckmarkColor;

            Vector2 rightSideAnchorMin = new Vector2( Skin.LabelWidthPercentage, 0f );
            // variableNameMask.rectTransform.anchorMin = rightSideAnchorMin;
            // ( (RectTransform) toggleInput.transform ).anchorMin = rightSideAnchorMin;
        }
        
        private void UpdateData(Atom.StartOn _atom)
        {
            return;
            List<GameObject> selectedObjects = EditorOp.Resolve<SelectionHandler>().GetSelectedObjects();
            if (selectedObjects.Count <= 1) return;
         
            foreach (var obj in selectedObjects)
            {
                foreach (Atom.StartOn atom in Atom.StartOn.AllInstances)
                {
                    if (obj.GetInstanceID() == atom.target.GetInstanceID())
                    { atom.data = Helper.DeepCopy(_atom.data);
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
                startOn.value = atom.data.startIndex;
                listenOn.value = atom.data.listenIndex;
                ShowHideListenDD();
            }
        }
    }
}