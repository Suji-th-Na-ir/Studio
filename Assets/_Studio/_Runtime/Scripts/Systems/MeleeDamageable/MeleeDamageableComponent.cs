using System;
using UnityEngine;
using UnityEngine.UI;
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
        public FXData FXData { get; set; }
        public Listen Listen { get; set; }
        [JsonIgnore] public bool CanExecute { get; set; }
        [JsonIgnore] public bool IsExecuted { get; set; }
        [JsonIgnore] public EventContext EventContext { get; set; }
        [JsonIgnore] public GameObject RefObj { get; set; }
        [JsonIgnore] public int currentHealth;

        public int health;
        public bool IsBroadcastableDead;
        public string BroadcastDead;
        [JsonIgnore] public GameObject healthUI;
        [JsonIgnore] public Image healthBar;
    }
}