using System;
using UnityEngine;
using System.Linq;
using System.Reflection;
using static Terra.Studio.Atom;
using RuntimeInspectorNamespace;

namespace Terra.Studio
{
    public class InstantiateOnField : ExpandableInspectorField
    {
        private FieldInfo[] allFields;
        protected FieldInfo[] FieldInfos
        {
            get
            {
                if (allFields == null)
                {
                    var fields = SupportedType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    allFields = fields.Where(field => !field.IsDefined(typeof(HideInInspector), false)).ToArray();
                }
                return allFields;
            }
        }

        protected override int Length { get { return 1; } }

        private readonly Type SupportedType = typeof(InstantiateOnData);
        private InspectorField spawnWhenField;
        private InspectorField listenToField;
        private InspectorField intervalField;
        private InspectorField roundsField;

        public override bool SupportsType(Type type)
        {
            return type == SupportedType;
        }

        public override void GenerateElements()
        {
            var value = (InstantiateOnData)Value;
            spawnWhenField = CreateDrawer(typeof(InstantiateOn), nameof(value.spawnWhen), () => { return value.spawnWhen; }, ValidateSpawnWhen, true);
            //spawnWhenField = CreateDrawerForField(nameof(value.spawnWhen));
            spawnWhenField.OnValueUpdated += ValidateSpawnWhen;
            GenerateSpawnWhenElements();
            IsExpanded = true;
        }

        private void ValidateSpawnWhen(object value)
        {
            Debug.Log($"Value received for spawn when is: {value}");
            var instantiateOnData = (InstantiateOnData)Value;
            var spawnWhen = (InstantiateOn)value;
            if (instantiateOnData.spawnWhen == spawnWhen) return;
            instantiateOnData.spawnWhen = spawnWhen;
            RegenerateElements();
        }

        private void GenerateSpawnWhenElements()
        {
            var instantiateOnData = (InstantiateOnData)Value;
            switch (instantiateOnData.spawnWhen)
            {
                default:
                case InstantiateOn.GameStart:
                    return;
                case InstantiateOn.BroadcastListen:
                    instantiateOnData.listenToStrings = SystemOp.Resolve<CrossSceneDataHolder>().BroadcastStrings;
                    listenToField = CreateDrawerForField(nameof(instantiateOnData.listenToStrings));
                    listenToField.OnValueUpdated += OnBroadcastListenUpdated;
                    return;
                case InstantiateOn.EveryXSeconds:
                    intervalField = CreateDrawerForField(nameof(instantiateOnData.interval));
                    intervalField.OnValueUpdated += OnIntervalValueUpdated;
                    roundsField = CreateDrawerForField(nameof(instantiateOnData.rounds));
                    roundsField.OnValueUpdated += OnRoundsValueUpdated;
                    break;
            }
        }

        private void OnBroadcastListenUpdated(object value)
        {
            Debug.Log($"Value received for broadcast listen is: {value}");
        }

        private void OnIntervalValueUpdated(object value)
        {
            Debug.Log($"Value received for interval update is: {value}");
        }

        private void OnRoundsValueUpdated(object value)
        {
            Debug.Log($"Value received for rounds update is: {value}");
        }

        protected override void OnUnbound()
        {
            base.OnUnbound();
            spawnWhenField.OnValueUpdated -= ValidateSpawnWhen;
        }
    }
}