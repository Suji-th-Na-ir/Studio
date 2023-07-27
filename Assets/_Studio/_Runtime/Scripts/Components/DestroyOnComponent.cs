using Newtonsoft.Json;
using UnityEngine;

namespace Terra.Studio
{
    /// <summary>
    /// Can destroy on following conditions
    /// 1. Listen to broadcast
    /// 2. On player collider
    /// 3. On click
    /// </summary>
    public struct DestroyOnComponent : IBaseComponent, IConditional, IBroadcastData
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
        [JsonIgnore] public GameObject refObj;
        [JsonIgnore] public bool isRegistered;
    }
}