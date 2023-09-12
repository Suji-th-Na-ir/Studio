using System;
using UnityEngine;
using Terra.Studio;
using PlayShifu.Terra;
using System.Collections.Generic;

namespace RuntimeInspectorNamespace
{
    public class PlaySFXField : BasePlayFXField
    {
        protected override string CommentKey => "SFX";

        public override bool SupportsType(Type type)
        {
            return type == typeof(Atom.PlaySfx);
        }

        protected override string[] GetAllClipNames()
        {
            return Helper.GetSfxClipNames();
        }

        protected override string GetClipNameByIndex(int index)
        {
            return Helper.GetSfxClipNameByIndex(index);
        }

        protected override void OnToggleValueSubmitted(bool _input)
        {
            base.OnToggleValueSubmitted(_input);
            var _sfx = (Atom.PlaySfx)Value;
            UpdateData(_sfx);
        }

        protected override void OnDropdownValueSubmitted(int index)
        {
            base.OnDropdownValueSubmitted(index);
            var _sfx = (Atom.PlaySfx)Value;
            UpdateData(_sfx);
        }

        private void UpdateData(Atom.PlaySfx _sfx)
        {
            if (_sfx == null)
            {
                return;
            }
            List<GameObject> selectedObjects = EditorOp.Resolve<SelectionHandler>().GetSelectedObjects();
            if (selectedObjects.Count <= 1) return;
            foreach (var obj in selectedObjects)
            {
                foreach (Atom.PlaySfx sfx in Atom.PlaySfx.AllInstances)
                {
                    if (obj.GetInstanceID() == sfx.target.GetInstanceID())
                    {
                        if (!string.IsNullOrEmpty(_sfx.fieldName) &&
                            !string.IsNullOrEmpty(sfx.fieldName) &&
                            !_sfx.fieldName.Equals(sfx.fieldName))
                        {
                            continue;
                        }
                        if (_sfx.componentType != null && _sfx.componentType == sfx.componentType)
                        {
                            sfx.data = Helper.DeepCopy(_sfx.data);
                        }
                    }
                }
            }
        }
    }
}