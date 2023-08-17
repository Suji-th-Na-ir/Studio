namespace Terra.Studio
{
    public interface IBaseComponent
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
    }
}
