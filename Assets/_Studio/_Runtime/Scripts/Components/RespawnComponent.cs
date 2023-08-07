using System;
using UnityEngine;
using Newtonsoft.Json;

namespace Terra.Studio
{
    [Serializable]
    public struct RespawnComponent : IBaseComponent, IConditional, IBroadcastData
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
        public bool canPlayVFX;
        public string sfxName;
        public string vfxName;
        public int sfxIndex;
        public int vfxIndex;
    }
}