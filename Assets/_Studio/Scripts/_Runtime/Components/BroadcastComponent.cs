using System;

namespace Terra.Studio
{
    [Serializable]
    public struct BroadcastComponent : IBaseComponent, IBroadcastData, IConditional
    {
        public bool IsBroadcastable { get; set; }
        public string Broadcast { get; set; }
        public bool IsTargeted { get; set; }
        public bool CanExecute { get; set; }
        public bool IsExecuted { get; set; }
        public bool IsConditionAvailable { get; set; }
        public string ConditionType { get; set; }
        public string ConditionData { get; set; }
    }
}