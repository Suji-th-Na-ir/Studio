using System;
using Terra.Studio;
using UnityEngine;
using System.Reflection;
using System.Linq;

namespace RuntimeInspectorNamespace
{
    public class TranslateField : ExpandableInspectorField
    {
        private FieldInfo[] allFields;
        protected FieldInfo[] FieldInfos
        {
            get
            {
                if (allFields == null)
                {
                    Type type = typeof(Atom.Translate);
                    var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    // Filter out fields with the HideInInspector attribute
                    allFields = fields.Where(field => !field.IsDefined(typeof(HideInInspector), false)).ToArray();
                }
                return allFields;
            }
        }

        protected override int Length { get { return FieldInfos.Length; } }
        private bool didCheckForExpand;

        public override void Initialize()
        {
            base.Initialize();
        }

        public override bool SupportsType(Type type)
        {
            return type == typeof(Atom.Translate);
        }

        protected override void GenerateElements()
        {
            for (int i = 0; i < allFields.Length; i++)
            {
                CreateDrawerForVariable(allFields[i], allFields[i].Name,true);
            }
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

        public override void SetInteractable(bool on)
        {
           
        }
    }
}
