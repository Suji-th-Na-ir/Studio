using System;
using UnityEngine;
using Newtonsoft.Json;

namespace Terra.Studio
{
    [Serializable]
    public struct TranslateComponent : IBaseComponent
    {
        public bool IsConditionAvailable { get; set; }
        public string ConditionType { get; set; }
        public string ConditionData { get; set; }
        public bool IsBroadcastable { get; set; }
        public string Broadcast { get; set; }
        public string BroadcastListen { get; set; }
        public bool IsTargeted { get; set; }
        public int TargetId { get; set; }

        [JsonIgnore] public bool CanExecute { get; set; }
        [JsonIgnore] public bool IsExecuted { get; set; }
        [JsonIgnore] public EventContext EventContext { get; set; }
        [JsonIgnore] public GameObject RefObj { get; set; }

        public bool canPlaySFX;
        public string sfxName;
        public int sfxIndex;
        public bool canPlayVFX;
        public string vfxName;
        public int vfxIndex;
        public int listenIndex;
        public int broadcastTypeIndex;

        [JsonConverter(typeof(Vector3Converter))] public Vector3 startPosition;
        [JsonConverter(typeof(Vector3Converter))] public Vector3 targetPosition;
        public TranslateType translateType;
        public BroadcastAt broadcastAt;
        public Listen listen;
        public float speed;
        public float pauseFor;
        public int repeatFor;
        [JsonIgnore] public float pauseDistance;
        [JsonIgnore] public Vector3 direction;
    }
}