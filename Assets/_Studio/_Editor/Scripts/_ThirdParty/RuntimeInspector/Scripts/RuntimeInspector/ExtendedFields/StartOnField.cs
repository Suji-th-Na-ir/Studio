using System;
using System.Collections.Generic;
using PlayShifu.Terra;
using UnityEngine;
using UnityEngine.UI;
using Terra.Studio;
using Unity.Plastic.Newtonsoft.Json.Converters;
using UnityEngine.Serialization;

namespace RuntimeInspectorNamespace
{
    public class StartOnField : InspectorField
    {
#pragma warning disable 0649
        [SerializeField] private Dropdown StartOn;

        [SerializeField] private Dropdown ListenOn;
#pragma warning restore 0649
        
        public override void Initialize()
        {
            base.Initialize();
            
            StartOn.onValueChanged.AddListener(OnStartValueChanged);
            LoadStartOnOptions();
            ListenOn.onValueChanged.AddListener(OnListenValueChanged);
            LoadListenTo();
            
            ShowHideListenDD();
        }

        private void LoadListenTo()
        {
            foreach (string _name in Helper.GetListenToTypes())
            {
                ListenOn.options.Add(new Dropdown.OptionData()
                {
                    text = _name
                });
            }
        }

        private void LoadStartOnOptions()
        {
            StartOn.options.Clear();
            int enumSize = System.Enum.GetValues(typeof(DestroyOnEnum)).Length;
            for (int i = 0; i < enumSize; i++)
            {
                StartOn.options.Add(new Dropdown.OptionData()
                {
                    text = ((DestroyOnEnum)i).ToString()
                });
            }
        }

        public override bool SupportsType( Type type )
        {
            return type == typeof( Atom.StartOn );
        }

        public void ShowHideListenDD()
        {
            if (StartOn.value == (int) DestroyOnEnum.BroadcastListen )
            {
                if (!ListenOn.gameObject.activeSelf)
                {
                    ListenOn.gameObject.SetActive(true);
                }
            }
            else if(ListenOn.gameObject.activeSelf)
            {
                ListenOn.gameObject.SetActive(false);
            }
        }
        
        private void OnStartValueChanged(int _index)
        {
            if(Inspector)  Inspector.RefreshDelayed();
            ShowHideListenDD();
            Atom.StartOn atom = (Atom.StartOn)Value;
            atom.data.startIndex = _index;
            atom.data.startName = Enum.ToObject(typeof(DestroyOnEnum), _index).ToString();
            UpdateData(atom);
        }

        private void OnListenValueChanged(int _index)
        {
            if(Inspector)  Inspector.RefreshDelayed();
            Atom.StartOn atom = (Atom.StartOn)Value;
            atom.data.listenName = Helper.GetListenToTypes()[_index];
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
            List<GameObject> selectedObjects = EditorOp.Resolve<SelectionHandler>().GetSelectedObjects();
            foreach (var obj in selectedObjects)
            {
                foreach (Atom.StartOn atom in Atom.StartOn.AllInstances)
                {
                    if (obj.GetInstanceID() == atom.target.GetInstanceID())
                    {
                        atom.data = Helper.DeepCopy(_atom.data);
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
                StartOn.value = atom.data.startIndex;
                ListenOn.value = atom.data.listenIndex;
                // ShowHideListenDD();
            }
        }
    }
}