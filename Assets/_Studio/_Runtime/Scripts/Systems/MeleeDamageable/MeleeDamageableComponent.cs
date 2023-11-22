using System;
using UnityEngine;
using Newtonsoft.Json;

namespace Terra.Studio
{
    [Serializable]
    public struct MeleeDamageableComponent : IBaseComponent
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
        [JsonIgnore] public int currentHealth;
        public int health;
        public bool canPlaySFXHit;
        public string sfxNameHit;
        public int sfxIndexHit;
        public bool canPlayVFXHit;
        public string vfxNameHit;
        public int vfxIndexHit;

        public bool canPlaySFXDead;
        public string sfxNameDead;
        public int sfxIndexDead;
        public bool canPlayVFXDead;
        public string vfxNameDead;
        public int vfxIndexDead;
        public bool IsBroadcastableDead;
        public string BroadcastDead;
    }
}