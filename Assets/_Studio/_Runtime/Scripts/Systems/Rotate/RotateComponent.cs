using System;
using UnityEngine;
using Newtonsoft.Json;

namespace Terra.Studio
{
    [Serializable]
    public struct RotateComponent : IBaseComponent
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
        [JsonIgnore] public EventContext EventContext { get; set; }
        [JsonIgnore] public GameObject RefObj { get; set; }
        [JsonConverter(typeof(Vector3Converter))] public Vector3 expectedRotateBy;
        [JsonConverter(typeof(Vector3Converter))] public Vector3 ghostLastRotation;
        public Axis[] axis;
        public Direction direction;
        public RotationType rotationType;
        public RepeatType repeatType;
        public BroadcastAt broadcastAt;
        public Listen listen;
        public float speed;
        public float pauseFor;
        public int repeatFor;
        public bool canPlaySFX;
        public string sfxName;
        public int sfxIndex;
        public bool canPlayVFX;
        public string vfxName;
        public int vfxIndex;
        public int listenIndex;
        public int broadcastTypeIndex;

        [JsonIgnore] public int currentRotateCount;
        [JsonIgnore] public int directionFactor;
        [JsonIgnore] public float pauseStartTime;
        [JsonIgnore] public bool canPause;
        [JsonIgnore] public bool isPaused;
        [JsonIgnore] public bool shouldPingPong;
        [JsonIgnore] public bool rotateForever;
        [JsonIgnore] public bool isHaltedByEvent;
        [JsonIgnore] public float xTargetRotation;
        [JsonIgnore] public float yTargetRotation;
        [JsonIgnore] public float zTargetRotation;
        [JsonIgnore] public float xCurrentRotation;
        [JsonIgnore] public float yCurrentRotation;
        [JsonIgnore] public float zCurrentRotation;
    }
}