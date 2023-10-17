using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Terra.Studio;
using UnityEngine;
using UnityEngine.UI;

namespace RuntimeInspectorNamespace
{
    public class Vector3Field : InspectorField
    {
#pragma warning disable 0649
        [SerializeField]
        private BoundInputField inputX;

        [SerializeField]
        private BoundInputField inputY;

        [SerializeField]
        private BoundInputField inputZ;

        [SerializeField]
        private Text labelX;

        [SerializeField]
        private Text labelY;

        [SerializeField]
        private Text labelZ;
#pragma warning restore 0649

#if UNITY_2017_2_OR_NEWER
        private bool isVector3Int;
#endif

        private readonly string[] TRANSFORM_FIELDS = new[] { "Position", "Rotation", "Scale" };

        public override void Initialize()
        {
            base.Initialize();

            inputX.Initialize();
            inputY.Initialize();
            inputZ.Initialize();

            inputX.OnValueChanged += OnValueChanged;
            inputY.OnValueChanged += OnValueChanged;
            inputZ.OnValueChanged += OnValueChanged;

            inputX.OnValueSubmitted += OnValueSubmitted;
            inputY.OnValueSubmitted += OnValueSubmitted;
            inputZ.OnValueSubmitted += OnValueSubmitted;

            inputX.DefaultEmptyValue = "0";
            inputY.DefaultEmptyValue = "0";
            inputZ.DefaultEmptyValue = "0";
        }

        public override bool SupportsType(Type type)
        {
#if UNITY_2017_2_OR_NEWER
            if (type == typeof(Vector3Int))
                return true;
#endif
            return type == typeof(Vector3);
        }

        protected override void OnBound(MemberInfo variable)
        {
            base.OnBound(variable);
            SetValueIndirectly();

            EditorOp.Resolve<FocusFieldsSystem>().AddFocusedGameobjects(inputX.gameObject,
                ()=>inputX.BackingField.targetGraphic.color=Skin.SelectedItemBackgroundColor,
                () => inputX.BackingField.targetGraphic.color = Skin.InputFieldNormalBackgroundColor);
            EditorOp.Resolve<FocusFieldsSystem>().AddFocusedGameobjects(inputY.gameObject,
                () => inputY.BackingField.targetGraphic.color = Skin.SelectedItemBackgroundColor,
                () => inputY.BackingField.targetGraphic.color = Skin.InputFieldNormalBackgroundColor);
            EditorOp.Resolve<FocusFieldsSystem>().AddFocusedGameobjects(inputZ.gameObject,
                () => inputZ.BackingField.targetGraphic.color = Skin.SelectedItemBackgroundColor,
                () => inputZ.BackingField.targetGraphic.color = Skin.InputFieldNormalBackgroundColor);
        }

        protected override void OnUnbound()
        {
            base.OnUnbound();

            EditorOp.Resolve<FocusFieldsSystem>().RemoveFocusedGameObjects(inputX.gameObject);
            EditorOp.Resolve<FocusFieldsSystem>().RemoveFocusedGameObjects(inputY.gameObject);
            EditorOp.Resolve<FocusFieldsSystem>().RemoveFocusedGameObjects(inputZ.gameObject);
        }


        private void SetValueIndirectly()
        {
#if UNITY_2017_2_OR_NEWER
            isVector3Int = BoundVariableType == typeof(Vector3Int);
            if (isVector3Int)
            {
                Vector3Int val = (Vector3Int)Value;
                inputX.Text = val.x.ToString(RuntimeInspectorUtils.numberFormat);
                inputY.Text = val.y.ToString(RuntimeInspectorUtils.numberFormat);
                inputZ.Text = val.z.ToString(RuntimeInspectorUtils.numberFormat);
            }
            else
#endif
            {
                Vector3 val = (Vector3)Value;
                inputX.Text = val.x.ToString(RuntimeInspectorUtils.numberFormat);
                inputY.Text = val.y.ToString(RuntimeInspectorUtils.numberFormat);
                inputZ.Text = val.z.ToString(RuntimeInspectorUtils.numberFormat);
            }
        }

        protected virtual bool OnValueChanged(BoundInputField source, string input)
        {
#if UNITY_2017_2_OR_NEWER
            if (isVector3Int)
            {
                if (int.TryParse(input, NumberStyles.Integer, RuntimeInspectorUtils.numberFormat, out int value))
                {
                    Vector3Int val = (Vector3Int)Value;
                    if (source == inputX)
                        val.x = value;
                    else if (source == inputY)
                        val.y = value;
                    else
                        val.z = value;

                    Value = val;
                    return true;
                }
            }
            else
#endif
            {
                if (float.TryParse(input, NumberStyles.Float, RuntimeInspectorUtils.numberFormat, out float value))
                {
                    Vector3 val = (Vector3)Value;
                    if (source == inputX)
                        val.x = value;
                    else if (source == inputY)
                        val.y = value;
                    else
                        val.z = value;

                    Value = val;
                    EditorOp.Resolve<SelectionHandler>().RefreshGizmo();
                    return true;
                }
            }
            Inspector.RefreshDelayed();
            return false;
        }

        protected virtual bool OnValueSubmitted(BoundInputField _, string __)
        {
            HandleUndoRedo();
            return true;
        }

        protected void HandleUndoRedo()
        {
            if (Value != lastSubmittedValue)
            {
                var message = $"Vector3 changed to: {Value}";
                EditorOp.Resolve<IURCommand>()?.Record(
                    (lastSubmittedValue, Name), (Value, Name),
                    message,
                    (value) =>
                    {
                        var modValue = ((object, string))value;
                        if (TRANSFORM_FIELDS.Any(x => x.Equals(modValue.Item2)))
                        {
                            SetTRS((Vector3)modValue.Item1, modValue.Item2);
                        }
                        else
                        {
                            setter(modValue.Item1);
                            Refresh();
                        }
                        lastSubmittedValue = modValue.Item1;
                    }
                );
            }
            lastSubmittedValue = Value;
        }

        private void SetTRS(Vector3 value, string key)
        {
            var transform = (Transform)virtualObject;
            switch (key)
            {
                case "Position":
                    transform.localPosition = value;
                    break;
                case "Rotation":
                    transform.localEulerAngles = value;
                    break;
                case "Scale":
                    transform.localScale = value;
                    break;
            }
            EditorOp.Resolve<SelectionHandler>().RefreshGizmo();
        }

        protected override void OnSkinChanged()
        {
            base.OnSkinChanged();

            labelX.SetSkinText(Skin);
            labelY.SetSkinText(Skin);
            labelZ.SetSkinText(Skin);

            inputX.Skin = Skin;
            inputY.Skin = Skin;
            inputZ.Skin = Skin;

            float inputFieldWidth = (1f - Skin.LabelWidthPercentage) / 3f;
            Vector2 rightSideAnchorMin = new Vector2(Skin.LabelWidthPercentage, 0f);
            Vector2 rightSideAnchorMax = new Vector2(Skin.LabelWidthPercentage + inputFieldWidth, 1f);
            if(variableNameMask)
                variableNameMask.rectTransform.anchorMin = rightSideAnchorMin;
            ((RectTransform)inputX.transform).SetAnchorMinMaxInputField(labelX.rectTransform, rightSideAnchorMin, rightSideAnchorMax);

            rightSideAnchorMin.x += inputFieldWidth;
            rightSideAnchorMax.x += inputFieldWidth;
            ((RectTransform)inputY.transform).SetAnchorMinMaxInputField(labelY.rectTransform, rightSideAnchorMin, rightSideAnchorMax);

            rightSideAnchorMin.x += inputFieldWidth;
            rightSideAnchorMax.x = 1f;
            ((RectTransform)inputZ.transform).SetAnchorMinMaxInputField(labelZ.rectTransform, rightSideAnchorMin, rightSideAnchorMax);
        }

        public override void Refresh()
        {
#if UNITY_2017_2_OR_NEWER
            if (isVector3Int)
            {
                Vector3Int prevVal = (Vector3Int)Value;
                base.Refresh();
                Vector3Int val = (Vector3Int)Value;

                if (val.x != prevVal.x)
                    inputX.Text = val.x.ToString(RuntimeInspectorUtils.numberFormat);
                if (val.y != prevVal.y)
                    inputY.Text = val.y.ToString(RuntimeInspectorUtils.numberFormat);
                if (val.z != prevVal.z)
                    inputZ.Text = val.z.ToString(RuntimeInspectorUtils.numberFormat);
            }
            else
#endif
            {
                Vector3 prevVal = (Vector3)Value;
                base.Refresh();
                Vector3 val = (Vector3)Value;

                if (val.x != prevVal.x)
                    inputX.Text = val.x.ToString(RuntimeInspectorUtils.numberFormat);
                if (val.y != prevVal.y)
                    inputY.Text = val.y.ToString(RuntimeInspectorUtils.numberFormat);
                if (val.z != prevVal.z)
                    inputZ.Text = val.z.ToString(RuntimeInspectorUtils.numberFormat);
            }
        }

        public override void SetInteractable(bool on)
        {
            inputX.BackingField.interactable = on;
            inputY.BackingField.interactable = on;
            inputZ.BackingField.interactable = on;
        }
    }
}