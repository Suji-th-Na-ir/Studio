using System;
using UnityEngine;
using UnityEngine.UI;
using System.Reflection;
using static Terra.Studio.Atom;
using RuntimeInspectorNamespace;

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

        public override bool SupportsType(Type type)
        {
            return type == SupportedType;
        }

        public override void GenerateElements()
        {
            spawnWhenField = CreateDrawerForField(nameof(unboxedData.spawnWhen));
            GenerateEveryXSecondsDrawer();
            spawnWhereField = CreateDrawer(typeof(SpawnWhere), "Where", () => { return unboxedData.spawnWhere; }, OnSpawnWhereUpdated, true);
            SpawnDynamicUI();
            GenerateHowManyDrawer();
            ToggleNativeStackingIntoIUR(false);
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
            if (unboxedData.spawnWhere == newValue) return;
            PostProcessDataUpdate(UpdateType.SpawnWhere, unboxedData.spawnWhere, value);
            if (unboxedData.spawnWhere != SpawnWhere.Random)
            {
                if (howManyField != null)
                {
                    ClearElement(howManyField);
                    howManyField = null;
                }
            }
            RegenerateElements();
        }

        private void SpawnDynamicUI()
        {
            ClearDynamicUI();
            if (unboxedData.spawnWhere == SpawnWhere.CurrentPoint)
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
            unboxedData.OnRecordToggled?.Invoke(currentRecordState);
            ToggleInteractivityOfAllFields(!currentRecordState);
            if (currentRecordState)
            {
                EditorOp.Resolve<ToolbarView>().ToggleInteractionOfGroup("GizmoToolGroup", true);
            }
        }

        private void ToggleInteractivityOfAllFields(bool status)
        {
            if (spawnWhereField) spawnWhereField.SetInteractable(status);
            if (intervalField) intervalField.SetInteractable(status);
            if (repeatForeverField) repeatForeverField.SetInteractable(status);
            if (howManyField) howManyField.SetInteractable(status);
            if (roundsField && !unboxedData.repeatForever) roundsField.SetInteractable(status);
        }

        private void GenerateEveryXSecondsDrawer()
        {
            if (unboxedData.instantiateOn != InstantiateOn.EveryXSeconds) return;
            intervalField = CreateDrawer(typeof(int), "Interval", () => { return unboxedData.interval; }, (value) => { PostProcessDataUpdate(UpdateType.Interval, unboxedData.interval, value); }, true);
            roundsField = CreateDrawer(typeof(uint), "Rounds", () => { return unboxedData.rounds; }, (value) => { PostProcessDataUpdate(UpdateType.Rounds, unboxedData.rounds, value); }, true);
            repeatForeverField = CreateDrawer(typeof(bool), "Forever", () => { return unboxedData.repeatForever; }, OnRepeatForeverChecked, true);
            OnRepeatForeverChecked(unboxedData.repeatForever);
        }

        private void OnRepeatForeverChecked(object value)
        {
            PostProcessDataUpdate(UpdateType.RepeatForever, unboxedData.repeatForever, value);
            var setInteractable = !unboxedData.repeatForever;
            roundsField.SetInteractable(setInteractable);
        }

        private void GenerateHowManyDrawer()
        {
            if (unboxedData.spawnWhere != SpawnWhere.Random) return;
            howManyField = CreateDrawer(typeof(uint), "Count", () => { return unboxedData.howMany; }, (value) => { PostProcessDataUpdate(UpdateType.HowMany, unboxedData.howMany, value); }, true);
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
            ToggleNativeStackingIntoIUR(true);
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
            if (unboxedData == null || unboxedData.spawnWhere == SpawnWhere.Random || !currentPointTRS) return;
            var target = unboxedData.target.transform.localPosition;
            currentPointTRS.text = $"X: {Math.Round(target.x, 2)}    Y: {Math.Round(target.y, 2)}    Z: {Math.Round(target.z, 2)}";
        }

        private void PostProcessDataUpdate(UpdateType updateType, object oldValue, object newValue)
        {
            if (oldValue != null && oldValue.Equals(newValue)) return;
            StackIntoURStack(updateType, oldValue, newValue);
            UpdateMultiselections(updateType, newValue);
        }

        private void StackIntoURStack(UpdateType updateType, object oldValue, object newValue)
        {
            var message = $"{updateType} updated to: {newValue}";
            EditorOp.Resolve<IURCommand>().Record((updateType, oldValue), (updateType, newValue), message, (stackData) =>
            {
                var (updateType, value) = ((UpdateType, object))stackData;
                UpdateMultiselections(updateType, value, true);
            });
        }

        private void UpdateMultiselections(UpdateType updateType, object value, bool forceAllDirty = false)
        {
            var selections = EditorOp.Resolve<SelectionHandler>().GetSelectedObjects();
            foreach (var selection in selections)
            {
                var isSameObject = unboxedData.target == selection;
                UpdateValue(selection, updateType, value, !isSameObject || forceAllDirty);
            }
        }

        private void UpdateValue(GameObject gameObject, UpdateType updateType, object value, bool isDirty = true)
        {
            if (!gameObject.TryGetComponent(out InstantiateStudioObject instantiate)) return;
            var data = instantiate.instantiateData;
            switch (updateType)
            {
                case UpdateType.SpawnWhere:
                    data.spawnWhere = (SpawnWhere)value;
                    data.isDirty = isDirty;
                    break;
                case UpdateType.Interval:
                    data.interval = (int)value;
                    data.isDirty = isDirty;
                    break;
                case UpdateType.Rounds:
                    data.rounds = (uint)value;
                    data.isDirty = isDirty;
                    break;
                case UpdateType.RepeatForever:
                    data.repeatForever = (bool)value;
                    data.isDirty = isDirty;
                    break;
                case UpdateType.HowMany:
                    data.howMany = (uint)value;
                    data.isDirty = isDirty;
                    break;
            }
        }

        private void ToggleNativeStackingIntoIUR(bool status)
        {
            if (spawnWhenField) spawnWhenField.shouldPopulateIntoUndoRedoStack = status;
            if (spawnWhereField) spawnWhereField.shouldPopulateIntoUndoRedoStack = status;
            if (intervalField) intervalField.shouldPopulateIntoUndoRedoStack = status;
            if (repeatForeverField) repeatForeverField.shouldPopulateIntoUndoRedoStack = status;
            if (howManyField) howManyField.shouldPopulateIntoUndoRedoStack = status;
            if (roundsField && !unboxedData.repeatForever) roundsField.shouldPopulateIntoUndoRedoStack = status;
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