using System;
using UnityEngine;
using Newtonsoft.Json;

namespace Terra.Studio
{
    [Serializable]
    public struct PlayerHealthComponent : IBaseComponent
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

        public bool regenerateHealth;
        public float regenerationPerSec;
        [JsonIgnore] public bool trackUI;
        [JsonIgnore] public GameObject healthUI;
        [JsonIgnore] public UnityEngine.UI.Image slider;
        [JsonIgnore] public float playerHealth;
    }
}