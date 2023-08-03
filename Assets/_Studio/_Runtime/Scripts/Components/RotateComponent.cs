using System;
using UnityEngine;
using Newtonsoft.Json;
using static Terra.Studio.GlobalEnums;

namespace Terra.Studio
{
    [Serializable]
    public struct RotateComponent : IBaseComponent, IConditional, IBroadcastData
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

        public Axis axis;
        public Direction direction;
        public RotationType rotationType;
        public RepeatType repeatType;
        public BroadcastAt broadcastAt;
        public float speed;
        public float rotateBy;
        public float pauseFor;
        public int repeatFor;
        public bool canPlaySFX;
        public string sfxName;
        public int sfxIndex;
        public bool canPlayVFX;
        public string vfxName;
        public int vfxIndex;
    }
}