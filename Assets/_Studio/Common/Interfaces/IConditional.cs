using System;

namespace Terra.Studio
{
    public interface IConditional
    {
        public bool IsConditionAvailable { get; set; }
        public string ConditionType { get; set; }
        public string ConditionData { get; set; }
    }
}
