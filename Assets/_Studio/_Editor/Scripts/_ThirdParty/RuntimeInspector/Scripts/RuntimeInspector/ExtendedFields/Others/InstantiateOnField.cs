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
        private InstantiateOnData unboxedData;
        private GameObject currentPointInstance;
        private GameObject ghostOptionInstance;
        private Text currentPointTRS;
        private bool currentRecordState;

        public override bool SupportsType(Type type)
        {
            return type == SupportedType;
        }

        public override void GenerateElements()
        {
            spawnWhenField = CreateDrawerForField(nameof(unboxedData.spawnWhen));
            GenerateEveryXSecondsDrawer();
            spawnWhereField = CreateDrawer(typeof(SpawnWhere), "Spawn Where", () => { return unboxedData.spawnWhere; }, OnSpawnWhereUpdated, true);
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
            if (unboxedData.spawnWhere == newValue) return;
            unboxedData.spawnWhere = newValue;
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
            if (spawnWhenField) spawnWhenField.SetInteractable(status);
            if (spawnWhereField) spawnWhereField.SetInteractable(status);
            if (intervalField) intervalField.SetInteractable(status);
            if (repeatForeverField) repeatForeverField.SetInteractable(status);
            if (howManyField) howManyField.SetInteractable(status);
            if (roundsField && !unboxedData.repeatForever) roundsField.SetInteractable(status);
        }

        private void GenerateEveryXSecondsDrawer()
        {
            if (unboxedData.instantiateOn != InstantiateOn.EveryXSeconds) return;
            intervalField = CreateDrawer(typeof(int), "Interval", () => { return unboxedData.interval; }, (value) => { unboxedData.interval = (int)value; }, true);
            roundsField = CreateDrawer(typeof(uint), "Rounds", () => { return unboxedData.rounds; }, (value) => { unboxedData.rounds = (uint)value; }, true);
            repeatForeverField = CreateDrawer(typeof(bool), "Repeat Forever", () => { return unboxedData.repeatForever; }, OnRepeatForeverChecked, true);
            OnRepeatForeverChecked(unboxedData.repeatForever);
        }

        private void OnRepeatForeverChecked(object value)
        {
            unboxedData.repeatForever = (bool)value;
            var setInteractable = !unboxedData.repeatForever;
            roundsField.SetInteractable(setInteractable);
        }

        private void GenerateHowManyDrawer()
        {
            if (unboxedData.spawnWhere != SpawnWhere.Random) return;
            howManyField = CreateDrawer(typeof(uint), "How Many", () => { return unboxedData.howMany; }, (value) => { unboxedData.howMany = (uint)value; }, true);
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
            UpdateTRS();
        }

        private void UpdateTRS()
        {
            if (unboxedData == null || unboxedData.spawnWhere == SpawnWhere.Random || !currentPointTRS) return;
            var target = unboxedData.target.transform.localPosition;
            currentPointTRS.text = $"X: {Math.Round(target.x, 2)}    Y: {Math.Round(target.y, 2)}    Z: {Math.Round(target.z, 2)}";
        }
    }
}