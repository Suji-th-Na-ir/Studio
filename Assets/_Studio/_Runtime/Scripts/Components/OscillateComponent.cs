using System;
using UnityEngine;
using Newtonsoft.Json;

namespace Terra.Studio
{
    [Serializable]
    public struct OscillateComponent : IBaseComponent, IConditional, IBroadcastData
    {
        public float speed;
        [JsonConverter(typeof(Vector3Converter))] public Vector3 fromPoint;
        [JsonConverter(typeof(Vector3Converter))] public Vector3 toPoint;
        public bool loop;
        public string ConditionType { get; set; }
        public string ConditionData { get; set; }
        public bool IsConditionAvailable { get; set; }
        public bool IsBroadcastable { get; set; }
        public string Broadcast { get; set; }
        [HideInInspector] public bool CanExecute { get; set; }
        [HideInInspector] public bool IsTargeted { get; set; }
        [HideInInspector] public int TargetId { get; set; }
        [HideInInspector] public bool isRegistered;
        [HideInInspector] public Transform oscillatableTr;
        [HideInInspector] public bool IsExecuted { get; set; }
    }
}
