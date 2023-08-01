using System;
using UnityEngine;
using Newtonsoft.Json;

namespace Terra.Studio
{
    [Serializable]
    public struct CollectableComponent : IBaseComponent, IConditional, IBroadcastData
    {
        public bool CanExecute { get; set; }
        public bool IsExecuted { get; set; }
        public bool IsConditionAvailable { get; set; }
        public string ConditionType { get; set; }
        public string ConditionData { get; set; }
        public bool IsBroadcastable { get; set; }
        public string Broadcast { get; set; }
        public bool IsTargeted { get; set; }
        public int TargetId { get; set; }
        public bool canPlaySFX;
        public string sfxName;
        public int sfxIndex;
        public bool canPlayVFX;
        public string vfxName;
        public int vfxIndex;
        public bool canUpdateScore;
        public float scoreValue;
        public bool showScoreUI;
        [JsonIgnore] public GameObject refObject;
        [JsonIgnore] public bool isRegistered;
    }
}