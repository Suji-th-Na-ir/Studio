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
        public bool IsTargeted { get; set; }
        public int TargetId { get; set; }
        public FXData FXData { get; set; }
        public Listen Listen { get; set; }

        [JsonIgnore] public bool CanExecute { get; set; }
        [JsonIgnore] public bool IsExecuted { get; set; }
        [JsonIgnore] public EventContext EventContext { get; set; }
        [JsonIgnore] public GameObject RefObj { get; set; }

        [JsonIgnore] public float pauseDistance;
        [JsonIgnore] public Vector3 direction;

        public int listenIndex;
        public int broadcastTypeIndex;
        [JsonConverter(typeof(Vector3Converter))] public Vector3 startPosition;
        [JsonConverter(typeof(Vector3Converter))] public Vector3 targetPosition;
        public RepeatDirectionType translateType;
        public BroadcastAt broadcastAt;
        public float speed;
        public float pauseFor;
        public int repeatFor;
        public bool repeatForever;
        [JsonIgnore] public Vector3 currentStartPosition;
        [JsonIgnore] public Vector3 currentTargetPosition;
        [JsonIgnore] public bool shouldPingPong;
        [JsonIgnore] public bool shouldPause;
        [JsonIgnore] public bool isPaused;
        [JsonIgnore] public bool isHaltedByEvent;
        [JsonIgnore] public int loopsFinished;
        [JsonIgnore] public float coveredDistance;
        [JsonIgnore] public float remainingDistance;
        [JsonIgnore] public float pauseStartTime;
        [JsonIgnore] public bool isInitialProcessDone;
    }
}