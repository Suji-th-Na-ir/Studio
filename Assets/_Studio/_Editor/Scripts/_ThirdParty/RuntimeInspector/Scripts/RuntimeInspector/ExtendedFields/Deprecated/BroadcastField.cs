using System;
using Terra.Studio;
using System.Reflection;
using UnityEngine.UI;
using UnityEngine;

namespace RuntimeInspectorNamespace
{
    public class BroadcastField : ExpandableInspectorField
    {
        [SerializeField] Dropdown broadcastDropdownDrawer;
        InspectorField BroadcastFieldDrawer;
        private bool didCheckForExpand;
        protected override int Length { get { return 2; } }
        public override void Initialize()
        {
            base.Initialize();
        }

        public override bool SupportsType(Type type)
        {
            return type == typeof(Atom.Broadcast);
        }

        protected override void OnBound(MemberInfo variable)
        {
            base.OnBound(variable);
        }

        public override void Refresh()
        {
            if (!didCheckForExpand && Value != null)
            {
                didCheckForExpand = true;
                IsExpanded = true;
            }
            base.Refresh();
        }

        protected override void ClearElements()
        {
            base.ClearElements();
            didCheckForExpand = false;
        }

        public override void GenerateElements()
        {
            var val = (Atom.Broadcast)Value;
            BroadcastFieldDrawer = CreateDrawerForField(nameof(val.broadcast));
            BroadcastFieldDrawer.OnValueUpdated += OnBroadcastValueChanged;
        }

        private void OnBroadcastValueChanged(object obj)
        {
           
        }
    }
}
