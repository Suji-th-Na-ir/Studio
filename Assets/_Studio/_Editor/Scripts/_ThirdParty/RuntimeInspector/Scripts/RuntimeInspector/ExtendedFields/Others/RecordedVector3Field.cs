using System;
using UnityEngine;
using UnityEngine.UI;
using System.Reflection;
using static Terra.Studio.Atom;
using RuntimeInspectorNamespace;

namespace Terra.Studio
{
    public class RecordedVector3Field : Vector3Field, ITooltipContent, ITooltipManager
    {
        [SerializeField] private Button recordButton;
        [SerializeField] private Button resetButton;
        [SerializeField] private Image recordImageHolder;
        [SerializeField] private Sprite recordImage;
        [SerializeField] private Sprite saveImage;

        private bool isRecording;

        bool ITooltipContent.IsActive { get { return this && gameObject.activeSelf; } }
        string ITooltipContent.TooltipText => "Record";

        public Canvas Canvas => Inspector.Canvas;

        public float TooltipDelay => 0.4f;

        public override void Initialize()
        {
            base.Initialize();
            var listner = gameObject.AddComponent<TooltipListener>();
            listner.Initialize(this);
            recordButton.gameObject.AddComponent<TooltipArea>().Initialize(listner, this);
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
            AddButtonListener(resetButton, OnResetButtonClicked);
            AddButtonListener(recordButton, OnRecordButtonClicked);
            recordedObj.OnModified = ToggleInteractivityOfResetButton;
            CheckAndToggleInteractivityOfResetButton();
        }

        protected override bool OnValueChanged(BoundInputField source, string input)
        {
            var isModified = base.OnValueChanged(source, input);
            if (isModified)
            {
                var vector3 = (Vector3)Value;
                var recordedObj = (RecordedVector3)ObscuredValue;
                recordedObj.UpdateGhostTrs?.Invoke();
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
                    recordedField.UpdateGhostTrs?.Invoke();
                }
            }
        }

        private void AddButtonListener(Button button, Action action)
        {
            if (!button) return;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => action?.Invoke());
        }

        private void OnRecordButtonClicked()
        {
            var recordedObj = (RecordedVector3)ObscuredValue;
            recordedObj.ToggleRecordMode?.Invoke();
            isRecording = !isRecording;
            if (isRecording)
            {
                recordImageHolder.sprite = saveImage;
                SetInteractable(false);
            }
            else
            {
                recordImageHolder.sprite = recordImage;
                SetInteractable(true);
            }
            CheckAndToggleInteractivityOfResetButton();
        }

        private void OnResetButtonClicked()
        {
            var recordedObj = (RecordedVector3)ObscuredValue;
            recordedObj.Reset();
            ToggleInteractivityOfResetButton(false);
        }

        private void CheckAndToggleInteractivityOfResetButton()
        {
            var recordedObj = (RecordedVector3)ObscuredValue;
            var isInteractive = recordedObj?.IsValueModified?.Invoke() ?? false;
            ToggleInteractivityOfResetButton(isInteractive);
        }

        private void ToggleInteractivityOfResetButton(bool isInteractable)
        {
            resetButton.interactable = isInteractable;
        }
    }
}
