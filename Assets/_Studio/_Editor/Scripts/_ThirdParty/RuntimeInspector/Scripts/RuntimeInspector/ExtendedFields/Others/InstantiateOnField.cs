using System;
using UnityEngine;
using UnityEngine.UI;
using System.Reflection;
using static Terra.Studio.Atom;
using RuntimeInspectorNamespace;
using UnityEditor;

namespace Terra.Studio
{
    public class InstantiateOnField : ExpandableInspectorField
    {
        [SerializeField] private GameObject currentPoint;
        [SerializeField] private GameObject ghostOption;

        private readonly Type SupportedType = typeof(InstantiateOnData);
        private InspectorField spawnWhenField;
        private InspectorField spawnWhereField;
        private InspectorField intervalField;
        private InspectorField roundsField;
        private InspectorField repeatForeverField;
        private InspectorField howManyField;
        private GameObject currentPointInstance;
        private GameObject ghostOptionInstance;
        private Text currentPointTRS;
        private bool currentRecordState;
        private InstantiateOnData unboxedData;
        private InstantiateOnData UnboxedData => unboxedData;

        public override bool SupportsType(Type type)
        {
            return type == SupportedType;
        }

        public override void GenerateElements()
        {
            spawnWhenField = CreateDrawerForField(nameof(UnboxedData.spawnWhen));
            GenerateEveryXSecondsDrawer();
            spawnWhereField = CreateDrawer(typeof(SpawnWhere), "Spawn Where", () => { return UnboxedData.spawnWhere; }, OnSpawnWhereUpdated, true);
            SpawnDynamicUI();
            GenerateHowManyDrawer();
            IsExpanded = true;
        }

        private void OnSpawnWhenUpdated(InstantiateOn instantiateOn)
        {
            if (instantiateOn != InstantiateOn.EveryXSeconds)
            {
                if (intervalField != null || roundsField != null || repeatForeverField != null)
                {
                    ClearElement(intervalField);
                    ClearElement(roundsField);
                    ClearElement(repeatForeverField);
                    intervalField = roundsField = repeatForeverField = null;
                    Refresh();
                }
                return;
            }
            else
            {
                RegenerateElements();
            }
        }

        private void OnSpawnWhereUpdated(object value)
        {
            var newValue = (SpawnWhere)value;
            if (UnboxedData.spawnWhere == newValue) return;
            UnboxedData.spawnWhere = newValue;
            if (UnboxedData.spawnWhere != SpawnWhere.Random)
            {
                if (howManyField != null)
                {
                    ClearElement(howManyField);
                    howManyField = null;
                }
            }
            PostProcessDataUpdate(UpdateType.SpawnWhere, value);
            RegenerateElements();
        }

        private void SpawnDynamicUI()
        {
            ClearDynamicUI();
            if (UnboxedData.spawnWhere == SpawnWhere.CurrentPoint)
            {
                currentPointInstance = Instantiate(currentPoint, drawArea.transform);
                currentPointTRS = currentPointInstance.GetComponent<Text>();
            }
            else
            {
                ghostOptionInstance = Instantiate(ghostOption, drawArea.transform);
                var button = ghostOptionInstance.GetComponent<Button>();
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(OnRecordToggled);
            }
        }

        private void ClearDynamicUI()
        {
            if (ghostOptionInstance) Destroy(ghostOptionInstance);
            if (currentPointInstance) Destroy(currentPointInstance);
        }

        private void OnRecordToggled()
        {
            currentRecordState = !currentRecordState;
            UnboxedData.OnRecordToggled?.Invoke(currentRecordState);
            ToggleInteractivityOfAllFields(!currentRecordState);
            if (currentRecordState)
            {
                EditorOp.Resolve<ToolbarView>().ToggleInteractionOfGroup("GizmoToolGroup", true);
            }
        }

        private void ToggleInteractivityOfAllFields(bool status)
        {
            if (spawnWhenField) spawnWhenField.SetInteractable(status);
            if (spawnWhereField) spawnWhereField.SetInteractable(status);
            if (intervalField) intervalField.SetInteractable(status);
            if (repeatForeverField) repeatForeverField.SetInteractable(status);
            if (howManyField) howManyField.SetInteractable(status);
            if (roundsField && !UnboxedData.repeatForever) roundsField.SetInteractable(status);
        }

        private void GenerateEveryXSecondsDrawer()
        {
            if (UnboxedData.instantiateOn != InstantiateOn.EveryXSeconds) return;
            intervalField = CreateDrawer(typeof(int), "Interval", () => { return UnboxedData.interval; }, (value) => { UnboxedData.interval = (int)value; PostProcessDataUpdate(UpdateType.Interval, value); }, true);
            roundsField = CreateDrawer(typeof(uint), "Rounds", () => { return UnboxedData.rounds; }, (value) => { UnboxedData.rounds = (uint)value; PostProcessDataUpdate(UpdateType.Rounds, value); }, true);
            repeatForeverField = CreateDrawer(typeof(bool), "Repeat Forever", () => { return UnboxedData.repeatForever; }, OnRepeatForeverChecked, true);
            OnRepeatForeverChecked(UnboxedData.repeatForever);
        }

        private void OnRepeatForeverChecked(object value)
        {
            UnboxedData.repeatForever = (bool)value;
            var setInteractable = !UnboxedData.repeatForever;
            roundsField.SetInteractable(setInteractable);
            PostProcessDataUpdate(UpdateType.RepeatForever, value);
        }

        private void GenerateHowManyDrawer()
        {
            if (UnboxedData.spawnWhere != SpawnWhere.Random) return;
            howManyField = CreateDrawer(typeof(uint), "How Many", () => { return UnboxedData.howMany; }, (value) => { UnboxedData.howMany = (uint)value; PostProcessDataUpdate(UpdateType.HowMany, value); }, true);
        }

        protected override void OnBound(MemberInfo variable)
        {
            base.OnBound(variable);
            var data = (InstantiateOnData)Value;
            data.OnSpawnWhenUpdated += OnSpawnWhenUpdated;
            unboxedData = data;
        }

        protected override void OnUnbound()
        {
            base.OnUnbound();
            if (Value != null || Value is not InstantiateOnData) return;
            ClearDynamicUI();
            var data = (InstantiateOnData)Value;
            data.OnSpawnWhenUpdated -= OnSpawnWhenUpdated;
            unboxedData = null;
        }

        public override void Refresh()
        {
            base.Refresh();
            if (unboxedData != null)
            {
                if (unboxedData.isDirty)
                {
                    unboxedData.isDirty = false;
                    RegenerateElements();
                }
                else
                {
                    UpdateTRS();
                }
            }
        }

        private void UpdateTRS()
        {
            if (UnboxedData == null || UnboxedData.spawnWhere == SpawnWhere.Random || !currentPointTRS) return;
            var target = UnboxedData.target.transform.localPosition;
            currentPointTRS.text = $"X: {Math.Round(target.x, 2)}    Y: {Math.Round(target.y, 2)}    Z: {Math.Round(target.z, 2)}";
        }

        private void PostProcessDataUpdate(UpdateType updateType, object value)
        {
            var selections = EditorOp.Resolve<SelectionHandler>().GetSelectedObjects();
            if (selections.Count <= 1) return;
            foreach (var selection in selections)
            {
                if (selection == unboxedData.target) continue;
                UpdateValue(selection, updateType, value);
            }
        }

        private void UpdateValue(GameObject gameObject, UpdateType updateType, object value)
        {
            if (!gameObject.TryGetComponent(out InstantiateStudioObject instantiate)) return;
            var data = instantiate.instantiateData;
            switch (updateType)
            {
                case UpdateType.SpawnWhere:
                    data.spawnWhere = (SpawnWhere)value;
                    data.isDirty = true;
                    break;
                case UpdateType.Interval:
                    data.interval = (int)value;
                    data.isDirty = true;
                    break;
                case UpdateType.Rounds:
                    data.rounds = (uint)value;
                    data.isDirty = true;
                    break;
                case UpdateType.RepeatForever:
                    data.repeatForever = (bool)value;
                    data.isDirty = true;
                    break;
                case UpdateType.HowMany:
                    data.howMany = (uint)value;
                    data.isDirty = true;
                    break;
            }
        }

        private enum UpdateType
        {
            SpawnWhere,
            Interval,
            Rounds,
            RepeatForever,
            HowMany
        }
    }
}