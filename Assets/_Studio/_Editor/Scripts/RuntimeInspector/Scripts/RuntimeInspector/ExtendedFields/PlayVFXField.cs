using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Terra.Studio.RTEditor;
using TMPro;
using UnityEngine.Serialization;

namespace RuntimeInspectorNamespace
{
    public class PlayVFXField : InspectorField
    {
#pragma warning disable 0649
        [SerializeField]
        private Image toggleBackground;

        [SerializeField]
        private Toggle input;

        [SerializeField]
        private Dropdown optionsDropdown;
#pragma warning restore 0649

        private const string sfxToggleKey = "vfx_toggle";
        private const string sfxDropdownkey = "vfx_dropdown";
        private const string resourceFolder = "vfx";

        private static string[] vfxClipNames;

        public override void Initialize()
        {
            base.Initialize();
            input.onValueChanged.AddListener(OnToggleValueChanged);
            optionsDropdown.onValueChanged.AddListener(OnDropdownValueChanged);
            LoadVfxClips();
        }

        private void OnEnable()
        {
            LoadItems();
        }

        public static string GetVfxClipName(int index)
        {
            return vfxClipNames[index];
        }

        public void LoadItems()
        {
            base.StateManagerSetup();
            if (stateManager != null)
            {
                input.isOn = stateManager.GetItem<bool>(sfxToggleKey);
                optionsDropdown.value = stateManager.GetItem<int>(sfxDropdownkey);
            }
        }

        private void LoadVfxClips()
        {
            var prefabs = Resources.LoadAll(resourceFolder, typeof(GameObject)).Cast<GameObject>().ToArray();
            optionsDropdown.options.Clear();
            // foreach (var pb in prefabs)
            // {
            //     optionsDropdown.options.Add(new Dropdown.OptionData() {
            //         text =pb.name
            //     });
            // }
            vfxClipNames = new string[prefabs.Length];
            for (int i = 0; i < prefabs.Length; i++)
            {
                optionsDropdown.options.Add(new Dropdown.OptionData()
                {
                    text = prefabs[i].name
                });
                vfxClipNames[i] = prefabs[i].name;
            }
        }

        public override bool SupportsType(Type type)
        {
            return type == typeof(Atom.PlayVFX);
        }

        private void OnDropdownValueChanged(int index)
        {
            base.StateManagerSetup();
            stateManager.SetItem(sfxDropdownkey, index);
        }

        private void OnToggleValueChanged(bool input)
        {
            LoadVfxClips();
            Value = input;
            Inspector.RefreshDelayed();
            SetOptionsDropdown(input);

            base.StateManagerSetup();
            stateManager.SetItem(sfxToggleKey, input);
        }

        private void SetOptionsDropdown(bool _value)
        {
            optionsDropdown.gameObject.SetActive(_value);
        }

        protected override void OnSkinChanged()
        {
            base.OnSkinChanged();

            toggleBackground.color = Skin.InputFieldNormalBackgroundColor;
            input.graphic.color = Skin.ToggleCheckmarkColor;

            Vector2 rightSideAnchorMin = new Vector2(Skin.LabelWidthPercentage, 0f);
            variableNameMask.rectTransform.anchorMin = rightSideAnchorMin;
            ((RectTransform)input.transform).anchorMin = rightSideAnchorMin;
        }

        public override void Refresh()
        {
            base.Refresh();
            SetOptionsDropdown(input.isOn);
        }
    }
}