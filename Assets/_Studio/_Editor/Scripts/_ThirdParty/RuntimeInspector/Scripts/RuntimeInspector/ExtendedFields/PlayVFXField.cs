using System;
using UnityEngine;
using Terra.Studio;
using PlayShifu.Terra;
using System.Collections.Generic;

namespace RuntimeInspectorNamespace
{
    public class PlayVFXField : BasePlayFXField
    {
        protected override string CommentKey => "VFX";
        protected override Type DerivedType => typeof(Atom.PlayVfx);

        public override bool SupportsType(Type type)
        {
            return type == DerivedType;
        }

        protected override string[] GetAllClipNames()
        {
            return Helper.GetVfxClipNames();
        }

        protected override string GetClipNameByIndex(int index)
        {
            return Helper.GetVfxClipNameByIndex(index);
        }

        protected override void OnToggleValueSubmitted(bool _input)
        {
            base.OnToggleValueSubmitted(_input);
            var _vfx = (Atom.PlayVfx)Value;
            UpdateData();
        }

        protected override void OnDropdownValueSubmitted(int index)
        {
            base.OnDropdownValueSubmitted(index);
            UpdateData();
        }

        protected override void UpdateData()
        {
            var _vfx = (Atom.PlayVfx)Value;
            List<GameObject> selectedObjects = EditorOp.Resolve<SelectionHandler>().GetSelectedObjects();
            if (selectedObjects.Count <= 1) return;
            foreach (var obj in selectedObjects)
            {
                var allInstances = EditorOp.Resolve<Atom>().AllVfxes;
                foreach (Atom.PlayVfx vfx in allInstances)
                {
                    if (obj.GetInstanceID() == vfx.target.GetInstanceID())
                    {
                        if (!string.IsNullOrEmpty(_vfx.fieldName) &&
                            !string.IsNullOrEmpty(vfx.fieldName) &&
                            !_vfx.fieldName.Equals(vfx.fieldName))
                        {
                            continue;
                        }
                        if (_vfx != null && _vfx.componentType != null && _vfx.componentType == vfx.componentType)
                        {
                            vfx.data = Helper.DeepCopy(_vfx.data);
                        }
                    }
                }
            }
        }
    }
}