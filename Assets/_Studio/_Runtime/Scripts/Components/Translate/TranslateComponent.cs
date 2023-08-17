using System;
using UnityEngine;
using Newtonsoft.Json;

namespace Terra.Studio
{
    [Serializable]
    public struct TranslateComponent : IBaseComponent, IConditional, IBroadcastData
    {
        public bool IsConditionAvailable { get; set; }
        public string ConditionType { get; set; }
        public string ConditionData { get; set; }
        public bool IsBroadcastable { get; set; }
        public string Broadcast { get; set; }
        public bool IsTargeted { get; set; }
        public int TargetId { get; set; }

        [JsonIgnore] public bool CanExecute { get; set; }
        [JsonIgnore] public bool IsExecuted { get; set; }
        [JsonIgnore] public GameObject refObj;

        public bool canPlaySFX;
        public string sfxName;
        public int sfxIndex;
        public bool canPlayVFX;
        public string vfxName;
        public int vfxIndex;

        [JsonConverter(typeof(Vector3Converter))] public Vector3 startPosition;
        [JsonConverter(typeof(Vector3Converter))] public Vector3 targetPosition;
        public TranslateType translateType;
        public BroadcastAt broadcastAt;
        public float speed;
        public float pauseFor;
        public int repeatFor;
    }
}