using System;
using UnityEngine;
using Newtonsoft.Json;

namespace Terra.Studio
{
    [Serializable]
    public struct OscillateComponent : IBaseComponent
    {
        public float speed;
        [JsonConverter(typeof(Vector3Converter))] public Vector3 fromPoint;
        [JsonConverter(typeof(Vector3Converter))] public Vector3 toPoint;
        public bool loop;
        public int listenIndex;
        public string ConditionType { get; set; }
        public string ConditionData { get; set; }
        public bool IsConditionAvailable { get; set; }
        public bool IsBroadcastable { get; set; }
        public string Broadcast { get; set; }
        public string BroadcastListen { get; set; }
        public bool IsTargeted { get; set; }
        public int TargetId { get; set; }
        [JsonIgnore] public bool CanExecute { get; set; }
        [JsonIgnore] public bool IsExecuted { get; set; }
        [JsonIgnore] public EventContext EventContext { get; set; }
        [JsonIgnore] public GameObject RefObj { get; set; }
    }
}
