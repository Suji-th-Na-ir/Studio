using System;
using UnityEngine;
using UnityEngine.UI;
using System.Reflection;
using static Terra.Studio.Atom;
using RuntimeInspectorNamespace;

namespace Terra.Studio
{
    public class RecordedVector3Field : Vector3Field
    {
        [SerializeField] private Button recordButton;
        [SerializeField] private Button resetButton;

        public override void Initialize()
        {
            base.Initialize();
        }

        public override bool SupportsType(Type type)
        {
            return type == typeof(RecordedVector3);
        }

        protected override void OnBound(MemberInfo variable)
        {
            BoundVariableType = ObscuredType;
            base.OnBound(variable);
            var recordedObj = (RecordedVector3)ObscuredValue;
            AddButtonListener(recordButton, recordedObj.ToggleGhostMode);
            AddButtonListener(resetButton, recordedObj.Reset);
        }

        protected override bool OnValueChanged(BoundInputField source, string input)
        {
            var isModified = base.OnValueChanged(source, input);
            if (isModified)
            {
                var vector3 = (Vector3)Value;
                InvokeDataChange(vector3);
            }
            return isModified;
        }

        protected override bool OnValueSubmitted(BoundInputField source, string input)
        {
            return base.OnValueSubmitted(source, input);
        }

        private void InvokeDataChange(Vector3 vector3)
        {
            var selections = EditorOp.Resolve<SelectionHandler>().GetSelectedObjects();
            var isMultiSelected = selections.Count > 1;
            if (!isMultiSelected) return;
            foreach (var selection in selections)
            {
                if (selection.TryGetComponent(Obscurer.DeclaredType, out var component))
                {
                    var baseComponent = (BaseBehaviour)component;
                    var recordedField = baseComponent.RecordedVector3;
                    recordedField?.Set(vector3);
                }
            }
        }

        private void AddButtonListener(Button button, Action action)
        {
            if (!button) return;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => action?.Invoke());
        }
    }
}
